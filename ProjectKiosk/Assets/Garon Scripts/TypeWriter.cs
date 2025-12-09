using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    public float typeSpeed = 0.05f;

    // Track typing state
    private bool isTyping = false;
    private bool skipRequested = false;
    private TMP_Text currentTextComponent;
    private string currentFullText;

    private void Update()
    {
        // Detect click anywhere on screen to skip typing
        if (isTyping && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            skipRequested = true;
        }
    }

    public IEnumerator TypeTextFromTMP(TMP_Text textComponent, string text)
    {
        isTyping = true;
        skipRequested = false;
        currentTextComponent = textComponent;
        currentFullText = text;

        textComponent.text = ""; // Clear text first

        foreach (char letter in currentFullText)
        {
            // If skip requested, instantly show full text
            if (skipRequested)
            {
                textComponent.text = currentFullText;
                break;
            }

            textComponent.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        skipRequested = false;
    }

    public IEnumerator TypeTwoTextsFromTMP(TMP_Text topUI, string textTop, TMP_Text bottomUI, string textBottom)
    {
        yield return StartCoroutine(TypeTextFromTMP(topUI, textTop));
        yield return StartCoroutine(TypeTextFromTMP(bottomUI, textBottom));
    }

    /// <summary>
    /// Returns true if text is currently typing
    /// </summary>
    public bool IsTyping()
    {
        return isTyping;
    }

    /// <summary>
    /// Manually complete the current typing animation
    /// </summary>
    public void CompleteTyping()
    {
        if (isTyping)
        {
            skipRequested = true;
        }
    }
}
