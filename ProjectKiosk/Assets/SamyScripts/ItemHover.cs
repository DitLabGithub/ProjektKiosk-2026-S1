using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemHoverDisplay : MonoBehaviour {
    public Color highlightColor = Color.yellow;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    public GameObject hoverTextObject; // Assign the parent GameObject (with Image + TMP) in Inspector
    private TextMeshProUGUI hoverText;
    private Image backgroundImage;
    private Coroutine typingCoroutine;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        hoverText = hoverTextObject.GetComponentInChildren<TextMeshProUGUI>();
        backgroundImage = hoverTextObject.GetComponent<Image>();
        hoverTextObject.SetActive(false);

        if (backgroundImage != null) {
            backgroundImage.color = new Color(0f, 0f, 0f, 0.5f); // Black with 50% opacity
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
    }

    IEnumerator TypeItemName(string text) {
        hoverText.text = "";
        foreach (char letter in text) {
            hoverText.text += letter;
            yield return new WaitForSeconds(0.05f); // Typing speed
        }
    }
}
