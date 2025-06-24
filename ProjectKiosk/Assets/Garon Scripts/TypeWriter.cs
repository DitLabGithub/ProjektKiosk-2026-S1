using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    public float typeSpeed = 0.05f;

    public IEnumerator TypeTextFromTMP(TMP_Text textComponent, string text)
    {
        string fullText = text;

        foreach (char letter in fullText)
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    public IEnumerator TypeTwoTextsFromTMP(TMP_Text topUI, string textTop, TMP_Text bottomUI, string textBottom)
    {
        yield return StartCoroutine(TypeTextFromTMP(topUI, textTop));
        yield return StartCoroutine(TypeTextFromTMP(bottomUI, textBottom));
    }
}
