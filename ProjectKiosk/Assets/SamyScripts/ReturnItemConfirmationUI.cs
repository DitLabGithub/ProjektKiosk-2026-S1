using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReturnItemConfirmationUI : MonoBehaviour {
    public GameObject popupPanel;
    public Button yesButton;
    public Button noButton;

    private System.Action onConfirmCallback;

    public void Show(System.Action onConfirm) {
        popupPanel.SetActive(true);
        onConfirmCallback = onConfirm;
    }

    private void Start() {
        popupPanel.SetActive(false);

        yesButton.onClick.AddListener(() => {
            // Play button click sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayGenericButtonClick();
            }
            popupPanel.SetActive(false);
            onConfirmCallback?.Invoke();
        });

        noButton.onClick.AddListener(() => {
            // Play button click sound
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayGenericButtonClick();
            }
            popupPanel.SetActive(false);
        });
    }
}
