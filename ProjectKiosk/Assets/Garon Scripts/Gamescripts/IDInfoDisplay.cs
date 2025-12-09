using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class IDInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dobText;
    public TextMeshProUGUI addressText;
    public TextMeshProUGUI ssiIssuer;
    public Image profileImage;
    public GameObject infoPanel;

    public void ShowInfo(string name, string dob, string address, string issuer, Sprite image)
    {
        nameText.text = string.IsNullOrEmpty(name) ? "[Access Denied]" : name;
        dobText.text = string.IsNullOrEmpty(dob) ? "[Access Denied]" : dob;
        addressText.text = string.IsNullOrEmpty(address) ? "[Access Denied]" : address;
        ssiIssuer.text = string.IsNullOrEmpty(issuer) ? "Access Denied" : issuer;


        if (image != null)
        {
            profileImage.sprite = image;
            profileImage.color = Color.white;
        }
        else
        {
            profileImage.sprite = null;
            profileImage.color = new Color(1, 1, 1, 0); // transparent
        }

        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    /// <summary>
    /// Overload for showing Authorization IDs (displays Authorization Status instead of Address)
    /// </summary>
    public void ShowInfo(string name, string dob, string address, string issuer, Sprite image, bool isAuthorizationID, string authorizationStatus = "")
    {
        nameText.text = string.IsNullOrEmpty(name) ? "[Access Denied]" : name;
        dobText.text = string.IsNullOrEmpty(dob) ? "[Access Denied]" : dob;

        // For Authorization IDs, don't show authorization status text (user request)
        if (isAuthorizationID)
        {
            // Authorization status removed - loading bar and checkmark show status instead
            addressText.text = string.IsNullOrEmpty(address) ? "[Access Denied]" : address;
        }
        else
        {
            addressText.text = string.IsNullOrEmpty(address) ? "[Access Denied]" : address;
        }

        ssiIssuer.text = string.IsNullOrEmpty(issuer) ? "Access Denied" : issuer;

        if (image != null)
        {
            profileImage.sprite = image;
            profileImage.color = Color.white;
        }
        else
        {
            profileImage.sprite = null;
            profileImage.color = new Color(1, 1, 1, 0); // transparent
        }

        if (infoPanel != null)
            infoPanel.SetActive(true);
    }

    public void HideInfo()
    {
        nameText.text = "";
        dobText.text = "";
        addressText.text = "";
        ssiIssuer.text = "";
        profileImage.sprite = null;
        profileImage.color = new Color(1, 1, 1, 0);

        if (infoPanel != null)
            infoPanel.SetActive(false);
    }
}
