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
            popupPanel.SetActive(false);
            onConfirmCallback?.Invoke();
        });

        noButton.onClick.AddListener(() => {
            popupPanel.SetActive(false);
        });
    }
}
