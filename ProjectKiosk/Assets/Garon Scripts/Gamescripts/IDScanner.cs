using UnityEngine;

public class IDScanner : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public SoundManager soundManager;
    private bool hasScanned = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        //if (hasScanned) return;

        CustomerID customerID = other.GetComponent<CustomerID>();
        if (customerID != null)
        {
            if (soundManager != null) {
                soundManager.PlayIDScanned();
            }

            hasScanned = true;
            dialogueManager.OnIDScanned(customerID);
            Destroy(customerID.gameObject);
        }
    }

    public void ResetScanner()
    {
        hasScanned = false;
    }
}
