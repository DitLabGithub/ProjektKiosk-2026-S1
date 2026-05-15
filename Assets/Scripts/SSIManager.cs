using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
// Close UI + resume dialogue (you'll hook this next)
public class SSIManager : MonoBehaviour
{
    public static SSIManager Instance;
    public Image photo;
    public TextMeshProUGUI name;
    public TextMeshProUGUI age;
    public TextMeshProUGUI above18;
    public TextMeshProUGUI above21;
    public TextMeshProUGUI address;
    public TextMeshProUGUI expiryDate;
    public GameObject buttonToHideForPhoto;


    private HashSet<string> askedFields = new HashSet<string>();
    private HashSet<string> requiredFields = new HashSet<string>();

    private string perfectNode;
    private string goodNode;
    private string badNode;

    private void Awake()
    {
        Instance = this;
    }
    public void SetupOrders(string orderString)
    {
        ResetSSI();

        requiredFields.Clear();

        string[] orders = orderString.Split(',');

        foreach (string order in orders)
        {
            string o = order.Trim().ToLower();

            if (o == "package")
            {
                requiredFields.Add("name");
                requiredFields.Add("address");
            }
            else if (o == "alcohol")
            {
                requiredFields.Add("above18");
                requiredFields.Add("photo");
            }
            else if (o == "food")
            {
                requiredFields.Add("expiryDate");
            }
        }

        askedFields.Clear();
    }

    public void Evaluate()
    {
        int correct = 0;

        // Count required fields asked
        foreach (var field in requiredFields)
        {
            if (askedFields.Contains(field))
                correct++;
        }

        bool missedRequired = correct < requiredFields.Count;

        bool askedExtra = false;

        foreach (var field in askedFields)
        {
            if (!requiredFields.Contains(field))
            {
                askedExtra = true;
                break;
            }
        }

        string result;

        if (missedRequired)
        {
            result = "bad";
        }
        else if (askedExtra)
        {
            result = "good";
        }
        else
        {
            result = "perfect";
        }

        Debug.Log("SSI Result: " + result);

        DialogueManager.Instance.currentNode =
    DialogueManager.Instance.currentDialogue.nodes.Find(
        n => n.id == (result == "perfect" ? perfectNode : result == "good" ? goodNode : badNode)
    );

        // Store in NPC
        DialogueManager.Instance.currentNPC.SetVariable(
     "SSI_Result",
     result);
    }

    public void FinishSSI()
    {
        Evaluate();

        UIManager.Instance.ToggleIdentification_SSI();

        DialogueManager.Instance.ShowCurrentNode();
    }


    public void RevealName()
    {
        name.text = DialogueManager.Instance.currentNPC.data.name;
        askedFields.Add("name");
    }

    public void RevealAge()
    {
        age.text = DialogueManager.Instance.currentNPC.data.age.ToString();
        askedFields.Add("age");
    }

    public void RevealAbove18()
    {
        above18.text = DialogueManager.Instance.currentNPC.data.age >= 18 ? "Yes" : "No";
        askedFields.Add("above18");
    }

    public void RevealAbove21()
    {
        above21.text = DialogueManager.Instance.currentNPC.data.age >= 21 ? "Yes" : "No";
        askedFields.Add("above21");
    }

    public void RevealAddress()
    {
        address.text = DialogueManager.Instance.currentNPC.data.address;
        askedFields.Add("address");
    }

    public void RevealExpiryDate()
    {
        expiryDate.text = DialogueManager.Instance.currentNPC.data.expiryDate;
        askedFields.Add("expiryDate");
    }

    public void RevealPhoto()
    {
        photo.sprite = DialogueManager.Instance.currentNPC.data.avatar;
        buttonToHideForPhoto.SetActive(false);
        askedFields.Add("photo");
    }

    public void SetupResultNodes(
    string perfect,
    string good,
    string bad
)
    {
        perfectNode = perfect;
        goodNode = good;
        badNode = bad;
    }

    public void ResetSSI()
    {
        askedFields.Clear();

        photo.sprite = null;

        name.text = "Request";
        age.text = "Request";
        above18.text = "Request";
        above21.text = "Request";
        address.text = "Request";
        expiryDate.text = "Request";

        buttonToHideForPhoto.SetActive(true);
    }
}