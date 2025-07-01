using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemHoverDisplay : MonoBehaviour {
    public Color highlightColor = Color.yellow;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    public GameObject hoverTextObject; // Parent GameObject with Image component
    private TextMeshProUGUI hoverText;
    private Image backgroundImage;
    private Coroutine typingCoroutine;
    private RectTransform bgRect;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        hoverText = hoverTextObject.GetComponentInChildren<TextMeshProUGUI>();
        backgroundImage = hoverTextObject.GetComponent<Image>();
        bgRect = backgroundImage.GetComponent<RectTransform>();

        hoverTextObject.SetActive(false);

        if (backgroundImage != null) {
            backgroundImage.color = new Color(0f, 0f, 0f, 0.9f); //set opacity/color of text bg here
        }
    }

    public void OnHoverEnter() {
        spriteRenderer.color = highlightColor;

        string rawName = GetComponent<ItemSlotData>().category.ToString();
        string displayName = rawName.Replace("_", " "); // Turn underscores into spaces

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        hoverTextObject.SetActive(true);
        typingCoroutine = StartCoroutine(TypeItemName(displayName));
    }

    public void OnHoverExit() {
        spriteRenderer.color = originalColor;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        hoverTextObject.SetActive(false);
        hoverText.text = "";

        if (bgRect != null) {
            bgRect.sizeDelta = new Vector2(0f, bgRect.sizeDelta.y); // Reset width
        }
    }

    IEnumerator TypeItemName(string text) {
        hoverText.text = "";

        float padding = 0.24f;
        hoverText.ForceMeshUpdate(); // Ensure preferredWidth is updated
        float fullWidth = hoverText.GetPreferredValues(text).x + padding;
        float widthPerChar = fullWidth / Mathf.Max(text.Length, 1);

        for (int i = 0; i < text.Length; i++) {
            hoverText.text += text[i];

            if (bgRect != null) {
                float currentWidth = (i + 1) * widthPerChar;
                bgRect.sizeDelta = new Vector2(currentWidth, bgRect.sizeDelta.y);
            }

            yield return new WaitForSeconds(0.05f);
        }

        // Final correction to ensure perfect width
        if (bgRect != null) {
            bgRect.sizeDelta = new Vector2(fullWidth, bgRect.sizeDelta.y);
        }
    }
}
