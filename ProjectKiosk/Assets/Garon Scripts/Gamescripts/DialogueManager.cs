using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System; // Added for JsonUtility

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private Button goBackButton;

    [System.Serializable]
    public class DialogueResponse
    {
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
    public class CustomerDialogue
    {
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

    [Header("JSON Dialogue Data")]
    public List<string> jsonCustomerFileNames = new();
    private CustomerScenarioData currentJsonCustomer;
    private GameObject currentJsonNpcObject; // New field to track the instantiated NPC object

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
        foreach (var button in scoreContinueButtons)
        {
            if (button != null)
                button.onClick.AddListener(OnScoreContinue);
        }
        continueButton.gameObject.SetActive(false);
        foreach (var screen in scoreScreens)
        {
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

    // New JSON Utility Method
    private CustomerScenarioData LoadCustomerFromJSON(string filename)
    {
        TextAsset jsonTextAsset = Resources.Load<TextAsset>(filename);

        if (jsonTextAsset == null)
        {
            Debug.LogError($"JSON file not found in Resources: {filename}");
            return null;
        }

        try
        {
            CustomerScenarioData data = JsonUtility.FromJson<CustomerScenarioData>(jsonTextAsset.text);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse JSON file {filename}. Error: {e.Message}");
            return null;
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

    void OnScoreContinue()
    {
        foreach (var screen in scoreScreens)
        {
            if (screen != null) screen.SetActive(false);
        }
        ResetDialogueState();

        // Deactivate the current NPC object if it exists (handles both old and new)
        if (currentCustomerIndex < customers.Count && customers[currentCustomerIndex].npcObject != null)
        {
            customers[currentCustomerIndex].npcObject.SetActive(false);
        }
        else if (currentJsonNpcObject != null)
        {
            Destroy(currentJsonNpcObject);
            currentJsonNpcObject = null;
        }

        currentCustomerIndex++;
        currentLineIndex = 0;

        int totalCustomers = customers.Count + jsonCustomerFileNames.Count;

        if (currentCustomerIndex < totalCustomers)
        {
            if (currentCustomerIndex < customers.Count)
            {
                // Old Customer Logic (Hoss, Robin, Amon)
                if (customers[currentCustomerIndex].npcObject != null)
                {
                    customers[currentCustomerIndex].npcObject.SetActive(true);
                }
                currentJsonCustomer = null; // Ensure JSON data is cleared
                ShowNextLine();
            }
            else
            {
                // New JSON Customer Logic (Emma and beyond)
                int jsonIndex = currentCustomerIndex - customers.Count;
                string filename = jsonCustomerFileNames[jsonIndex];

                currentJsonCustomer = LoadCustomerFromJSON(filename);
                if (currentJsonCustomer != null)
                {
                    // Instantiate NPC Prefab
                    GameObject npcPrefab = Resources.Load<GameObject>(currentJsonCustomer.npcPrefabPath);
                    if (npcPrefab != null)
                    {
                        // Instantiate in the same parent as DialogueManager
                        currentJsonNpcObject = Instantiate(npcPrefab, transform.parent);

                        // FIX: Reset RectTransform for correct scaling/positioning
                        RectTransform rect = currentJsonNpcObject.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            // Hardcoding a smaller scale for testing. This is the value you need to control in the JSON.
                            float scale = 0.5f;
                            rect.localScale = new Vector3(scale, scale, scale);
                            rect.anchoredPosition = Vector2.zero; // Center the object
                        }

                        currentJsonNpcObject.SetActive(true); // Ensure it is active

                        // Get CustomerUIHandler and link the DialogueManager
                        CustomerUIHandler handler = currentJsonNpcObject.GetComponent<CustomerUIHandler>();
                        if (handler != null)
                        {
                            handler.dialogueManager = this; // Set reference
                        }
                    }
                    ShowNextLine();
                }
                else
                {
                    Debug.LogError($"Failed to load customer from JSON file: {filename}. Ending sequence.");
                    currentCustomerIndex = totalCustomers; // Skip to end
                    OnScoreContinue(); // Re-call to show end screen
                }
            }
        }
        else
        {
            // End of all customers
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
        Sprite image = currentScannedIDData.allowPictureAccess ? currentScannedIDData.ProfileImage : null;

        infoDisplay.ShowInfo(name, dob, address, issuer, image);
    }

    void ShowNextLine()
    {
        continueButton.gameObject.SetActive(false);
        goBackButton.gameObject.SetActive(false); // Hide go back by default
        ClearResponses();

        int totalCustomers = customers.Count + jsonCustomerFileNames.Count;
        if (currentCustomerIndex >= totalCustomers)
        {
            speakerText.text = "";
            dialogueText.text = "All customers served!";
            continueButton.gameObject.SetActive(false);
            infoDisplay.HideInfo();
            endCreditScreen.gameObject.SetActive(true);
            return;
        }

        // Determine if we are using the old customer list or the new JSON data
        CustomerDialogue oldCustomer = null;
        DialogueLine oldLine = null;
        DialogueLineData jsonLine = null;

        if (currentCustomerIndex < customers.Count)
        {
            // Old Customer Logic
            oldCustomer = customers[currentCustomerIndex];
            if (currentLineIndex >= oldCustomer.lines.Count)
            {
                ResetDialogueState();
                ShowNextLine();
                return;
            }
            oldLine = oldCustomer.lines[currentLineIndex];
        }
        else
        {
            // New JSON Customer Logic
            if (currentJsonCustomer == null)
            {
                // This should not happen if OnScoreContinue is called correctly, but as a safeguard
                Debug.LogError("currentJsonCustomer is null. Cannot show next line for JSON customer.");
                return;
            }

            if (currentLineIndex >= currentJsonCustomer.lines.Count)
            {
                ResetDialogueState();
                ShowNextLine();
                return;
            }
            jsonLine = currentJsonCustomer.lines[currentLineIndex];
        }

        // --- Start of logic that needs to be data source aware ---

        // Data Access Granting
        if (currentScannedIDData != null)
        {
            bool accessChanged = false;

            bool grantName = oldLine != null ? oldLine.grantNameAccess : (jsonLine != null ? jsonLine.grantNameAccess : false);
            bool grantDOB = oldLine != null ? oldLine.grantDOBAccess : (jsonLine != null ? jsonLine.grantDOBAccess : false);
            bool grantAddress = oldLine != null ? oldLine.grantAddressAccess : (jsonLine != null ? jsonLine.grantAddressAccess : false);
            bool grantIssuer = oldLine != null ? oldLine.grantIssuerAccess : (jsonLine != null ? jsonLine.grantIssuerAccess : false);
            bool grantPicture = oldLine != null ? oldLine.grantPictureAccess : (jsonLine != null ? jsonLine.grantPictureAccess : false);

            if (grantName && !currentScannedIDData.allowNameAccess) { currentScannedIDData.allowNameAccess = true; accessChanged = true; }
            if (grantDOB && !currentScannedIDData.allowDOBAccess) { currentScannedIDData.allowDOBAccess = true; accessChanged = true; }
            if (grantAddress && !currentScannedIDData.allowAddressAccess) { currentScannedIDData.allowAddressAccess = true; accessChanged = true; }
            if (grantPicture && !currentScannedIDData.allowPictureAccess) { currentScannedIDData.allowPictureAccess = true; accessChanged = true; }

            if (accessChanged) UpdateIDInfoPanel();
        }

        // Speaker and Text 
        string speaker = oldLine != null ? oldLine.speaker : (jsonLine != null ? jsonLine.speaker : "N/A");
        string text = oldLine != null ? oldLine.text : (jsonLine != null ? jsonLine.text : "N/A");
        bool askForID = oldLine != null ? oldLine.askForID : (jsonLine != null ? jsonLine.askForID : false);
        bool endConversationHere = oldLine != null ? oldLine.endConversationHere : (jsonLine != null ? jsonLine.endConversationHere : false);
        int scoreScreenIndex = oldLine != null ? oldLine.scoreScreenIndex : (jsonLine != null ? jsonLine.scoreScreenIndex : 0);
        bool showGoBackButton = oldLine != null ? oldLine.showGoBackButton : (jsonLine != null ? jsonLine.showGoBackButton : false);
        int goBackTargetIndex = oldLine != null ? oldLine.goBackTargetIndex : (jsonLine != null ? jsonLine.goBackTargetIndex : -1);
        bool disableContinueButton = oldLine != null ? oldLine.disableContinueButton : (jsonLine != null ? jsonLine.disableContinueButton : false);

        List<DialogueResponse> responses = oldLine != null ? oldLine.responses : new List<DialogueResponse>();
        List<DialogueResponseData> jsonResponses = jsonLine != null ? jsonLine.responses : new List<DialogueResponseData>();


        speakerText.text = speaker;
        dialogueText.text = text;
        if (currentScannedIDData != null)
            UpdateIDInfoPanel();

        // Ask for ID
        if (askForID)
        {
            waitingForAction = true;
            continueButton.gameObject.SetActive(false);
            if (oldCustomer != null)
            {
                SpawnID(oldCustomer);
            }
            else if (currentJsonCustomer != null)
            {
                SpawnIDFromJSON(currentJsonCustomer);
            }
            return;
        }

        // End Conversation
        if (endConversationHere)
        {
            if (dialogueReturnStack.Count > 0)
            {
                currentLineIndex = dialogueReturnStack.Pop() + 1;
                ShowNextLine();
            }
            else
            {
                ShowScoreScreen(scoreScreenIndex);
            }
            if (itemPickupManager != null)
            {
                itemPickupManager.ReturnCarriedItemsToShelf();
            }
            return;
        }

        // Responses
        if (responses.Count > 0 || jsonResponses.Count > 0)
        {
            choicePanel.SetActive(true);
            continueButton.gameObject.SetActive(false);

            int currentLineIdx = currentLineIndex;

            if (!chosenResponses.ContainsKey(currentLineIdx))
                chosenResponses[currentLineIdx] = new HashSet<int>();

            int responseCount = oldLine != null ? oldLine.responses.Count : jsonLine.responses.Count;

            for (int i = 0; i < responseCount; i++)
            {
                // Use a common response object for the button creation
                string responseText;
                int nextIndex;
                bool returnAfterResponse;
                bool isMakeSaleResponse;

                if (oldLine != null)
                {
                    var response = oldLine.responses[i];
                    responseText = response.responseText;
                    nextIndex = response.nextLineIndex;
                    returnAfterResponse = response.returnAfterResponse;
                    isMakeSaleResponse = response.isMakeSaleResponse;
                }
                else
                {
                    var response = jsonLine.responses[i];
                    responseText = response.responseText;
                    nextIndex = response.nextLineIndex;
                    returnAfterResponse = response.returnAfterResponse;
                    isMakeSaleResponse = response.isMakeSaleResponse;
                }

                GameObject btn = Instantiate(responseButtonPrefab, responseButtonContainer);
                btn.SetActive(true);
                var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                txt.text = responseText;

                Button buttonComponent = btn.GetComponent<Button>();

                if (chosenResponses[currentLineIdx].Contains(i))
                {
                    buttonComponent.interactable = true; // ✅ Let player click again
                    txt.color = Color.gray;
                }
                else
                {
                    txt.color = Color.white;
                }

                int responseIndex = i;

                buttonComponent.onClick.AddListener(() =>
                {
                    // Re-fetch response data inside the listener
                    string listenerResponseText;
                    int listenerNextIndex;
                    bool listenerReturnAfterResponse;
                    bool listenerIsMakeSaleResponse;

                    if (oldLine != null)
                    {
                        var response = oldLine.responses[responseIndex];
                        listenerResponseText = response.responseText;
                        listenerNextIndex = response.nextLineIndex;
                        listenerReturnAfterResponse = response.returnAfterResponse;
                        listenerIsMakeSaleResponse = response.isMakeSaleResponse;
                    }
                    else
                    {
                        var response = jsonLine.responses[responseIndex];
                        listenerResponseText = response.responseText;
                        listenerNextIndex = response.nextLineIndex;
                        listenerReturnAfterResponse = response.returnAfterResponse;
                        listenerIsMakeSaleResponse = response.isMakeSaleResponse;
                    }

                    //Sale logic
                    if (listenerIsMakeSaleResponse)
                    {
                        List<ItemCategory> requested = oldCustomer != null ? oldCustomer.requestedItems : currentJsonCustomer.requestedItems;
                        List<ItemCategory> present = GetItemCategoriesInCheckout();

                        if (!CheckRequestedItemsMatch(requested, present))
                        {
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

                    chosenResponses[currentLineIdx].Add(responseIndex);

                    if (listenerNextIndex >= 0)
                    {
                        if (listenerReturnAfterResponse)
                            dialogueReturnStack.Push(currentLineIdx);

                        currentLineIndex = listenerNextIndex;
                    }
                    else
                    {
                        currentLineIndex++;
                    }

                    ShowNextLine();
                });

            }
        }
        else
        {
            choicePanel.SetActive(false);

            if (!disableContinueButton)
            {
                continueButton.gameObject.SetActive(true);
            }
        }

        // Show the go back button if it's enabled
        if (showGoBackButton && goBackTargetIndex >= 0)
        {
            goBackButton.gameObject.SetActive(true);
            goBackButton.onClick.RemoveAllListeners();
            goBackButton.onClick.AddListener(() =>
            {
                currentLineIndex = goBackTargetIndex;
                ShowNextLine();
            });
        }
        else
        {
            goBackButton.gameObject.SetActive(false);
        }
    }

    void ShowScoreScreen(int screenIndex = 0)
    {
        foreach (var screen in scoreScreens)
        {
            if (screen != null) screen.SetActive(false);
        }

        if (screenIndex >= 0 && screenIndex < scoreScreens.Count && scoreScreens[screenIndex] != null)
        {
            scoreScreens[screenIndex].SetActive(true);
        }
        else
        {
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

    void SpawnIDFromJSON(CustomerScenarioData customerData)
    {
        if (string.IsNullOrEmpty(customerData.idPrefabPath) || idSpawnLocation == null)
        {
            Debug.LogWarning("Missing ID prefab path or spawn point for JSON customer.");
            return;
        }

        if (currentIDInstance != null)
        {
            Destroy(currentIDInstance);
        }

        GameObject idPrefab = Resources.Load<GameObject>(customerData.idPrefabPath);
        if (idPrefab == null)
        {
            Debug.LogError($"ID Prefab not found at path: {customerData.idPrefabPath}");
            return;
        }

        currentIDInstance = Instantiate(idPrefab, idSpawnLocation);
        RectTransform rect = currentIDInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
        }

        // IMPORTANT: We need to inject the ID data into the spawned ID prefab
        CustomerID customerIDComponent = currentIDInstance.GetComponent<CustomerID>();
        if (customerIDComponent != null)
        {
            // The JSON data structure is designed to hold the ID data directly
            // We need to manually transfer the data from the JSON object to the CustomerID component
            // Note: Sprite ProfileImage cannot be set from JSON string, it must be part of the prefab setup
            customerIDComponent.Name = customerData.idName;
            customerIDComponent.DOB = customerData.idDOB;
            customerIDComponent.Address = customerData.idAddress;
            customerIDComponent.Issuer = customerData.idIssuer;
            customerIDComponent.allowNameAccess = customerData.idAllowNameAccess;
            customerIDComponent.allowDOBAccess = customerData.idAllowDOBAccess;
            customerIDComponent.allowAddressAccess = customerData.idAllowAddressAccess;
            customerIDComponent.allowIssuerAccess = customerData.idAllowIssuerAccess;
            customerIDComponent.allowPictureAccess = customerData.idAllowPictureAccess;
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

    private List<ItemCategory> GetItemCategoriesInCheckout()
    {
        List<ItemCategory> categories = new List<ItemCategory>();
        foreach (Transform item in itemPickupManager.checkoutDropZone)
        {
            ItemSlotData data = item.GetComponent<ItemSlotData>();
            if (data != null)
            {
                categories.Add(data.category);
            }
        }
        return categories;
    }

    private bool CheckRequestedItemsMatch(List<ItemCategory> requested, List<ItemCategory> present)
    {
        if (requested.Count != present.Count)
            return false;

        // Make copies so we can modify them safely
        List<ItemCategory> requestedCopy = new List<ItemCategory>(requested);
        List<ItemCategory> presentCopy = new List<ItemCategory>(present);

        foreach (ItemCategory category in requestedCopy)
        {
            if (!presentCopy.Contains(category))
                return false;

            presentCopy.Remove(category); // Remove matched item to prevent duplicates
        }

        return presentCopy.Count == 0; // Make sure nothing extra remains
    }


}
