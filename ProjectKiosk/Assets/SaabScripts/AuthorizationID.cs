using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AuthorizationID : MonoBehaviour
{
    [Header("Authorization Settings")]
    [Tooltip("Duration of the authorization loading process in seconds")]
    public float loadingDuration = 5f;

    [Header("UI References")]
    [Tooltip("Loading bar UI element (Image with fillAmount or Slider)")]
    public Image loadingBar;

    [Tooltip("Checkmark icon that appears when authorized")]
    public GameObject checkmarkIcon;

    [Tooltip("Optional loading text (e.g., 'Verifying Authorization...')")]
    public TMPro.TextMeshProUGUI loadingText;

    [Header("Status")]
    public AuthorizationStatus status = AuthorizationStatus.Pending;

    // Events for DialogueManager to subscribe to
    public System.Action OnAuthorizationStarted;
    public System.Action OnAuthorizationCompleted;

    private CustomerID customerID;

    public enum AuthorizationStatus
    {
        Pending,
        Loading,
        Authorized,
        Denied  // For future use
    }

    void Awake()
    {
        customerID = GetComponent<CustomerID>();

        if (customerID == null)
        {
            Debug.LogError("[AuthorizationID] ERROR: CustomerID component NOT FOUND on this GameObject!");
        }
        else
        {
            Debug.Log("[AuthorizationID] CustomerID component found successfully");
        }

        // Debug: Log what references we have
        Debug.Log($"[AuthorizationID] Awake - LoadingBar: {(loadingBar != null ? "ASSIGNED ✓" : "NULL ✗")}");
        Debug.Log($"[AuthorizationID] Awake - Checkmark: {(checkmarkIcon != null ? "ASSIGNED ✓" : "NULL ✗")}");
        Debug.Log($"[AuthorizationID] Awake - LoadingText: {(loadingText != null ? "ASSIGNED ✓" : "NULL (optional)")}");

        // Hide UI elements initially
        if (loadingBar != null)
        {
            loadingBar.fillAmount = 0f;
            loadingBar.gameObject.SetActive(false);
            Debug.Log("[AuthorizationID] Loading bar initialized and hidden");
        }
        else
        {
            Debug.LogWarning("[AuthorizationID] WARNING: Loading bar is NOT assigned in Inspector!");
        }

        if (checkmarkIcon != null)
        {
            checkmarkIcon.SetActive(false);
            Debug.Log("[AuthorizationID] Checkmark initialized and hidden");
        }
        else
        {
            Debug.LogWarning("[AuthorizationID] WARNING: Checkmark is NOT assigned in Inspector!");
        }

        if (loadingText != null)
            loadingText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Starts the authorization process with loading animation
    /// </summary>
    public void StartAuthorization()
    {
        Debug.Log($"[AuthorizationID] ===== StartAuthorization() called! Current status: {status} =====");

        if (status == AuthorizationStatus.Loading || status == AuthorizationStatus.Authorized)
        {
            Debug.LogWarning($"[AuthorizationID] Authorization already in progress or completed! Status: {status}");
            return;
        }

        Debug.Log("[AuthorizationID] Starting authorization coroutine...");
        StartCoroutine(AuthorizationLoadingCoroutine());
    }

    private IEnumerator AuthorizationLoadingCoroutine()
    {
        Debug.Log("[AuthorizationID] Coroutine started!");
        status = AuthorizationStatus.Loading;

        // Update CustomerID authorization status
        if (customerID != null)
        {
            customerID.isAuthorizationID = true;
            customerID.authorizationStatus = "Verifying...";
            Debug.Log("[AuthorizationID] CustomerID status updated to 'Verifying...'");
        }

        // Show loading UI
        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(true);
            loadingBar.fillAmount = 0f;
            Debug.Log("[AuthorizationID] Loading bar activated and set to 0");
        }
        else
        {
            Debug.LogError("[AuthorizationID] ERROR: LoadingBar is NULL! Cannot show loading animation!");
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "Verifying Authorization...";
            Debug.Log("[AuthorizationID] Loading text shown");
        }

        // Notify DialogueManager that authorization started
        Debug.Log("[AuthorizationID] Invoking OnAuthorizationStarted event...");
        OnAuthorizationStarted?.Invoke();

        // Animate loading bar over the specified duration
        Debug.Log($"[AuthorizationID] Starting {loadingDuration}s loading animation...");
        float elapsed = 0f;
        while (elapsed < loadingDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / loadingDuration;

            if (loadingBar != null)
            {
                loadingBar.fillAmount = progress;
            }

            yield return null;
        }

        Debug.Log("[AuthorizationID] Loading animation complete!");

        // Ensure loading bar is fully filled
        if (loadingBar != null)
        {
            loadingBar.fillAmount = 1f;
        }

        // Wait a brief moment for visual feedback
        yield return new WaitForSeconds(0.3f);

        // Hide loading elements
        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(false);
            Debug.Log("[AuthorizationID] Loading bar hidden");
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        // Show checkmark and mark as authorized
        Debug.Log("[AuthorizationID] Calling CompleteAuthorization()...");
        CompleteAuthorization();
    }

    private void CompleteAuthorization()
    {
        Debug.Log("[AuthorizationID] ===== CompleteAuthorization() called! =====");
        status = AuthorizationStatus.Authorized;

        // Update CustomerID authorization status
        if (customerID != null)
        {
            customerID.authorizationStatus = "Authorized ✓";
            Debug.Log("[AuthorizationID] CustomerID status updated to 'Authorized ✓'");
        }

        // Show checkmark
        if (checkmarkIcon != null)
        {
            checkmarkIcon.SetActive(true);
            Debug.Log("[AuthorizationID] Checkmark icon ACTIVATED!");
        }
        else
        {
            Debug.LogError("[AuthorizationID] ERROR: Checkmark icon is NULL! Cannot show checkmark!");
        }

        // Notify DialogueManager that authorization is complete
        Debug.Log("[AuthorizationID] Invoking OnAuthorizationCompleted event...");
        OnAuthorizationCompleted?.Invoke();

        Debug.Log("[AuthorizationID] ===== Authorization completed successfully! =====");
    }

    /// <summary>
    /// Resets the authorization state (useful for testing or reuse)
    /// </summary>
    public void ResetAuthorization()
    {
        status = AuthorizationStatus.Pending;

        if (customerID != null)
        {
            customerID.authorizationStatus = "Pending";
        }

        if (loadingBar != null)
        {
            loadingBar.fillAmount = 0f;
            loadingBar.gameObject.SetActive(false);
        }

        if (checkmarkIcon != null)
        {
            checkmarkIcon.SetActive(false);
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Check if authorization is complete
    /// </summary>
    public bool IsAuthorized()
    {
        return status == AuthorizationStatus.Authorized;
    }
}
