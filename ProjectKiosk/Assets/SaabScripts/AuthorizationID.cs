using UnityEngine;

/// <summary>
/// Component for IDs that require authorization (like Owner ID).
/// Has NO UI references - delegates to AuthorizationUIManager.
/// </summary>
public class AuthorizationID : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Duration of the authorization process in seconds")]
    public float loadingDuration = 5f;

    [Header("Status (Read-Only)")]
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
        Denied
    }

    void Awake()
    {
        customerID = GetComponent<CustomerID>();

        if (customerID == null)
        {
            Debug.LogError("[AuthorizationID] ERROR: CustomerID component NOT FOUND!");
        }
        else
        {
            Debug.Log("[AuthorizationID] CustomerID component found");
        }

        // Mark as authorization ID
        if (customerID != null)
        {
            customerID.isAuthorizationID = true;
            customerID.authorizationStatus = "Pending";
        }
    }

    /// <summary>
    /// Starts the authorization process
    /// </summary>
    public void StartAuthorization()
    {
        Debug.Log($"[AuthorizationID] ===== StartAuthorization() called! Current status: {status} =====");

        if (status == AuthorizationStatus.Loading || status == AuthorizationStatus.Authorized)
        {
            Debug.LogWarning($"[AuthorizationID] Already {status}! Ignoring.");
            return;
        }

        // Check if UIManager exists
        if (AuthorizationUIManager.Instance == null)
        {
            Debug.LogError("[AuthorizationID] ERROR: AuthorizationUIManager.Instance is NULL! Cannot show authorization UI!");
            Debug.LogError("[AuthorizationID] Make sure AuthorizationUIManager component exists in the scene!");

            // Still complete the authorization (just without UI)
            CompleteAuthorization();
            return;
        }

        status = AuthorizationStatus.Loading;

        // Update CustomerID status
        if (customerID != null)
        {
            customerID.authorizationStatus = "Verifying...";
            Debug.Log("[AuthorizationID] CustomerID status updated to 'Verifying...'");
        }

        // Notify that authorization started
        Debug.Log("[AuthorizationID] Invoking OnAuthorizationStarted event...");
        OnAuthorizationStarted?.Invoke();

        // Tell UI Manager to show authorization UI
        Debug.Log("[AuthorizationID] Calling AuthorizationUIManager.StartAuthorization()...");
        AuthorizationUIManager.Instance.StartAuthorization(OnAuthorizationAnimationComplete);
    }

    /// <summary>
    /// Called when the UI animation completes
    /// </summary>
    private void OnAuthorizationAnimationComplete()
    {
        Debug.Log("[AuthorizationID] UI animation complete, finalizing authorization...");
        CompleteAuthorization();
    }

    /// <summary>
    /// Marks authorization as complete
    /// </summary>
    private void CompleteAuthorization()
    {
        Debug.Log("[AuthorizationID] ===== CompleteAuthorization() called! =====");
        status = AuthorizationStatus.Authorized;

        // Update CustomerID status
        if (customerID != null)
        {
            customerID.authorizationStatus = "Authorized ✓";
            Debug.Log("[AuthorizationID] CustomerID status updated to 'Authorized ✓'");
        }

        // Notify that authorization is complete
        Debug.Log("[AuthorizationID] Invoking OnAuthorizationCompleted event...");
        OnAuthorizationCompleted?.Invoke();

        Debug.Log("[AuthorizationID] ===== Authorization completed successfully! =====");
    }

    /// <summary>
    /// Resets the authorization state
    /// </summary>
    public void ResetAuthorization()
    {
        status = AuthorizationStatus.Pending;

        if (customerID != null)
        {
            customerID.authorizationStatus = "Pending";
        }

        Debug.Log("[AuthorizationID] Authorization reset");
    }

    /// <summary>
    /// Check if authorization is complete
    /// </summary>
    public bool IsAuthorized()
    {
        return status == AuthorizationStatus.Authorized;
    }
}
