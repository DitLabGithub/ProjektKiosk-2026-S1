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
    [SerializeField] private int maxScore = 80;
    [SerializeField] private int currentScore = 0;

    [Header("UI Display")]
    [Tooltip("TextMeshPro component to display 'Morality Score: X'")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Inbox Message Thresholds")]
    [Tooltip("Corruption thresholds that trigger inbox warning messages (e.g., 15, 30, 45)")]
    [SerializeField] private List<InboxMessageThreshold> inboxThresholds = new List<InboxMessageThreshold>();

    [Header("Police Scenarios")]
    [Tooltip("Add police scenarios here. Each has a threshold and JSON file name. Expandable for future scenarios.")]
    [SerializeField] private List<PoliceScenarioThreshold> policeScenarios = new List<PoliceScenarioThreshold>();

    // Events for when score changes and thresholds are reached
    public event Action<int> OnScoreChanged;
    public event Action<InboxMessageThreshold> OnInboxThresholdReached;
    public event Action<PoliceScenarioThreshold> OnPoliceThresholdReached;

    [System.Serializable]
    public class InboxMessageThreshold
    {
        [Tooltip("Display name for this message (e.g., 'Minor Irregularities', 'Official Warning')")]
        public string messageName = "Warning Message";

        [Tooltip("Score needed to trigger this inbox message")]
        public int scoreThreshold = 15;

        [Tooltip("Sender name (e.g., 'NEU Compliance Office')")]
        public string sender = "NEU Compliance Office";

        [Tooltip("Message subject line")]
        public string subject = "Minor Irregularities Detected";

        [Tooltip("Full message content")]
        [TextArea(3, 10)]
        public string content = "We've noticed some minor irregularities in your transaction logs. Please ensure all protocols are followed.";

        [Tooltip("Message type (Info, Warning, Alert, Critical)")]
        public MessageType messageType = MessageType.Warning;

        [HideInInspector]
        public bool triggered = false;
    }

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
        // Sort inbox thresholds by score for proper triggering order
        inboxThresholds.Sort((a, b) => a.scoreThreshold.CompareTo(b.scoreThreshold));

        // Sort police scenarios by threshold for proper triggering order
        policeScenarios.Sort((a, b) => a.scoreThreshold.CompareTo(b.scoreThreshold));

        Debug.Log($"[ScoreManager] ===== SCORE SYSTEM INITIALIZED =====");
        Debug.Log($"[ScoreManager] Current Score: {currentScore}/{maxScore}");

        Debug.Log($"[ScoreManager] Inbox message thresholds configured: {inboxThresholds.Count}");
        foreach (var inbox in inboxThresholds)
        {
            Debug.Log($"  📧 {inbox.messageName} @ {inbox.scoreThreshold} points → {inbox.messageType}");

            // Validate threshold is positive and within range
            if (inbox.scoreThreshold <= 0 || inbox.scoreThreshold > maxScore)
            {
                Debug.LogWarning($"  ⚠️ WARNING: '{inbox.messageName}' has invalid threshold ({inbox.scoreThreshold})! Should be 1-{maxScore}");
            }
        }

        Debug.Log($"[ScoreManager] Police scenarios configured: {policeScenarios.Count}");
        foreach (var scenario in policeScenarios)
        {
            Debug.Log($"  🚨 {scenario.scenarioName} @ {scenario.scoreThreshold} points → {scenario.jsonFileName}");

            // Validate threshold is positive
            if (scenario.scoreThreshold <= 0)
            {
                Debug.LogWarning($"  ⚠️ WARNING: '{scenario.scenarioName}' has invalid threshold ({scenario.scoreThreshold})! Should be > 0");
            }
        }

        // Initialize UI
        UpdateScoreDisplay();

        // TEST: Display key shortcuts for testing
        Debug.Log("[ScoreManager] ===== TEST MODE ENABLED =====");
        Debug.Log("[ScoreManager] Press number keys to test thresholds:");
        Debug.Log("  1 = Set score to 15 (Inbox Message 1)");
        Debug.Log("  2 = Set score to 30 (Inbox Message 2)");
        Debug.Log("  3 = Set score to 45 (Inbox Message 3)");
        Debug.Log("  4 = Set score to 50 (Police Warning)");
        Debug.Log("  5 = Set score to 70 (Police Arrest)");
        Debug.Log("  0 = Reset score to 0");
    }

    /// <summary>
    /// TEST METHOD: Press number keys to manually trigger thresholds
    /// Remove this method after testing is complete
    /// </summary>
    private void Update()
    {
        // TEST: Manual score control with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            Debug.Log("[ScoreManager] TEST: Setting score to 15");
            SetScore(15);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            Debug.Log("[ScoreManager] TEST: Setting score to 30");
            SetScore(30);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            Debug.Log("[ScoreManager] TEST: Setting score to 45");
            SetScore(45);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            Debug.Log("[ScoreManager] TEST: Setting score to 50");
            SetScore(50);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
        {
            Debug.Log("[ScoreManager] TEST: Setting score to 70");
            SetScore(70);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0))
        {
            Debug.Log("[ScoreManager] TEST: Resetting score to 0");
            ResetScore();
        }
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
    /// Checks if any inbox message or police scenario thresholds have been crossed
    /// </summary>
    private void CheckThresholds(int previousScore, int newScore)
    {
        Debug.Log($"[ScoreManager] CheckThresholds: {previousScore} → {newScore}");

        // Check inbox message thresholds
        foreach (var inboxThreshold in inboxThresholds)
        {
            // Check if this message hasn't been triggered yet and threshold was crossed
            bool notTriggeredYet = !inboxThreshold.triggered;
            bool wasBelow = previousScore < inboxThreshold.scoreThreshold;
            bool isNowAbove = newScore >= inboxThreshold.scoreThreshold;

            if (notTriggeredYet && wasBelow && isNowAbove)
            {
                inboxThreshold.triggered = true;
                Debug.Log($"[ScoreManager] 📧 INBOX TRIGGER: '{inboxThreshold.messageName}' @ {inboxThreshold.scoreThreshold} points!");
                Debug.Log($"[ScoreManager]   → Sending message to InboxUIManager: {inboxThreshold.subject}");

                // Notify InboxUIManager to show this message
                OnInboxThresholdReached?.Invoke(inboxThreshold);
            }
        }

        // Check police scenario thresholds
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
    /// Gets all inbox message thresholds (for inspector viewing or debugging)
    /// </summary>
    public List<InboxMessageThreshold> GetInboxThresholds()
    {
        return inboxThresholds;
    }

    /// <summary>
    /// Gets all police scenarios (for inspector viewing or debugging)
    /// </summary>
    public List<PoliceScenarioThreshold> GetPoliceScenarios()
    {
        return policeScenarios;
    }

    /// <summary>
    /// Checks if a specific inbox message has been triggered by name
    /// </summary>
    public bool IsInboxMessageTriggered(string messageName)
    {
        foreach (var inbox in inboxThresholds)
        {
            if (inbox.messageName == messageName)
            {
                return inbox.triggered;
            }
        }
        return false;
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

        // Reset all inbox threshold triggered flags
        foreach (var inboxThreshold in inboxThresholds)
        {
            inboxThreshold.triggered = false;
        }

        // Reset all police scenario triggered flags
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
