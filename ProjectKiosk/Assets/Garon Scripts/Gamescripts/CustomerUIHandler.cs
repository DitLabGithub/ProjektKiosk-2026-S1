using UnityEngine;
using UnityEngine.EventSystems;

public class CustomerUIHandler : MonoBehaviour, IPointerClickHandler
{
    public DialogueManager dialogueManager;
    private CanvasGroup canvasGroup;
    public float fadeDuration = 2f;

    private bool isFadedIn = false;
    private bool canClick = false;


    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        StartCoroutine(FadeIn());
    }
    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        isFadedIn = true;
        canClick = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isFadedIn && canClick)
        {
            dialogueManager.StartDialogue();
            canClick = false;
        }
    }
}
