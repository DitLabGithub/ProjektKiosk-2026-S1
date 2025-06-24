using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class FadeController : MonoBehaviour
{
    public CanvasGroup fadeGroup;

    private void Awake()
    {
        fadeGroup.alpha = 0f;
    }

    public IEnumerator FadeIn(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeGroup.alpha = 1f;
    }

    public IEnumerator FadeOut(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeGroup.alpha = 0f;
    }
}
