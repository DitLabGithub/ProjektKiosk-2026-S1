using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private Button goBackButton;

    [System.Serializable]
    public class DialogueResponse {
        public string responseText;
        public int nextLineIndex = -1;
        public bool returnAfterResponse = false;
        public bool activateContinueAfterChoice = false;

        public bool isMakeSaleResponse = false;
    }


    [System.Serializable]
    public class DialogueLine
    {
        public int editorIndex;
        public string speaker;
        [TextArea(2, 5)]
        public string text;
        public List<DialogueResponse> responses = new(); 

        public bool askForID = false;
        public bool showGoBackButton = false;
        public int goBackTargetIndex = -1;

        public bool grantNameAccess = false;
        public bool grantDOBAccess = false;
        public bool grantAddressAccess = false;
        public bool grantIssuerAccess = false;
        public bool grantPictureAccess = false;
        public bool disableContinueButton = false;

        public bool endConversationHere = false;
        public int scoreScreenIndex = 0;    

    }

    [System.Serializable]
    public class CustomerDialogue {
        public string name;
        public List<DialogueLine> lines = new();
        public GameObject idPrefab;
        public GameObject npcObject;

        public List<ItemCategory> requestedItems = new(); //per-NPC item requests
    }

    private Dictionary<int, HashSet<int>> chosenResponses = new Dictionary<int, HashSet<int>>();
    private Stack<int> dialogueReturnStack = new Stack<int>();
    private Dictionary<int, bool> continueFlagPerLine = new(); // Track continue activation

    public ItemPickupManager itemPickupManager;

    [Header("Sale Audio")]
    public AudioClip saleSuccessClip;
    public AudioClip saleRejectedClip;


    [Header("UI References")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public GameObject choicePanel;
    public GameObject responseButtonPrefab;
    public Transform responseButtonContainer;
    public Transform idSpawnLocation;

    public IDInfoDisplay infoDisplay;

    [Header("Dialogue Data")]
    public List<CustomerDialogue> customers = new();

    [Header("UI Panels")]
    public List<GameObject> scoreScreens = new();
    public GameObject endCreditScreen;

    public List<Button> scoreContinueButtons;

    private int currentCustomerIndex = 0;
    private int currentLineIndex = 0;
    private bool dialogueStarted = false;

    private GameObject currentIDInstance;
    private CustomerData currentScannedIDData;  

    // Access flags with public getters and private setters
    public bool AllowNameAccess { get; private set; }
    public bool AllowDOBAccess { get; private set; }
    public bool AllowAddressAccess { get; private set; }
    public bool AllowPictureAccess { get; private set; }
    public bool AllowwIssuerAccess { get; private set; }

    private bool waitingForAction = false;  // pause dialogue waiting for ID scan


    private System.Collections.IEnumerator ReturnToPreviousSlideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dialogueReturnStack.Count > 0)
        {
            int returnToIndex = dialogueReturnStack.Pop();

            bool shouldEnableContinue = false;
            if (chosenResponses.TryGetValue(returnToIndex, out var responseIndices))
            {
                var line = customers[currentCustomerIndex].lines[returnToIndex];
                foreach (int idx in responseIndices)
                {
                    if (idx < line.responses.Count && line.responses[idx].activateContinueAfterChoice)
                    {
                        shouldEnableContinue = true;
                        break;
                    }
                }
            }

            currentLineIndex = returnToIndex;
            ShowNextLine();

            if (shouldEnableContinue)
            {
                continueButton.gameObject.SetActive(true);
            }
        }
    }



    void Start()
    {
        continueButton.onClick.AddListener(NextLine);
        foreach (var button in scoreContinueButtons) {
            if (button != null)
                button.onClick.AddListener(OnScoreContinue);
        }
        continueButton.gameObject.SetActive(false);
        foreach (var screen in scoreScreens) {
            if (screen != null) screen.SetActive(false);
        }
        ClearDialogueUI();
    }

    public void StartDialogue()
    {
        if (!dialogueStarted)
        {
            dialogueStarted = true;
            ShowNextLine();
        }
    }

    public void OnIDScanned(CustomerID scannedID)
    {
        currentScannedIDData = new CustomerData(scannedID);
        waitingForAction = false;

        UpdateIDInfoPanel();

        if (!choicePanel.activeSelf)
        {
            continueButton.gameObject.SetActive(true);
        }

        dialogueText.text = "ID scanned. Thank you!";
    }
    void OnScoreContinue() {
        foreach (var screen in scoreScreens) {
            if (screen != null) screen.SetActive(false);
        }
        ResetDialogueState();

        if (currentCustomerIndex < customers.Count && customers[currentCustomerIndex].npcObject != null) {
            customers[currentCustomerIndex].npcObject.SetActive(false);
        }

        currentCustomerIndex++;
        currentLineIndex = 0;

        if (currentCustomerIndex < customers.Count) {
            if (customers[currentCustomerIndex].npcObject != null) {
                customers[currentCustomerIndex].npcObject.SetActive(true);
            }

            ShowNextLine();
        } else {
            dialogueText.text = "All customers served!";
            speakerText.text = "";
            endCreditScreen.gameObject.SetActive(true);
        }
    }


    void UpdateIDInfoPanel()
    {
        if (currentScannedIDData == null || infoDisplay == null)
        {
            infoDisplay.HideInfo();
            return;
        }

        string name = currentScannedIDData.allowNameAccess ? currentScannedIDData.Name : "[Access Denied]";
        string dob = currentScannedIDData.allowDOBAccess ? currentScannedIDData.DOB : "[Access Denied]";
        string address = currentScannedIDData.allowAddressAccess ? currentScannedIDData.Address : "[Access Denied]";
        string issuer = currentScannedIDData.allowIssuerAccess ? currentScannedIDData.Issuer : "[Access Denied]";
        Sprite image = currentScannedIDData .allowPictureAccess ? currentScannedIDData.ProfileImage : null;

        infoDisplay.ShowInfo(name, dob, address,issuer, image);
    }

    void ShowNextLine() {
        continueButton.gameObject.SetActive(false);
        goBackButton.gameObject.SetActive(false); // Hide go back by default
        ClearResponses();

        if (currentCustomerIndex >= customers.Count) {
            speakerText.text = "";
            dialogueText.text = "All customers served!";
            continueButton.gameObject.SetActive(false);
            infoDisplay.HideInfo();
            endCreditScreen.gameObject.SetActive(true);
            return;
        }

        var customer = customers[currentCustomerIndex];
        if (currentLineIndex >= customer.lines.Count) {
            ResetDialogueState();
            ShowNextLine();
            return;
        }

        var line = customer.lines[currentLineIndex];

        if (currentScannedIDData != null) {
            bool accessChanged = false;

            if (line.grantNameAccess && !currentScannedIDData.allowNameAccess) { currentScannedIDData.allowNameAccess = true; accessChanged = true; }
            if (line.grantDOBAccess && !currentScannedIDData.allowDOBAccess) { currentScannedIDData.allowDOBAccess = true; accessChanged = true; }
            if (line.grantAddressAccess && !currentScannedIDData.allowAddressAccess) { currentScannedIDData.allowAddressAccess = true; accessChanged = true; }
            if (line.grantPictureAccess && !currentScannedIDData.allowPictureAccess) { currentScannedIDData.allowPictureAccess = true; accessChanged = true; }

            if (accessChanged) UpdateIDInfoPanel();
        }

        speakerText.text = line.speaker;
        dialogueText.text = line.text;

        if (currentScannedIDData != null)
            UpdateIDInfoPanel();

        if (line.askForID) {
            waitingForAction = true;
            continueButton.gameObject.SetActive(false);
            SpawnID(customer);
            return;
        }

        if (line.endConversationHere) {
            if (dialogueReturnStack.Count > 0) {
                currentLineIndex = dialogueReturnStack.Pop() + 1;
                ShowNextLine();
            } else {
                ShowScoreScreen(line.scoreScreenIndex);
            }
            return;
        }

        if (line.responses.Count > 0) {
            choicePanel.SetActive(true);
            continueButton.gameObject.SetActive(false);

            if (!chosenResponses.ContainsKey(currentLineIndex))
                chosenResponses[currentLineIndex] = new HashSet<int>();

            for (int i = 0; i < line.responses.Count; i++) {
                var response = line.responses[i];
                GameObject btn = Instantiate(responseButtonPrefab, responseButtonContainer);
                btn.SetActive(true);
                var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                txt.text = response.responseText;

                Button buttonComponent = btn.GetComponent<Button>();

                if (chosenResponses[currentLineIndex].Contains(i)) {
                    buttonComponent.interactable = true; // ✅ Let player click again
                    txt.color = Color.gray;
                } else {
                    txt.color = Color.white;
                }

                int nextIndex = response.nextLineIndex;
                int responseIndex = i;

                buttonComponent.onClick.AddListener(() =>
                {
                    var customer = customers[currentCustomerIndex];
                    var response = line.responses[responseIndex];

                    //Sale logic
                    if (response.isMakeSaleResponse) {
                        List<ItemCategory> requested = customer.requestedItems;
                        List<ItemCategory> present = GetItemCategoriesInCheckout();

                        if (!CheckRequestedItemsMatch(requested, present)) {
                            Debug.Log("Sale rejected!");
                            SoundManager.Instance.PlaySound(saleRejectedClip);

                            string requestedItemsText = string.Join(", ", requested.Select(r => r.ToString().Replace("_", " ")));
                            dialogueText.text = $"I asked for {requestedItemsText}, that's it.";

                            return; // Don't advance
                        }


                        //Successful sale
                        Debug.Log("Sale successful!");
                        SoundManager.Instance.PlaySound(saleSuccessClip);
                        itemPickupManager.SellItems(); // Remove items from checkout
                    }

                    chosenResponses[currentLineIndex].Add(responseIndex);

                    if (response.nextLineIndex >= 0) {
                        if (response.returnAfterResponse)
                            dialogueReturnStack.Push(currentLineIndex);

                        currentLineIndex = response.nextLineIndex;
                    } else {
                        currentLineIndex++;
                    }

                    ShowNextLine();
                });

            }
        } else {
            choicePanel.SetActive(false);

            if (!line.disableContinueButton) {
                continueButton.gameObject.SetActive(true);
            }
        }

        // ✅ Show the go back button if it's enabled
        if (line.showGoBackButton && line.goBackTargetIndex >= 0) {
            goBackButton.gameObject.SetActive(true);
            goBackButton.onClick.RemoveAllListeners();
            goBackButton.onClick.AddListener(() =>
            {
                currentLineIndex = line.goBackTargetIndex;
                ShowNextLine();
            });
        } else {
            goBackButton.gameObject.SetActive(false);
        }
    }

    void ShowScoreScreen(int screenIndex = 0) {
        foreach (var screen in scoreScreens) {
            if (screen != null) screen.SetActive(false);
        }

        if (screenIndex >= 0 && screenIndex < scoreScreens.Count && scoreScreens[screenIndex] != null) {
            scoreScreens[screenIndex].SetActive(true);
        } else {
            Debug.LogWarning($"Score screen index {screenIndex} is invalid.");
        }

        continueButton.gameObject.SetActive(false);
        infoDisplay.gameObject.SetActive(false);
    }

    void NextLine()
    {
        if (waitingForAction)
        {
            dialogueText.text = "Please complete the required action.";
            return;
        }

        if (currentScannedIDData != null)
            UpdateIDInfoPanel();

        currentLineIndex++;
        ShowNextLine();
    }

    void SpawnID(CustomerDialogue customer)
    {
        if (customer.idPrefab == null || idSpawnLocation == null)
        {
            Debug.LogWarning("Missing ID prefab or spawn point.");
            return;
        }

        if (currentIDInstance != null)
        {
            Destroy(currentIDInstance);
        }

        currentIDInstance = Instantiate(customer.idPrefab, idSpawnLocation);
        RectTransform rect = currentIDInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
        }
    }

    void ResetDialogueState()
    {
        currentScannedIDData = null;
        AllowNameAccess = false;
        AllowDOBAccess = false;
        AllowAddressAccess = false;
        AllowPictureAccess = false;
        infoDisplay.HideInfo();
        waitingForAction = false;
    }

    void ClearResponses()
    {
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    void ClearDialogueUI()
    {
        speakerText.text = "";
        dialogueText.text = "";
        choicePanel.SetActive(false);
        infoDisplay.HideInfo();
    }

    private List<ItemCategory> GetItemCategoriesInCheckout() {
        List<ItemCategory> categories = new List<ItemCategory>();
        foreach (Transform item in itemPickupManager.checkoutDropZone) {
            ItemSlotData data = item.GetComponent<ItemSlotData>();
            if (data != null) {
                categories.Add(data.category);
            }
        }
        return categories;
    }

    private bool CheckRequestedItemsMatch(List<ItemCategory> requested, List<ItemCategory> present) {
        if (requested.Count != present.Count)
            return false;

        // Make copies so we can modify them safely
        List<ItemCategory> requestedCopy = new List<ItemCategory>(requested);
        List<ItemCategory> presentCopy = new List<ItemCategory>(present);

        foreach (ItemCategory category in requestedCopy) {
            if (!presentCopy.Contains(category))
                return false;

            presentCopy.Remove(category); // Remove matched item to prevent duplicates
        }

        return presentCopy.Count == 0; // Make sure nothing extra remains
    }


}
