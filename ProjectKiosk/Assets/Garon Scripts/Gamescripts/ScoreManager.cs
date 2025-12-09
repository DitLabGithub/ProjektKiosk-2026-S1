using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages the morality/corruption score system that tracks player choices throughout the game.
/// Score accumulates based on dialogue responses and triggers police scenarios at thresholds.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    [SerializeField] private int maxScore = 30;
    [SerializeField] private int currentScore = 0;

    [Header("UI Display")]
    [Tooltip("TextMeshPro component to display 'Morality Score: X'")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Police Scenarios")]
    [Tooltip("Add police scenarios here. Each has a threshold and JSON file name. Expandable for future scenarios.")]
    [SerializeField] private List<PoliceScenarioThreshold> policeScenarios = new List<PoliceScenarioThreshold>();

    // Events for when score changes and thresholds are reached
    public event Action<int> OnScoreChanged;
    public event Action<PoliceScenarioThreshold> OnPoliceThresholdReached;

    [System.Serializable]
    public class PoliceScenarioThreshold
    {
        [Tooltip("Display name for this police scenario")]
        public string scenarioName = "Police Visit";

        [Tooltip("Score needed to trigger this scenario")]
        public int scoreThreshold = 15;

        [Tooltip("JSON file name in Resources folder (e.g., 'PoliceWarningScenario')")]
        public string jsonFileName = "";

        [HideInInspector]
        public bool triggered = false;
    }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[ScoreManager] Instance created and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.LogWarning("[ScoreManager] Duplicate instance detected and destroyed");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Sort police scenarios by threshold for proper triggering order
        policeScenarios.Sort((a, b) => a.scoreThreshold.CompareTo(b.scoreThreshold));

        Debug.Log($"[ScoreManager] ===== SCORE SYSTEM INITIALIZED =====");
        Debug.Log($"[ScoreManager] Current Score: {currentScore}/{maxScore}");
        Debug.Log($"[ScoreManager] Police scenarios configured: {policeScenarios.Count}");
        foreach (var scenario in policeScenarios)
        {
            Debug.Log($"  ✓ {scenario.scenarioName} @ {scenario.scoreThreshold} points → {scenario.jsonFileName}");

            // Validate threshold is positive
            if (scenario.scoreThreshold <= 0)
            {
                Debug.LogWarning($"  ⚠️ WARNING: '{scenario.scenarioName}' has invalid threshold ({scenario.scoreThreshold})! Should be > 0");
            }
        }

        // Initialize UI
        UpdateScoreDisplay();
    }

    /// <summary>
    /// Adds points to the corruption score and checks for threshold triggers
    /// </summary>
    public void AddScore(int points)
    {
        if (points <= 0) return;

        int previousScore = currentScore;
        currentScore = Mathf.Min(currentScore + points, maxScore);

        Debug.Log($"[ScoreManager] Score increased by {points}. Total: {currentScore}/{maxScore}");

        // Update UI
        UpdateScoreDisplay();

        // Notify listeners of score change
        OnScoreChanged?.Invoke(currentScore);

        // Check for threshold crossings
        CheckThresholds(previousScore, currentScore);
    }

    /// <summary>
    /// Updates the score text UI
    /// </summary>
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Morality Score: {currentScore}";
        }
    }

    /// <summary>
    /// Checks if any police scenario thresholds have been crossed
    /// </summary>
    private void CheckThresholds(int previousScore, int newScore)
    {
        Debug.Log($"[ScoreManager] CheckThresholds: {previousScore} → {newScore}");

        foreach (var policeScenario in policeScenarios)
        {
            // Check if this scenario hasn't been triggered yet and threshold was crossed
            bool notTriggeredYet = !policeScenario.triggered;
            bool wasBelow = previousScore < policeScenario.scoreThreshold;
            bool isNowAbove = newScore >= policeScenario.scoreThreshold;

            if (notTriggeredYet && wasBelow && isNowAbove)
            {
                policeScenario.triggered = true;
                Debug.Log($"[ScoreManager] 🚨 POLICE TRIGGER: '{policeScenario.scenarioName}' @ {policeScenario.scoreThreshold} points!");
                Debug.Log($"[ScoreManager]   → Injecting scenario: {policeScenario.jsonFileName}");

                // Notify ScenarioManager to inject this police scenario
                OnPoliceThresholdReached?.Invoke(policeScenario);
            }
        }
    }

    /// <summary>
    /// Gets the current corruption score
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// Gets the maximum possible score
    /// </summary>
    public int GetMaxScore()
    {
        return maxScore;
    }

    /// <summary>
    /// Gets all police scenarios (for inspector viewing or debugging)
    /// </summary>
    public List<PoliceScenarioThreshold> GetPoliceScenarios()
    {
        return policeScenarios;
    }

    /// <summary>
    /// Checks if a specific police scenario has been triggered by name
    /// </summary>
    public bool IsPoliceScenarioTriggered(string scenarioName)
    {
        foreach (var scenario in policeScenarios)
        {
            if (scenario.scenarioName == scenarioName)
            {
                return scenario.triggered;
            }
        }
        return false;
    }

    /// <summary>
    /// Resets the score system (for new game or testing)
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;

        // Reset all triggered flags
        foreach (var policeScenario in policeScenarios)
        {
            policeScenario.triggered = false;
        }

        Debug.Log("[ScoreManager] Score system reset.");
        UpdateScoreDisplay();
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Debug method to manually set score for testing
    /// </summary>
    public void SetScore(int score)
    {
        int previousScore = currentScore;
        currentScore = Mathf.Clamp(score, 0, maxScore);
        Debug.Log($"[ScoreManager] Score manually set to {currentScore}/{maxScore}");
        UpdateScoreDisplay();
        OnScoreChanged?.Invoke(currentScore);
        CheckThresholds(previousScore, currentScore);
    }

    /// <summary>
    /// Assign the score text UI at runtime (called from DialogueManager or scene setup)
    /// </summary>
    public void SetScoreText(TextMeshProUGUI textComponent)
    {
        scoreText = textComponent;
        UpdateScoreDisplay();
    }
}
