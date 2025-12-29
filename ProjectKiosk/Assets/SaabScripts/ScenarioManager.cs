using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ScenarioEntry
{
    public string filename;
    public string displayName;
    public string followUpScenario; // Filename of scenario that plays immediately after this one
}

[Serializable]
public class ScenarioConfig
{
    public bool shuffleEnabled;
    public List<ScenarioEntry> scenarios = new List<ScenarioEntry>();
}

public class ScenarioManager : MonoBehaviour
{
    private ScenarioConfig config;
    private Queue<string> scenarioQueue;
    private Dictionary<string, ScenarioEntry> scenarioLookup;
    private string currentScenarioFilename;

    private void Awake()
    {
        LoadConfig();
        InitializeScenarioQueue();
    }

    private void Start()
    {
        // Try to subscribe to score threshold events
        SubscribeToScoreManager();
    }

    private void SubscribeToScoreManager()
    {
        // Subscribe to score threshold events for police scenario injection
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPoliceThresholdReached += OnPoliceThresholdReached;
            Debug.Log("[ScenarioManager] Successfully subscribed to police threshold events");
        }
        else
        {
            Debug.LogWarning("[ScenarioManager] ScoreManager.Instance is null! Will retry...");
            // Retry after a short delay if ScoreManager isn't ready yet
            Invoke(nameof(SubscribeToScoreManager), 0.1f);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPoliceThresholdReached -= OnPoliceThresholdReached;
        }
    }

    private void LoadConfig()
    {
        TextAsset configAsset = Resources.Load<TextAsset>("ScenarioConfig");
        if (configAsset == null)
        {
            Debug.LogError("ScenarioConfig.json not found in Resources folder!");
            config = new ScenarioConfig();
            return;
        }

        try
        {
            config = JsonUtility.FromJson<ScenarioConfig>(configAsset.text);
            Debug.Log($"Loaded ScenarioConfig with {config.scenarios.Count} scenarios. Shuffle: {config.shuffleEnabled}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse ScenarioConfig.json: {e.Message}");
            config = new ScenarioConfig();
        }
    }

    private void InitializeScenarioQueue()
    {
        scenarioQueue = new Queue<string>();
        scenarioLookup = new Dictionary<string, ScenarioEntry>();

        if (config == null || config.scenarios.Count == 0)
        {
            Debug.LogWarning("No scenarios to load!");
            return;
        }

        // Build lookup dictionary for quick access
        foreach (var scenario in config.scenarios)
        {
            scenarioLookup[scenario.filename] = scenario;
        }

        // Get list of scenario filenames
        List<string> scenarioFilenames = config.scenarios.Select(s => s.filename).ToList();

        // Shuffle if enabled
        if (config.shuffleEnabled)
        {
            ShuffleList(scenarioFilenames);
            Debug.Log($"Shuffled scenario order: {string.Join(", ", scenarioFilenames)}");
        }

        // Add to queue
        foreach (string filename in scenarioFilenames)
        {
            scenarioQueue.Enqueue(filename);
        }
    }

    // Fisher-Yates shuffle algorithm
    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public bool HasMoreScenarios()
    {
        bool hasMore = scenarioQueue.Count > 0;
        Debug.Log($"[ScenarioManager] HasMoreScenarios() called. Queue count: {scenarioQueue.Count}, Returning: {hasMore}");
        return hasMore;
    }

    public string GetNextScenarioFilename()
    {
        if (scenarioQueue.Count == 0)
        {
            Debug.LogWarning("No more scenarios in queue!");
            return null;
        }

        currentScenarioFilename = scenarioQueue.Dequeue();
        Debug.Log($"Next scenario: {currentScenarioFilename}");
        return currentScenarioFilename;
    }

    public void OnScenarioComplete(string completedScenarioFilename)
    {
        Debug.Log($"Scenario completed: {completedScenarioFilename}");

        // Check if this scenario has a follow-up
        if (scenarioLookup.TryGetValue(completedScenarioFilename, out ScenarioEntry entry))
        {
            if (!string.IsNullOrEmpty(entry.followUpScenario))
            {
                Debug.Log($"Queueing follow-up scenario: {entry.followUpScenario}");
                // Insert follow-up at the front of the queue so it plays next
                Queue<string> tempQueue = new Queue<string>();
                tempQueue.Enqueue(entry.followUpScenario);

                // Add remaining scenarios after the follow-up
                while (scenarioQueue.Count > 0)
                {
                    tempQueue.Enqueue(scenarioQueue.Dequeue());
                }

                scenarioQueue = tempQueue;
            }
        }
    }

    public int GetTotalScenarioCount()
    {
        return config != null ? config.scenarios.Count : 0;
    }

    public int GetRemainingScenarioCount()
    {
        return scenarioQueue.Count;
    }

    /// <summary>
    /// Called when a police scenario threshold is reached. Injects the police scenario to play next.
    /// </summary>
    private void OnPoliceThresholdReached(ScoreManager.PoliceScenarioThreshold policeScenario)
    {
        if (string.IsNullOrEmpty(policeScenario.jsonFileName))
        {
            Debug.LogWarning($"[ScenarioManager] Police scenario '{policeScenario.scenarioName}' has no JSON filename!");
            return;
        }

        Debug.Log($"[ScenarioManager] Injecting police scenario: {policeScenario.jsonFileName} (Threshold: {policeScenario.scoreThreshold})");

        // Insert police scenario at the front of the queue so it plays immediately after current scenario
        Queue<string> tempQueue = new Queue<string>();
        tempQueue.Enqueue(policeScenario.jsonFileName);

        // Add remaining scenarios after the police scenario
        while (scenarioQueue.Count > 0)
        {
            tempQueue.Enqueue(scenarioQueue.Dequeue());
        }

        scenarioQueue = tempQueue;

        Debug.Log($"[ScenarioManager] Police scenario queued. Next scenario will be: {policeScenario.jsonFileName}");
        Debug.Log($"[ScenarioManager] Queue now has {scenarioQueue.Count} scenarios remaining");
    }
}
