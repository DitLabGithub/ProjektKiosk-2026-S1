using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

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

        // Score System
        public int scoreValue = 0; // Points to add to morality score when this response is chosen
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

        // NEW FIELDS for Fridgy scenario
        public string idPrefabToSpawn = ""; // Specific ID prefab path (e.g., "ID_Prefabs/FridgyID" or "ID_Prefabs/OwnerID")
        public float displayDuration = 0f;  // Duration in seconds to wait before showing Continue button (0 = show immediately)
        public bool autoAdvanceToNext = false; // If true, automatically advance to next line after displayDuration (no Continue button)

        // Business Card System
        public bool showBusinessCard = false;
        public string businessCardImagePath = "";
        public bool waitForCardDismissal = false;

        // NPC Sprite Change System
        public string npcSprite = "";

        // Voice-Over System
        public AudioClip voiceOverClip;
        public string voiceOverPath = ""; // Path to load voice-over from Resources folder (e.g., "VoiceOvers/ShaunBaker_Line1")

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

    [Header("Voice-Over Audio")]
    public AudioSource voiceOverSource;

    [Header("UI References")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public GameObject choicePanel;
    public GameObject responseButtonPrefab;
    public Transform responseButtonContainer;
    public Transform idSpawnLocation;

    public IDInfoDisplay infoDisplay;

    [Header("Business Card System")]
    public GameObject businessCardPanel; // Panel to display business card
    public UnityEngine.UI.Image businessCardImage; // Image component for the card

    [Header("Dialogue Data")]
    public List<CustomerDialogue> customers = new();

    [Header("JSON Dialogue Data")]
    public List<string> jsonCustomerFileNames = new(); // DEPRECATED: Use ScenarioManager instead
    private CustomerScenarioData currentJsonCustomer;
    private GameObject currentJsonNpcObject; // New field to track the instantiated NPC object
    private string currentJsonScenarioFilename; // Track current scenario filename for completion notification

    [Header("Scenario Management")]
    public ScenarioManager scenarioManager; // Reference to ScenarioManager for shuffling

    [Header("UI Panels")]
    public List<GameObject> scoreScreens = new();
    public GameObject endCreditScreen;

    [Header("DEPRECATED - No longer needed, buttons are auto-detected")]
    public List<Button> scoreContinueButtons; // OLD: Manually assigned buttons (not used anymore)

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

    // Typewriter effect
    private Coroutine currentTypewriterCoroutine;
    private bool isTyping = false;
    private string currentFullText = ""; // Store full text for skip functionality


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

    // NEW: Coroutine to show Continue button after a delay
    private System.Collections.IEnumerator ShowContinueButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        continueButton.gameObject.SetActive(true);
    }

    // NEW: Typewriter effect coroutine with auto-advance support
    private System.Collections.IEnumerator TypewriterEffect(string fullText, float duration, bool showContinueAfter, bool autoAdvance = false)
    {
        isTyping = true;
        dialogueText.text = "";
        Debug.Log($"[DialogueManager] TypewriterEffect started. Text length: {fullText.Length}, Duration: {duration}s, isTyping: {isTyping}");

        // Calculate delay per character based on duration
        float delayPerChar = duration / fullText.Length;

        // Type out each character
        for (int i = 0; i < fullText.Length; i++)
        {
            // Check if typing was interrupted (by click or skip)
            if (!isTyping)
            {
                // Typing was interrupted, exit early
                yield break;
            }

            dialogueText.text += fullText[i];
            yield return new WaitForSeconds(delayPerChar);
        }

        isTyping = false;
        currentFullText = ""; // Clear stored text after completion

        // If auto-advance is enabled, automatically go to next line
        if (autoAdvance)
        {
            // Text stays visible briefly, then advances automatically
            currentLineIndex++;
            ShowNextLine();
        }
        else if (showContinueAfter)
        {
            // Show Continue button after typing is complete
            continueButton.gameObject.SetActive(true);
        }
    }

    // Business Card Display Methods
    void ShowBusinessCard(string imagePath)
    {
        if (businessCardPanel == null || businessCardImage == null)
        {
            Debug.LogWarning("Business card UI not set up properly!");
            return;
        }

        // Load the card image from Resources
        Sprite cardSprite = Resources.Load<Sprite>(imagePath);
        if (cardSprite == null)
        {
            Debug.LogError($"Business card image not found at path: {imagePath}");
            return;
        }

        businessCardImage.sprite = cardSprite;

        // Ensure the image is fully opaque
        Color imageColor = businessCardImage.color;
        imageColor.a = 1f;
        businessCardImage.color = imageColor;

        // Ensure CanvasGroup is fully opaque if it exists
        CanvasGroup canvasGroup = businessCardPanel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        businessCardPanel.SetActive(true);
    }

    void HideBusinessCard()
    {
        if (businessCardPanel != null)
        {
            businessCardPanel.SetActive(false);
        }
    }

    // NPC Sprite Change Method
    void ChangeNPCSprite(string spritePath)
    {
        if (string.IsNullOrEmpty(spritePath))
            return;

        // Load sprite from Resources/NPC_Sprites/
        Sprite newSprite = Resources.Load<Sprite>($"NPC_Sprites/{spritePath}");
        if (newSprite == null)
        {
            Debug.LogError($"NPC Sprite not found at path: NPC_Sprites/{spritePath}");
            return;
        }

        // Get the Image component from the current NPC object
        UnityEngine.UI.Image npcImage = null;

        // Try JSON customer first
        if (currentJsonNpcObject != null)
        {
            npcImage = currentJsonNpcObject.GetComponent<UnityEngine.UI.Image>();
            if (npcImage == null)
            {
                // If not on root, try to find it in children
                npcImage = currentJsonNpcObject.GetComponentInChildren<UnityEngine.UI.Image>();
            }
        }
        // Then try old customer system
        else if (currentCustomerIndex < customers.Count && customers[currentCustomerIndex].npcObject != null)
        {
            npcImage = customers[currentCustomerIndex].npcObject.GetComponent<UnityEngine.UI.Image>();
            if (npcImage == null)
            {
                npcImage = customers[currentCustomerIndex].npcObject.GetComponentInChildren<UnityEngine.UI.Image>();
            }
        }

        if (npcImage != null)
        {
            npcImage.sprite = newSprite;
        }
        else
        {
            Debug.LogWarning("Could not find Image component on NPC object to change sprite.");
        }
    }


    private void EnsureVoiceOverSource()
    {
        // Check if voiceOverSource is null, destroyed, or disabled
        if (voiceOverSource == null || !voiceOverSource.enabled)
        {
            // Try to find existing AudioSource on this GameObject or children
            AudioSource[] sources = GetComponentsInChildren<AudioSource>(true);

            // Look for one that's specifically for voice-overs (not used by other systems)
            foreach (var source in sources)
            {
                // Skip if it's used by SoundManager
                if (source.gameObject.name.ToLower().Contains("voiceover") ||
                    source.gameObject.name.ToLower().Contains("voice"))
                {
                    voiceOverSource = source;
                    voiceOverSource.enabled = true;
                    Debug.Log("Found and enabled existing voiceOverSource");
                    return;
                }
            }

            // If no suitable AudioSource found, create one
            GameObject voiceOverObj = new GameObject("VoiceOverSource");
            voiceOverObj.transform.SetParent(this.transform);
            voiceOverSource = voiceOverObj.AddComponent<AudioSource>();
            voiceOverSource.playOnAwake = false;
            voiceOverSource.loop = false;
            voiceOverSource.volume = 1.0f;
            Debug.Log("Created new voiceOverSource AudioSource");
        }
    }

    void Start()
    {
        // Ensure voiceOverSource is valid and enabled
        EnsureVoiceOverSource();

        continueButton.onClick.AddListener(NextLine);

        // IMPROVED: Automatically find and setup Continue buttons in each score screen
        foreach (var screen in scoreScreens)
        {
            if (screen != null)
            {
                screen.SetActive(false);

                // Find the Continue button within this score screen
                // First try to find a button with "Continue" in its name
                Button continueBtn = null;
                Button[] buttons = screen.GetComponentsInChildren<Button>(true);

                foreach (Button btn in buttons)
                {
                    if (btn.gameObject.name.ToLower().Contains("continue"))
                    {
                        continueBtn = btn;
                        break;
                    }
                }

                // If no "Continue" named button found, use the first button
                if (continueBtn == null && buttons.Length > 0)
                {
                    continueBtn = buttons[0];
                }

                if (continueBtn != null)
                {
                    // Remove any existing listeners to avoid duplicates
                    continueBtn.onClick.RemoveAllListeners();
                    continueBtn.onClick.AddListener(OnScoreContinue);
                }
                else
                {
                    Debug.LogWarning($"Score screen '{screen.name}' is missing a Continue button!");
                }
            }
        }

        continueButton.gameObject.SetActive(false);
        ClearDialogueUI();

        // Business Card System Setup
        if (businessCardPanel != null)
        {
            businessCardPanel.SetActive(false);
        }

        // NEW LOGIC: Force start if using only JSON customers (via ScenarioManager or legacy list)
        bool useScenarioManager = scenarioManager != null && scenarioManager.HasMoreScenarios();
        bool useLegacyList = customers.Count == 0 && jsonCustomerFileNames.Count > 0;

        if (useScenarioManager || useLegacyList)
        {
            // Set the index to the one before the first customer, so OnScoreContinue() advances to the first one (index 0).
            currentCustomerIndex = -1;
            // We call OnScoreContinue() to advance to the next (first JSON) customer.
            OnScoreContinue();
        }
    }

    void Update()
    {
        // Click anywhere on screen (or press Space) to skip typewriter effect
        if (isTyping && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            Debug.Log("[DialogueManager] Click detected while typing. Pointer over UI: " + UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject());

            // Skip regardless of pointer position (simplified check)
            SkipTypewriterEffect();
        }
    }

    private void SkipTypewriterEffect()
    {
        Debug.Log("[DialogueManager] SkipTypewriterEffect called. Full text: " + currentFullText);

        if (currentTypewriterCoroutine != null)
        {
            StopCoroutine(currentTypewriterCoroutine);
            currentTypewriterCoroutine = null;
        }
        isTyping = false;
        // Show the complete text
        dialogueText.text = currentFullText;
        currentFullText = "";
        // Show Continue button
        continueButton.gameObject.SetActive(true);

        Debug.Log("[DialogueManager] Skip complete. Text should now be visible.");
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

        // Check if this is an Authorization ID
        AuthorizationID authID = scannedID.GetComponent<AuthorizationID>();
        if (authID != null)
        {
            // Authorization ID detected - start authorization process
            Debug.Log("Authorization ID detected. Starting authorization process...");

            // Subscribe to authorization completion event
            authID.OnAuthorizationCompleted += OnAuthorizationComplete;

            // Start the authorization loading (UI is handled by AuthorizationUIManager)
            authID.StartAuthorization();

            // Do NOT show Continue button yet - wait for authorization to complete
            // Text removed per user request - authorization UI shows loading bar only
            // dialogueText.text = "Verifying authorization...";
        }
        else
        {
            // Regular ID - show Continue button immediately
            if (!choicePanel.activeSelf)
            {
                continueButton.gameObject.SetActive(true);
            }

            dialogueText.text = "ID scanned. Thank you!";
        }
    }

    private void OnAuthorizationComplete()
    {
        Debug.Log("Authorization complete! Updating UI and enabling Continue button.");

        // Update the ID info panel to show "Authorized" status
        UpdateIDInfoPanel();

        // Now show the Continue button
        if (!choicePanel.activeSelf)
        {
            continueButton.gameObject.SetActive(true);
        }

        // Text removed per user request - authorization UI shows checkmark only
        // dialogueText.text = "Authorization verified. Thank you!";
    }
    void OnScoreContinue()
    {
        // Play button click sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayContinueButtonClick();
        }

        foreach (var screen in scoreScreens)
        {
            if (screen != null) screen.SetActive(false);
        }
        ResetDialogueState();

        // Check if the scenario that just finished was the Police Arrest scenario
        // If so, END THE GAME (don't continue to more scenarios)
        if (!string.IsNullOrEmpty(currentJsonScenarioFilename) &&
            currentJsonScenarioFilename == "PoliceGuy2Scenario")
        {
            Debug.Log("[DialogueManager] Police Arrest scenario completed. Game Over - showing end credits.");

            // Show game over / end credits
            dialogueText.text = "Game Over - You were arrested.";
            speakerText.text = "";
            endCreditScreen.gameObject.SetActive(true);
            return; // Exit method - do NOT continue to next scenario
        }

        // Notify ScenarioManager that the previous scenario is complete
        if (!string.IsNullOrEmpty(currentJsonScenarioFilename) && scenarioManager != null)
        {
            scenarioManager.OnScenarioComplete(currentJsonScenarioFilename);
        }

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

        // Determine if we should use ScenarioManager or legacy list
        bool useScenarioManager = scenarioManager != null;

        // Check if there are more scenarios to play (includes dynamically injected police scenarios)
        bool hasMoreScenarios = (currentCustomerIndex < customers.Count) ||
                                (useScenarioManager && scenarioManager.HasMoreScenarios()) ||
                                (!useScenarioManager && currentCustomerIndex < customers.Count + jsonCustomerFileNames.Count);

        if (hasMoreScenarios)
        {
            if (currentCustomerIndex < customers.Count)
            {
                // Old Customer Logic (Hoss, Robin, Amon)
                if (customers[currentCustomerIndex].npcObject != null)
                {
                    customers[currentCustomerIndex].npcObject.SetActive(true);
                }
                currentJsonCustomer = null; // Ensure JSON data is cleared
                currentJsonScenarioFilename = ""; // Clear scenario filename
                ShowNextLine();
            }
            else
            {
                // New JSON Customer Logic - Use ScenarioManager if available
                string filename;

                if (useScenarioManager && scenarioManager.HasMoreScenarios())
                {
                    // Use ScenarioManager for shuffled/queued scenarios
                    filename = scenarioManager.GetNextScenarioFilename();
                }
                else
                {
                    // Fallback to legacy list
                    int jsonIndex = currentCustomerIndex - customers.Count;
                    filename = jsonCustomerFileNames[jsonIndex];
                }

                currentJsonScenarioFilename = filename; // Track current scenario
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
                    // Clear the scenario queue to force game end
                    if (useScenarioManager && scenarioManager != null)
                    {
                        // ScenarioManager will return false for HasMoreScenarios() when queue is empty
                        // For now, just set a high index to skip to end
                        currentCustomerIndex = 9999;
                    }
                    else
                    {
                        currentCustomerIndex = customers.Count + jsonCustomerFileNames.Count;
                    }
                    OnScoreContinue(); // Re-call to show end screen
                }
            }
        }
        else
        {
            // End of all customers
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "TutorialScene")
            {
                // Tutorial is complete, transition to gameplay scene
                SceneManager.LoadScene("GameplayScene");
            }
            else
            {
                // In gameplay scene, show end credits
                dialogueText.text = "All customers served!";
                speakerText.text = "";
                endCreditScreen.gameObject.SetActive(true);
            }
        }
    }


    void UpdateIDInfoPanel()
    {
        if (currentScannedIDData == null || infoDisplay == null)
        {
            if (infoDisplay != null)
                infoDisplay.HideInfo();
            return;
        }

        // Display ID information based on access permissions
        string displayName = currentScannedIDData.allowNameAccess ? currentScannedIDData.Name : "";
        string displayDOB = currentScannedIDData.allowDOBAccess ? currentScannedIDData.DOB : "";
        string displayAddress = currentScannedIDData.allowAddressAccess ? currentScannedIDData.Address : "";
        string displayIssuer = currentScannedIDData.allowIssuerAccess ? currentScannedIDData.Issuer : "";
        Sprite displayImage = currentScannedIDData.allowPictureAccess ? currentScannedIDData.ProfileImage : null;

        // Check if this is an Authorization ID and get the current authorization status
        bool isAuthID = currentScannedIDData.isAuthorizationID;
        string authStatus = currentScannedIDData.authorizationStatus;

        // Use the appropriate ShowInfo overload
        if (isAuthID)
        {
            infoDisplay.ShowInfo(displayName, displayDOB, displayAddress, displayIssuer, displayImage, true, authStatus);
        }
        else
        {
            infoDisplay.ShowInfo(displayName, displayDOB, displayAddress, displayIssuer, displayImage);
        }
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
        // Play button click sound
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayContinueButtonClick();
        }

        // If currently typing, complete the text immediately and show Continue button
        if (isTyping)
        {
            if (currentTypewriterCoroutine != null)
            {
                StopCoroutine(currentTypewriterCoroutine);
                currentTypewriterCoroutine = null;
            }
            isTyping = false;
            // Show the complete text
            dialogueText.text = currentFullText;
            currentFullText = "";
            // Show Continue button
            continueButton.gameObject.SetActive(true);
            return;
        }

        if (waitingForAction)
        {
            dialogueText.text = "Please complete the required action.";
            return;
        }

        // Check if current line has a business card and should end conversation
        bool shouldEndAfterCard = false;
        int endScoreIndex = 0;

        if (currentJsonCustomer != null && currentLineIndex < currentJsonCustomer.lines.Count)
        {
            var currentLine = currentJsonCustomer.lines[currentLineIndex];
            if (currentLine.showBusinessCard && currentLine.endConversationHere)
            {
                shouldEndAfterCard = true;
                endScoreIndex = currentLine.scoreScreenIndex;
            }
        }

        // Hide business card if it's showing
        HideBusinessCard();

        // If the line had a card and should end, show score screen now
        if (shouldEndAfterCard)
        {
            ShowScoreScreen(endScoreIndex);
            if (itemPickupManager != null)
            {
                itemPickupManager.ReturnCarriedItemsToShelf();
            }
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
                        isMakeSaleResponse = r.isMakeSaleResponse,
                        scoreValue = r.scoreValue // Copy score value from JSON data
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
                    idPrefabToSpawn = jsonLineData.idPrefabToSpawn,
                    displayDuration = jsonLineData.displayDuration,
                    autoAdvanceToNext = jsonLineData.autoAdvanceToNext,
                    showBusinessCard = jsonLineData.showBusinessCard,
                    businessCardImagePath = jsonLineData.businessCardImagePath,
                    waitForCardDismissal = jsonLineData.waitForCardDismissal,
                    npcSprite = jsonLineData.npcSprite,
                    voiceOverPath = jsonLineData.voiceOverPath,
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
                    // Play button click sound
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayGenericButtonClick();
                    }

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

                    // Score System: Add points if this response has a scoreValue
                    if (currentResponse.scoreValue > 0 && ScoreManager.Instance != null)
                    {
                        ScoreManager.Instance.AddScore(currentResponse.scoreValue);
                        Debug.Log($"[DialogueManager] Added {currentResponse.scoreValue} points for response: '{currentResponse.responseText}'");
                    }

                    if (currentResponse.nextLineIndex >= 0)
                    {
                        if (currentResponse.returnAfterResponse)
                            dialogueReturnStack.Push(currentLineIndex);

                        // For JSON scenarios: convert editorIndex to array index
                        // For old scenarios: use nextLineIndex directly as array index
                        if (isJsonCustomer)
                        {
                            int targetIndex = FindLineIndexByEditorIndex(currentResponse.nextLineIndex);
                            if (targetIndex >= 0)
                            {
                                currentLineIndex = targetIndex;
                            }
                            else
                            {
                                Debug.LogError($"Invalid nextLineIndex (editorIndex): {currentResponse.nextLineIndex}");
                                return;
                            }
                        }
                        else
                        {
                            // Old system uses direct array indices
                            //currentLineIndex = currentResponse.nextLineIndex;
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

            // Only show Continue button here if NOT using typewriter effect
            // (typewriter effect handles showing the button after typing completes)
            if (!line.disableContinueButton && line.displayDuration <= 0)
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
                // Play button click sound
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayGenericButtonClick();
                }

                // For JSON scenarios: convert editorIndex to array index
                // For old scenarios: use goBackTargetIndex directly as array index
                if (isJsonCustomer)
                {
                    int targetIndex = FindLineIndexByEditorIndex(line.goBackTargetIndex);
                    if (targetIndex >= 0)
                    {
                        currentLineIndex = targetIndex;
                        ShowNextLine();
                    }
                    else
                    {
                        Debug.LogError($"Invalid goBackTargetIndex (editorIndex): {line.goBackTargetIndex}");
                    }
                }
                else
                {
                    // Old system uses direct array indices
                    currentLineIndex = line.goBackTargetIndex;
                    ShowNextLine();
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

        // Play voice-over if available
        // Ensure voiceOverSource is valid before using it
        EnsureVoiceOverSource();

        if (voiceOverSource != null && voiceOverSource.enabled)
        {
            AudioClip clipToPlay = line.voiceOverClip;

            // If no clip is assigned but a path is provided, try loading from Resources
            if (clipToPlay == null && !string.IsNullOrEmpty(line.voiceOverPath))
            {
                clipToPlay = Resources.Load<AudioClip>(line.voiceOverPath);
                if (clipToPlay == null)
                {
                    Debug.LogWarning($"Voice-over not found at path: {line.voiceOverPath}");
                }
            }

            // Play the clip if we have one
            if (clipToPlay != null)
            {
                voiceOverSource.Stop(); // Stop any currently playing voice-over
                voiceOverSource.PlayOneShot(clipToPlay);
                Debug.Log($"Playing voice-over: {line.voiceOverPath ?? "assigned clip"}");
            }
        }

        // Stop any existing typewriter effect
        if (currentTypewriterCoroutine != null)
        {
            StopCoroutine(currentTypewriterCoroutine);
            currentTypewriterCoroutine = null;
            isTyping = false;
        }

        // Use typewriter effect if displayDuration is set, otherwise show text immediately
        if (line.displayDuration > 0 && !line.disableContinueButton)
        {
            // Store full text for skip functionality
            currentFullText = line.text;
            // Start typewriter effect with auto-advance if specified
            bool showContinue = !line.autoAdvanceToNext; // Only show Continue button if NOT auto-advancing
            currentTypewriterCoroutine = StartCoroutine(TypewriterEffect(line.text, line.displayDuration, showContinue, line.autoAdvanceToNext));
        }
        else
        {
            // Show text immediately
            dialogueText.text = line.text;
            currentFullText = "";
        }

        if (currentScannedIDData != null)
            UpdateIDInfoPanel();

        // 8. Handle Business Card Display
        if (line.showBusinessCard && !string.IsNullOrEmpty(line.businessCardImagePath))
        {
            ShowBusinessCard(line.businessCardImagePath);
            // Don't block the Continue button - let player dismiss card by clicking Continue
        }

        // 8.5. Handle NPC Sprite Change
        if (!string.IsNullOrEmpty(line.npcSprite))
        {
            ChangeNPCSprite(line.npcSprite);
        }

        // 9. Handle ID request
        if (line.askForID)
        {
            waitingForAction = true;
            continueButton.gameObject.SetActive(false);
            if (isJsonCustomer)
            {
                // Use line-specific ID prefab if specified, otherwise fall back to customer default
                string idPath = string.IsNullOrEmpty(line.idPrefabToSpawn)
                    ? currentJsonCustomer.idPrefabPath
                    : line.idPrefabToSpawn;
                SpawnIDFromJSON(idPath);
            }
            else
            {
                SpawnID(customers[currentCustomerIndex]);
            }
            return;
        }

        // 10. Handle end of conversation (only if NOT showing a business card)
        if (line.endConversationHere && !line.showBusinessCard)
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
        // Hide business card to prevent it from carrying over
        HideBusinessCard();

        // Hide authorization UI (checkmark and loading bar) when scenario ends
        if (AuthorizationUIManager.Instance != null)
        {
            AuthorizationUIManager.Instance.HideAll();
        }

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

    void SpawnIDFromJSON(string idPrefabPath)
    {
        if (string.IsNullOrEmpty(idPrefabPath) || idSpawnLocation == null)
        {
            Debug.LogWarning("Missing ID prefab path or spawn point for JSON customer.");
            return;
        }

        if (currentIDInstance != null)
        {
            Destroy(currentIDInstance);
        }

        GameObject idPrefab = Resources.Load<GameObject>(idPrefabPath);
        if (idPrefab == null)
        {
            Debug.LogError($"ID Prefab not found at path: {idPrefabPath}");
            return;
        }

        currentIDInstance = Instantiate(idPrefab, idSpawnLocation);
        RectTransform rect = currentIDInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
        }

        // IMPORTANT: Check if this is the default customer ID, if so, inject the customer's ID data
        // Otherwise, use the prefab's own data (for scenarios with multiple different IDs like Fridgy)
        if (currentJsonCustomer != null && idPrefabPath == currentJsonCustomer.idPrefabPath)
        {
            CustomerID customerIDComponent = currentIDInstance.GetComponent<CustomerID>();
            if (customerIDComponent != null)
            {
                // This is the default customer ID, so inject data from the JSON
                customerIDComponent.Name = currentJsonCustomer.idName;
                customerIDComponent.DOB = currentJsonCustomer.idDOB;
                customerIDComponent.Address = currentJsonCustomer.idAddress;
                customerIDComponent.Issuer = currentJsonCustomer.idIssuer;
                customerIDComponent.allowNameAccess = currentJsonCustomer.idAllowNameAccess;
                customerIDComponent.allowDOBAccess = currentJsonCustomer.idAllowDOBAccess;
                customerIDComponent.allowAddressAccess = currentJsonCustomer.idAllowAddressAccess;
                customerIDComponent.allowIssuerAccess = currentJsonCustomer.idAllowIssuerAccess;
                customerIDComponent.allowPictureAccess = currentJsonCustomer.idAllowPictureAccess;
            }
        }
        // If it's a different ID prefab (like OwnerID when FridgyID is default), the prefab's own data will be used
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

        // Hide authorization UI when resetting for new scenario
        if (AuthorizationUIManager.Instance != null)
        {
            AuthorizationUIManager.Instance.HideAll();
        }
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
        HideBusinessCard(); // Hide card when clearing UI
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
