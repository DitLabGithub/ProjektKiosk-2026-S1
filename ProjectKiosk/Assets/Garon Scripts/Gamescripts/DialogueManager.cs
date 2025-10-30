using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

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

        // NEW LOGIC: Force start if using only JSON customers
        if (customers.Count == 0 && jsonCustomerFileNames.Count > 0)
        {
            // Set the index to the one before the first customer, so OnScoreContinue() advances to the first one (index 0).
            currentCustomerIndex = -1;
            // We call OnScoreContinue() to advance to the next (first JSON) customer.
            OnScoreContinue();
        }
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

        // Index of the customer that was just on screen (before increment)
        int previousCustomerIndex = currentCustomerIndex;

        currentCustomerIndex++; // Advance to the next customer
        currentLineIndex = 0;

        // Deactivate the previous NPC object if it exists (handles both old and new)
        if (previousCustomerIndex >= 0)
        {
            if (previousCustomerIndex < customers.Count && customers[previousCustomerIndex].npcObject != null)
            {
                // Old Customer Logic
                customers[previousCustomerIndex].npcObject.SetActive(false);
            }
            else if (currentJsonNpcObject != null)
            {
                // JSON Customer Logic
                Destroy(currentJsonNpcObject);
                currentJsonNpcObject = null;
            }
        }

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
                            // Removed: rect.anchoredPosition = Vector2.zero; to allow prefab's position to be used.
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
        // NOTE: The original UpdateInfo call is removed to fix the compilation error.
        // The display logic should be handled by IDInfoDisplay.cs, which we don't have.
        // We will stick to the original code's intent of simply calling HideInfo if data is null.
    }

    // Helper method to convert editorIndex to actual array index
    private int FindLineIndexByEditorIndex(int editorIndex)
    {
        if (currentJsonCustomer != null)
        {
            for (int i = 0; i < currentJsonCustomer.lines.Count; i++)
            {
                if (currentJsonCustomer.lines[i].editorIndex == editorIndex)
                    return i;
            }
        }
        else if (currentCustomerIndex < customers.Count)
        {
            for (int i = 0; i < customers[currentCustomerIndex].lines.Count; i++)
            {
                if (customers[currentCustomerIndex].lines[i].editorIndex == editorIndex)
                    return i;
            }
        }
        Debug.LogWarning($"Could not find line with editorIndex {editorIndex}");
        return -1;
    }

    public void NextLine()
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

    void ShowNextLine()
    {
        // 1. Determine the current customer's data source (old list or new JSON)
        DialogueLine line = null;
        List<DialogueResponse> responses = null;
        List<ItemCategory> requestedItems = null;

        bool isJsonCustomer = currentJsonCustomer != null;

        if (isJsonCustomer)
        {
            if (currentLineIndex < currentJsonCustomer.lines.Count)
            {
                // Convert the JSON data structure (DialogueLineData) to the internal structure (DialogueLine)
                // This resolves the 'conditional expression' error by explicitly mapping the types.
                var jsonLineData = currentJsonCustomer.lines[currentLineIndex];
                line = new DialogueLine
                {
                    editorIndex = jsonLineData.editorIndex,
                    speaker = jsonLineData.speaker,
                    text = jsonLineData.text,
                    responses = jsonLineData.responses.Select(r => new DialogueResponse
                    {
                        responseText = r.responseText,
                        nextLineIndex = r.nextLineIndex,
                        returnAfterResponse = r.returnAfterResponse,
                        activateContinueAfterChoice = r.activateContinueAfterChoice,
                        isMakeSaleResponse = r.isMakeSaleResponse
                    }).ToList(),
                    askForID = jsonLineData.askForID,
                    showGoBackButton = jsonLineData.showGoBackButton,
                    goBackTargetIndex = jsonLineData.goBackTargetIndex,
                    grantNameAccess = jsonLineData.grantNameAccess,
                    grantDOBAccess = jsonLineData.grantDOBAccess,
                    grantAddressAccess = jsonLineData.grantAddressAccess,
                    grantIssuerAccess = jsonLineData.grantIssuerAccess,
                    grantPictureAccess = jsonLineData.grantPictureAccess,
                    disableContinueButton = jsonLineData.disableContinueButton,
                    endConversationHere = jsonLineData.endConversationHere,
                    scoreScreenIndex = jsonLineData.scoreScreenIndex
                };
                responses = line.responses;
                requestedItems = currentJsonCustomer.requestedItems;
            }
        }
        else if (currentCustomerIndex < customers.Count)
        {
            if (currentLineIndex < customers[currentCustomerIndex].lines.Count)
            {
                line = customers[currentCustomerIndex].lines[currentLineIndex];
                responses = line.responses;
                requestedItems = customers[currentCustomerIndex].requestedItems;
            }
        }

        // 2. Handle end of dialogue for current customer
        if (line == null)
        {
            OnScoreContinue();
            return;
        }

        // 3. Clear UI elements
        ClearResponses();
        continueButton.gameObject.SetActive(false);

        // 4. Handle choices
        if (responses != null && responses.Count > 0)
        {
            choicePanel.SetActive(true);

            if (!chosenResponses.ContainsKey(currentLineIndex))
                chosenResponses[currentLineIndex] = new HashSet<int>();

            for (int i = 0; i < responses.Count; i++)
            {
                var response = responses[i];
                GameObject btn = Instantiate(responseButtonPrefab, responseButtonContainer);
                btn.SetActive(true);
                var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                txt.text = response.responseText;

                Button buttonComponent = btn.GetComponent<Button>();

                if (chosenResponses[currentLineIndex].Contains(i))
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
                    // Determine customer data source again inside the listener
                    List<ItemCategory> currentRequestedItems = isJsonCustomer ? currentJsonCustomer.requestedItems : customers[currentCustomerIndex].requestedItems;
                    var currentResponse = isJsonCustomer ? line.responses[responseIndex] : customers[currentCustomerIndex].lines[currentLineIndex].responses[responseIndex];

                    //Sale logic
                    if (currentResponse.isMakeSaleResponse)
                    {
                        List<ItemCategory> requested = currentRequestedItems;
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

                    chosenResponses[currentLineIndex].Add(responseIndex);

                    if (currentResponse.nextLineIndex >= 0)
                    {
                        if (currentResponse.returnAfterResponse)
                            dialogueReturnStack.Push(currentLineIndex);

                        // Convert editorIndex to array index
                        int targetIndex = FindLineIndexByEditorIndex(currentResponse.nextLineIndex);
                        if (targetIndex >= 0)
                        {
                            currentLineIndex = targetIndex;
                        }
                        else
                        {
                            Debug.LogError($"Invalid nextLineIndex: {currentResponse.nextLineIndex}");
                            return;
                        }
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

            if (!line.disableContinueButton)
            {
                continueButton.gameObject.SetActive(true);
            }
        }

        // 5. Show the go back button if it's enabled
        if (line.showGoBackButton && line.goBackTargetIndex >= 0)
        {
            goBackButton.gameObject.SetActive(true);
            goBackButton.onClick.RemoveAllListeners();
            goBackButton.onClick.AddListener(() =>
            {
                // Convert editorIndex to array index
                int targetIndex = FindLineIndexByEditorIndex(line.goBackTargetIndex);
                if (targetIndex >= 0)
                {
                    currentLineIndex = targetIndex;
                    ShowNextLine();
                }
                else
                {
                    Debug.LogError($"Invalid goBackTargetIndex: {line.goBackTargetIndex}");
                }
            });
        }
        else
        {
            goBackButton.gameObject.SetActive(false);
        }

        // 6. Handle data access grants
        if (currentScannedIDData != null)
        {
            bool accessChanged = false;

            if (line.grantNameAccess && !currentScannedIDData.allowNameAccess) { currentScannedIDData.allowNameAccess = true; accessChanged = true; }
            if (line.grantDOBAccess && !currentScannedIDData.allowDOBAccess) { currentScannedIDData.allowDOBAccess = true; accessChanged = true; }
            if (line.grantAddressAccess && !currentScannedIDData.allowAddressAccess) { currentScannedIDData.allowAddressAccess = true; accessChanged = true; }
            if (line.grantIssuerAccess && !currentScannedIDData.allowIssuerAccess) { currentScannedIDData.allowIssuerAccess = true; accessChanged = true; }
            if (line.grantPictureAccess && !currentScannedIDData.allowPictureAccess) { currentScannedIDData.allowPictureAccess = true; accessChanged = true; }

            if (accessChanged) UpdateIDInfoPanel();
        }

        // 7. Update UI Text
        speakerText.text = line.speaker;
        dialogueText.text = line.text;
        if (currentScannedIDData != null)
            UpdateIDInfoPanel();

        // 8. Handle ID request
        if (line.askForID)
        {
            waitingForAction = true;
            continueButton.gameObject.SetActive(false);
            if (isJsonCustomer)
            {
                SpawnIDFromJSON(currentJsonCustomer);
            }
            else
            {
                SpawnID(customers[currentCustomerIndex]);
            }
            return;
        }

        // 9. Handle end of conversation
        if (line.endConversationHere)
        {
            if (dialogueReturnStack.Count > 0)
            {
                // This logic seems incorrect based on the original code's intent for returnAfterResponse
                // The original code uses a coroutine for this. I will keep the original logic's intent.
                ShowScoreScreen(line.scoreScreenIndex);
            }
            else
            {
                ShowScoreScreen(line.scoreScreenIndex);
            }
            if (itemPickupManager != null)
            {
                itemPickupManager.ReturnCarriedItemsToShelf();
            }
            return;
        }

        // 10. Handle choices again (for lines that were not handled in step 4, though this is redundant)
        if (responses != null && responses.Count > 0)
        {
            choicePanel.SetActive(true);
            continueButton.gameObject.SetActive(false);
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
