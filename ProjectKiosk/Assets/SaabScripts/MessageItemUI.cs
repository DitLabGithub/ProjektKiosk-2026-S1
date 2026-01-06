using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the visual display of a single message item in the inbox list.
/// Updates colors, text, and indicators based on message data.
/// </summary>
public class MessageItemUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI senderText;
    public TextMeshProUGUI subjectText;
    public TextMeshProUGUI timestampText;
    public Image typeIcon;
    public TextMeshProUGUI iconText;
    public GameObject unreadIndicator;
    public Image backgroundImage;

    // Color schemes for different message types
    private static readonly Color InfoColor = new Color(0.2f, 0.6f, 1f); // Blue
    private static readonly Color WarningColor = new Color(1f, 0.8f, 0f); // Yellow/Orange
    private static readonly Color AlertColor = new Color(1f, 0.55f, 0f); // Orange
    private static readonly Color CriticalColor = new Color(1f, 0.2f, 0.2f); // Red

    /// <summary>
    /// Sets up the message item UI with provided message data
    /// </summary>
    public void Setup(MessageData data)
    {
        if (data == null)
        {
            Debug.LogError("[MessageItemUI] Cannot setup with null MessageData");
            return;
        }

        // Set text fields
        if (senderText != null)
        {
            senderText.text = data.sender;
        }

        if (subjectText != null)
        {
            subjectText.text = data.subject;
        }

        if (timestampText != null)
        {
            timestampText.text = data.timestamp;
        }

        // Set message type icon and color
        Color typeColor = GetColorForMessageType(data.messageType);
        string iconSymbol = GetIconForMessageType(data.messageType);

        if (typeIcon != null)
        {
            typeIcon.color = typeColor;
        }

        if (iconText != null)
        {
            iconText.text = iconSymbol;
        }

        // Show/hide unread indicator
        if (unreadIndicator != null)
        {
            unreadIndicator.SetActive(!data.isRead);
        }

        // Adjust background opacity if read
        if (backgroundImage != null && data.isRead)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = 0.6f; // Slightly transparent for read messages
            backgroundImage.color = bgColor;
        }

        Debug.Log($"[MessageItemUI] Setup complete for message: '{data.subject}'");
    }

    /// <summary>
    /// Returns the appropriate color for a message type
    /// </summary>
    private Color GetColorForMessageType(MessageType type)
    {
        switch (type)
        {
            case MessageType.Info:
                return InfoColor;
            case MessageType.Warning:
                return WarningColor;
            case MessageType.Alert:
                return AlertColor;
            case MessageType.Critical:
                return CriticalColor;
            default:
                return Color.white;
        }
    }

    /// <summary>
    /// Returns the appropriate icon symbol for a message type
    /// </summary>
    private string GetIconForMessageType(MessageType type)
    {
        switch (type)
        {
            case MessageType.Info:
                return "i"; // Info symbol
            case MessageType.Warning:
                return "!"; // Warning symbol
            case MessageType.Alert:
                return "!!"; // Alert symbol
            case MessageType.Critical:
                return "!!!"; // Critical symbol
            default:
                return "•";
        }
    }

    /// <summary>
    /// Marks this message as read (hides unread indicator)
    /// </summary>
    public void MarkAsRead()
    {
        if (unreadIndicator != null)
        {
            unreadIndicator.SetActive(false);
        }

        // Make background slightly transparent
        if (backgroundImage != null)
        {
            Color bgColor = backgroundImage.color;
            bgColor.a = 0.6f;
            backgroundImage.color = bgColor;
        }

        Debug.Log("[MessageItemUI] Message marked as read");
    }
}
