using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the inbox UI overlay where players can read warning messages and notifications.
/// Messages appear when corruption score crosses certain thresholds.
/// </summary>
public class InboxUIManager : MonoBehaviour
{
    public static InboxUIManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("The mail icon button in top-right corner")]
    public Button mailIconButton;

    [Tooltip("Red notification badge that appears when there are unread messages")]
    public GameObject notificationBadge;

    [Tooltip("Text showing number of unread messages (e.g., '!' or '3')")]
    public TextMeshProUGUI badgeText;

    [Tooltip("Full inbox overlay panel (entire screen darkened)")]
    public GameObject inboxPanel;

    [Tooltip("Close button to exit inbox")]
    public Button closeButton;

    [Tooltip("Content container where message items will be instantiated")]
    public Transform messageListContent;

    [Header("Message Prefab (will create later)")]
    [Tooltip("Prefab for individual message items in the list")]
    public GameObject messageItemPrefab;

    [Header("Audio (optional)")]
    public AudioClip messageReceivedSound;

    // Internal state
    private List<MessageData> messages = new List<MessageData>();
    private int unreadCount = 0;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[InboxUIManager] Instance created");
        }
        else
        {
            Debug.LogWarning("[InboxUIManager] Duplicate instance detected and destroyed");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Wire up button events
        if (mailIconButton != null)
        {
            mailIconButton.onClick.AddListener(OpenInbox);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseInbox);
        }

        // Ensure inbox is hidden at start
        if (inboxPanel != null)
        {
            inboxPanel.SetActive(false);
        }

        // Hide notification badge initially
        UpdateNotificationBadge();

        Debug.Log("[InboxUIManager] Initialized successfully");

        // Subscribe to ScoreManager inbox threshold events
        SubscribeToScoreManager();

        // TEST: Add sample messages for testing (comment out after Phase 2 testing)
        // AddTestMessages();
    }

    /// <summary>
    /// Subscribes to ScoreManager inbox threshold events
    /// </summary>
    private void SubscribeToScoreManager()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnInboxThresholdReached += OnInboxThresholdReached;
            Debug.Log("[InboxUIManager] Successfully subscribed to inbox threshold events");
        }
        else
        {
            Debug.LogWarning("[InboxUIManager] ScoreManager.Instance is null! Will retry...");
            // Retry after a short delay if ScoreManager isn't ready yet
            Invoke(nameof(SubscribeToScoreManager), 0.1f);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnInboxThresholdReached -= OnInboxThresholdReached;
        }
    }

    /// <summary>
    /// Called when a corruption threshold is reached. Adds the corresponding message to the inbox.
    /// </summary>
    private void OnInboxThresholdReached(ScoreManager.InboxMessageThreshold threshold)
    {
        Debug.Log($"[InboxUIManager] Inbox threshold reached: '{threshold.messageName}' @ {threshold.scoreThreshold} points");

        // Add the message from the threshold data
        AddMessage(
            threshold.sender,
            threshold.subject,
            threshold.content,
            threshold.messageType
        );
    }

    /// <summary>
    /// TEST METHOD: Adds sample messages for testing inbox functionality
    /// Remove this method once integrated with ScoreManager
    /// </summary>
    private void AddTestMessages()
    {
        // Soft warning
        AddMessage(
            "NEU Compliance Office",
            "Minor Irregularities Detected",
            "Hello, this is NEU Compliance Office. We've noticed some minor irregularities in your transaction logs. Please ensure all protocols are followed.\n\n- Automated Message",
            MessageType.Warning
        );

        // Official warning
        AddMessage(
            "NEU Data Protection Agency",
            "ATTENTION: Kiosk Flagged for Review",
            "Your kiosk has been flagged for review. Suspicious activity detected. An officer may visit for routine inspection. Ensure all records are compliant.\n\n- NEU Data Protection Agency",
            MessageType.Alert
        );

        // Final warning
        AddMessage(
            "Lead Investigator Larry Beige",
            "URGENT: Active Investigation",
            "Multiple violations detected. You are now under active investigation. Any further infractions will result in immediate enforcement action.\n\nThis is your final warning.\n\n- Lead Investigator Larry Beige\nNEU Police Department",
            MessageType.Critical
        );

        Debug.Log("[InboxUIManager] Test messages added for demonstration");
    }

    /// <summary>
    /// Opens the inbox overlay panel
    /// </summary>
    public void OpenInbox()
    {
        if (inboxPanel != null)
        {
            inboxPanel.SetActive(true);
            Debug.Log("[InboxUIManager] Inbox opened");

            // Play button click sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayGenericButtonClick();
            }

            // Mark all messages as read when inbox is opened
            MarkAllMessagesAsRead();
        }
    }

    /// <summary>
    /// Closes the inbox overlay panel
    /// </summary>
    public void CloseInbox()
    {
        if (inboxPanel != null)
        {
            inboxPanel.SetActive(false);
            Debug.Log("[InboxUIManager] Inbox closed");

            // Play button click sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayGenericButtonClick();
            }
        }
    }

    /// <summary>
    /// Adds a new message to the inbox
    /// </summary>
    public void AddMessage(string sender, string subject, string content, MessageType type = MessageType.Warning)
    {
        MessageData newMessage = new MessageData
        {
            sender = sender,
            subject = subject,
            content = content,
            messageType = type,
            isRead = false,
            timestamp = System.DateTime.Now.ToString("HH:mm")
        };

        messages.Add(newMessage);
        unreadCount++;

        Debug.Log($"[InboxUIManager] New message added: '{subject}' from {sender}");

        // Update UI
        UpdateNotificationBadge();
        RefreshMessageList();

        // Play notification sound
        if (messageReceivedSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(messageReceivedSound);
        }
    }

    /// <summary>
    /// Updates the red notification badge visibility and count
    /// </summary>
    private void UpdateNotificationBadge()
    {
        if (notificationBadge != null)
        {
            bool hasUnread = unreadCount > 0;
            notificationBadge.SetActive(hasUnread);

            // Update badge text
            if (badgeText != null)
            {
                if (unreadCount > 9)
                {
                    badgeText.text = "9+";
                }
                else
                {
                    badgeText.text = unreadCount.ToString();
                }
            }

            Debug.Log($"[InboxUIManager] Badge updated: {unreadCount} unread messages");
        }
    }

    /// <summary>
    /// Marks all messages as read and clears notification badge
    /// </summary>
    private void MarkAllMessagesAsRead()
    {
        foreach (var message in messages)
        {
            message.isRead = true;
        }

        unreadCount = 0;
        UpdateNotificationBadge();
        RefreshMessageList();
    }

    /// <summary>
    /// Refreshes the message list UI by instantiating message items for each message
    /// </summary>
    private void RefreshMessageList()
    {
        if (messageListContent == null)
        {
            Debug.LogWarning("[InboxUIManager] messageListContent is not assigned");
            return;
        }

        // Clear existing message items
        foreach (Transform child in messageListContent)
        {
            Destroy(child.gameObject);
        }

        // Check if prefab is assigned
        if (messageItemPrefab == null)
        {
            Debug.LogWarning("[InboxUIManager] messageItemPrefab is not assigned. Cannot display messages.");
            return;
        }

        // Instantiate a message item for each message (newest first)
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            MessageData message = messages[i];

            // Instantiate the message item prefab
            GameObject messageItemObj = Instantiate(messageItemPrefab, messageListContent);

            // Get the MessageItemUI component and set it up
            MessageItemUI messageItemUI = messageItemObj.GetComponent<MessageItemUI>();
            if (messageItemUI != null)
            {
                messageItemUI.Setup(message);
            }
            else
            {
                Debug.LogError("[InboxUIManager] MessageItemPrefab is missing MessageItemUI component!");
            }
        }

        Debug.Log($"[InboxUIManager] Message list refreshed. Total messages: {messages.Count}");
    }

    /// <summary>
    /// Gets the total number of messages
    /// </summary>
    public int GetMessageCount()
    {
        return messages.Count;
    }

    /// <summary>
    /// Gets the number of unread messages
    /// </summary>
    public int GetUnreadCount()
    {
        return unreadCount;
    }

    /// <summary>
    /// Clears all messages (for testing or new game)
    /// </summary>
    public void ClearAllMessages()
    {
        messages.Clear();
        unreadCount = 0;
        UpdateNotificationBadge();
        RefreshMessageList();
        Debug.Log("[InboxUIManager] All messages cleared");
    }
}

/// <summary>
/// Data structure for a single message
/// </summary>
[System.Serializable]
public class MessageData
{
    public string sender;
    public string subject;
    public string content;
    public MessageType messageType;
    public bool isRead;
    public string timestamp;
}

/// <summary>
/// Types of messages for color coding and icons
/// </summary>
public enum MessageType
{
    Info,       // Blue - general information
    Warning,    // Yellow/Orange - soft warning
    Alert,      // Orange - official warning
    Critical    // Red - final warning / arrest
}
