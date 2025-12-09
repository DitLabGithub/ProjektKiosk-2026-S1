using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the authorization UI (loading bar and checkmark).
/// Singleton pattern - only one instance in the scene.
/// </summary>
public class AuthorizationUIManager : MonoBehaviour
{
    public static AuthorizationUIManager Instance { get; private set; }

    [Header("UI Elements (Must be in scene hierarchy)")]
    [Tooltip("Loading bar Image - must have Image Type = Filled")]
    public Image loadingBar;

    [Tooltip("Checkmark GameObject to show when authorized")]
    public GameObject checkmark;

    [Tooltip("Optional text to show during loading")]
    public TextMeshProUGUI loadingText;

    [Header("Settings")]
    public float loadingDuration = 5f;

    private Coroutine currentAuthCoroutine;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[AuthorizationUIManager] Multiple instances detected! Destroying duplicate.");
            Destroy(this);
            return;
        }

        Instance = this;
        Debug.Log("[AuthorizationUIManager] Instance created successfully");

        // Validate references
        if (loadingBar == null)
        {
            Debug.LogError("[AuthorizationUIManager] ERROR: Loading Bar is not assigned!");
        }
        else
        {
            Debug.Log($"[AuthorizationUIManager] Loading Bar assigned: {loadingBar.gameObject.name}");
        }

        if (checkmark == null)
        {
            Debug.LogError("[AuthorizationUIManager] ERROR: Checkmark is not assigned!");
        }
        else
        {
            Debug.Log($"[AuthorizationUIManager] Checkmark assigned: {checkmark.name}");
        }

        // Ensure UI starts hidden
        HideAll();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Starts the authorization animation. Returns when complete.
    /// </summary>
    public void StartAuthorization(System.Action onComplete = null)
    {
        Debug.Log("[AuthorizationUIManager] ===== StartAuthorization() called =====");

        // Stop any existing authorization animation
        if (currentAuthCoroutine != null)
        {
            StopCoroutine(currentAuthCoroutine);
        }

        currentAuthCoroutine = StartCoroutine(AuthorizationSequence(onComplete));
    }

    private IEnumerator AuthorizationSequence(System.Action onComplete)
    {
        Debug.Log("[AuthorizationUIManager] Starting authorization sequence...");

        // Hide checkmark, show loading bar
        if (checkmark != null)
        {
            checkmark.SetActive(false);
            Debug.Log("[AuthorizationUIManager] Checkmark hidden");
        }

        if (loadingBar != null)
        {
            // Ensure it's visible and reset
            loadingBar.gameObject.SetActive(true);
            loadingBar.fillAmount = 0f;

            // Force alpha to 1
            Color color = loadingBar.color;
            color.a = 1f;
            loadingBar.color = color;

            Debug.Log("[AuthorizationUIManager] Loading bar shown and reset to 0");
            Debug.Log($"[AuthorizationUIManager] - Active: {loadingBar.gameObject.activeSelf}");
            Debug.Log($"[AuthorizationUIManager] - ActiveInHierarchy: {loadingBar.gameObject.activeInHierarchy}");
            Debug.Log($"[AuthorizationUIManager] - Enabled: {loadingBar.enabled}");
            Debug.Log($"[AuthorizationUIManager] - Color.a: {loadingBar.color.a}");
            Debug.Log($"[AuthorizationUIManager] - Position: {loadingBar.transform.position}");
        }
        else
        {
            Debug.LogError("[AuthorizationUIManager] ERROR: Cannot show loading bar - reference is NULL!");
            yield break;
        }

        // Text removed per user request - keep loading bar and checkmark only
        /*
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "Verifying Authorization...";
            Debug.Log("[AuthorizationUIManager] Loading text shown");
        }
        */

        // Animate loading bar
        Debug.Log($"[AuthorizationUIManager] Starting {loadingDuration}s animation...");
        float elapsed = 0f;
        int logCount = 0;

        while (elapsed < loadingDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / loadingDuration);

            if (loadingBar != null)
            {
                loadingBar.fillAmount = progress;

                // Log every 60 frames
                if (logCount % 60 == 0)
                {
                    Debug.Log($"[AuthorizationUIManager] Progress: {progress:P0} | FillAmount: {loadingBar.fillAmount:F2} | Time.deltaTime: {Time.deltaTime:F3}");
                }
                logCount++;
            }

            yield return null;
        }

        // Ensure fully filled
        if (loadingBar != null)
        {
            loadingBar.fillAmount = 1f;
            Debug.Log("[AuthorizationUIManager] Animation complete! FillAmount set to 1.0");
        }

        // Wait a moment
        yield return new WaitForSeconds(0.3f);

        // Hide loading UI, show checkmark
        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(false);
            Debug.Log("[AuthorizationUIManager] Loading bar hidden");
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        if (checkmark != null)
        {
            checkmark.SetActive(true);
            Debug.Log("[AuthorizationUIManager] Checkmark shown");
        }

        Debug.Log("[AuthorizationUIManager] ===== Authorization sequence complete! =====");

        // Notify completion
        onComplete?.Invoke();

        currentAuthCoroutine = null;
    }

    /// <summary>
    /// Hides all authorization UI elements
    /// </summary>
    public void HideAll()
    {
        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(false);
            loadingBar.fillAmount = 0f;
        }

        if (checkmark != null)
        {
            checkmark.SetActive(false);
        }

        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        Debug.Log("[AuthorizationUIManager] All UI elements hidden");
    }

    /// <summary>
    /// Resets the authorization UI to initial state
    /// </summary>
    public void Reset()
    {
        if (currentAuthCoroutine != null)
        {
            StopCoroutine(currentAuthCoroutine);
            currentAuthCoroutine = null;
        }

        HideAll();
        Debug.Log("[AuthorizationUIManager] Reset complete");
    }
}
