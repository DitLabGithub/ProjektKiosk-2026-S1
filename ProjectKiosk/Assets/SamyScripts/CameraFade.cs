using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFade : MonoBehaviour {
    public float fadeDuration = 1f;
    public Color fadeColor = Color.black;
    public AnimationCurve Curve = AnimationCurve.Linear(0, 0, 1, 1);
    public bool startFadedOut = false;

    public Vector3[] roomPositions = new Vector3[3]; // 0 = Left, 1 = Middle, 2 = Right
    private int currentRoomIndex = 1; // Start in middle

    private float alpha = 0f;
    private Texture2D texture;
    private bool isFading = false;

    private void Start() {
        alpha = startFadedOut ? 1f : 0f;
        texture = new Texture2D(1, 1);
        UpdateTextureAlpha();
        // Make sure camera starts at correct position
        Camera.main.transform.position = roomPositions[currentRoomIndex];
    }

    private void Update() {
        if (isFading) return;

        // Move right (→ or D) and play walking sound
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) {
            SoundManager.Instance.PlayWalkSound();
            TryMoveToRoom(currentRoomIndex + 1);
        }

        // Move left (← or A) and play walking sound
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) {
            SoundManager.Instance.PlayWalkSound();
            TryMoveToRoom(currentRoomIndex - 1);
        }

    }

    private void TryMoveToRoom(int targetIndex) {
        if (targetIndex >= 0 && targetIndex < roomPositions.Length) {
            StartCoroutine(FadeAndMove(targetIndex));
        }
    }

    private IEnumerator FadeAndMove(int newIndex) {
        isFading = true;

        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f));

        // Move camera
        currentRoomIndex = newIndex;
        Camera.main.transform.position = roomPositions[currentRoomIndex];

        // Optional short pause
        yield return new WaitForSeconds(0.05f);

        // Fade In
        yield return StartCoroutine(Fade(1f, 0f));

        isFading = false;
    }

    private IEnumerator Fade(float from, float to) {
        float time = 0f;
        while (time < fadeDuration) {
            float t = time / fadeDuration;
            alpha = Mathf.Lerp(from, to, Curve.Evaluate(t));
            UpdateTextureAlpha();
            time += Time.deltaTime;
            yield return null;
        }

        alpha = to;
        UpdateTextureAlpha();
    }

    private void OnGUI() {
        if (alpha > 0f) {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);
        }
    }

    private void UpdateTextureAlpha() {
        texture.SetPixel(0, 0, new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha));
        texture.Apply();
    }
    public void MoveLeftButton() {
        TryMoveToRoom(currentRoomIndex - 1);
        SoundManager.Instance.PlayWalkSound();
    }

    public void MoveRightButton() {
        TryMoveToRoom(currentRoomIndex + 1);
        SoundManager.Instance.PlayWalkSound();
    }


}
