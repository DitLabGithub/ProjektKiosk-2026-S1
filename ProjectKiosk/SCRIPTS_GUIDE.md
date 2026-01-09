# GAME SCRIPTS COMMUNICATION GUIDE
**For Interns: How the Game's 27 Scripts Work Together**

---

## TABLE OF CONTENTS
1. [Overview: Script Architecture](#overview-script-architecture)
2. [Communication Methods](#communication-methods)
3. [Core Systems](#core-systems)
4. [Script-by-Script Reference](#script-by-script-reference)
5. [Data Flow Examples](#data-flow-examples)
6. [Quick Reference Tables](#quick-reference-tables)

---

## OVERVIEW: SCRIPT ARCHITECTURE

### The Big Picture
```
┌─────────────────────────────────────────────────────────────────┐
│                      GAME ARCHITECTURE                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────┐      ┌──────────────┐      ┌──────────────┐ │
│  │   MANAGERS   │◄────►│   HANDLERS   │◄────►│   UI SCRIPTS │ │
│  │  (Singletons)│      │  (Workers)   │      │  (Display)   │ │
│  └──────────────┘      └──────────────┘      └──────────────┘ │
│         │                      │                      │         │
│         │                      │                      │         │
│         └──────────────────────┴──────────────────────┘         │
│                                │                                │
│                         ┌──────▼──────┐                        │
│                         │ DATA CLASSES │                        │
│                         │  (Storage)   │                        │
│                         └──────────────┘                        │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Script Categories

**1. MANAGER SCRIPTS (The Bosses)**
- Control major game systems
- Usually singletons (only one exists)
- Persist across scenes or control entire scenes
- Examples: DialogueManager, ScoreManager, SoundManager

**2. HANDLER SCRIPTS (The Workers)**
- Handle specific tasks for managers
- Attached to individual game objects
- Do one job well
- Examples: ItemPickupManager, SaleManager, IDScanner

**3. UI SCRIPTS (The Display)**
- Show information to player
- Update visuals based on data
- Usually don't contain game logic
- Examples: IDInfoDisplay, MessageItemUI, InboxUIManager

**4. DATA CLASSES (The Storage)**
- Store information (no logic)
- Serializable (can be saved/loaded)
- Passed between scripts
- Examples: CustomerData, DialogueLineData, MessageData

**5. HELPER SCRIPTS (The Utilities)**
- Provide useful functions
- Small, focused purpose
- Reusable across game
- Examples: TypeWriter, FadeController, DraggableItem

---

## COMMUNICATION METHODS

### Method 1: Direct Reference (Most Common)
```csharp
// Script A has a public reference to Script B
public DialogueManager dialogueManager;

// Script A calls Script B directly
dialogueManager.StartDialogue();
```
**When Used:** Parent-child relationships, UI updating managers
**Example:** CustomerUIHandler → DialogueManager

### Method 2: Singleton Pattern (For Managers)
```csharp
// Manager makes itself globally accessible
public static ScoreManager Instance;

// Any script can access it
ScoreManager.Instance.AddScore(10);
```
**When Used:** Scripts that need to be accessed from anywhere
**Example:** ScoreManager, SoundManager, InboxUIManager

### Method 3: Events (For Broadcasting)
```csharp
// Manager declares event
public event Action<int> OnScoreChanged;

// Manager fires event
OnScoreChanged?.Invoke(newScore);

// Other scripts subscribe to event
ScoreManager.Instance.OnScoreChanged += OnScoreUpdated;
```
**When Used:** One script needs to notify multiple scripts
**Example:** ScoreManager → InboxUIManager, ScenarioManager

### Method 4: GetComponent (Finding Attached Scripts)
```csharp
// Find script on same GameObject
NPCRequest request = npcObject.GetComponent<NPCRequest>();

// Find script in children
IDScanner scanner = GetComponentInChildren<IDScanner>();
```
**When Used:** Scripts attached to same object or related objects
**Example:** DialogueManager finding NPCRequest on customer

### Method 5: Unity Events (Inspector-Configured)
```csharp
// Button click in Inspector → calls script method
public void OnButtonClick() { ... }
```
**When Used:** UI buttons, Unity event triggers
**Example:** Continue button → DialogueManager.OnContinue()

---

## CORE SYSTEMS

### SYSTEM 1: DIALOGUE SYSTEM
**Purpose:** Handle all customer conversations and story flow

#### Scripts Involved:
```
DialogueManager.cs          ← BOSS (controls everything)
    ↓ uses
CustomerScenarioDialogue.cs ← DATA (dialogue line structure)
CustomerData.cs             ← DATA (customer info storage)
CustomerUIHandler.cs        ← WORKER (click detection on NPC)
TypeWriter.cs               ← HELPER (animated text effect)
```

#### How It Works:
```
1. Player clicks NPC
   CustomerUIHandler.OnPointerClick()
        ↓ calls
   DialogueManager.StartDialogue()

2. DialogueManager loads JSON scenario
   Reads CustomerScenarioData (lines, responses, ID info)

3. DialogueManager shows dialogue line
   Uses TypeWriter to animate text
   Creates response buttons dynamically

4. Player clicks response button
   Button fires → DialogueManager.OnResponseClicked()
   Adds corruption score (if any)
   Jumps to next dialogue line

5. Scenario ends
   DialogueManager.ShowScoreScreen()
   Loads next scenario from ScenarioManager
```

#### Key Communication:
- **CustomerUIHandler → DialogueManager**: "Player clicked NPC, start talking!"
- **DialogueManager → TypeWriter**: "Animate this text"
- **DialogueManager → ScoreManager**: "Add corruption points"
- **DialogueManager → SoundManager**: "Play continue button sound"

---

### SYSTEM 2: SCENARIO MANAGEMENT
**Purpose:** Control which customer appears and in what order

#### Scripts Involved:
```
ScenarioManager.cs       ← BOSS (queue controller)
    ↑ requests
DialogueManager.cs       ← CONSUMER (asks for next scenario)
    ↑ triggers
ScoreManager.cs          ← TRIGGER (police scenario injection)
```

#### How It Works:
```
1. Game starts
   ScenarioManager.LoadConfig()
   Reads ScenarioConfig.json
   Builds queue: [Fridgy, Amon, Robin, ...]

2. Scenario complete
   DialogueManager asks: "Any more scenarios?"
   ScenarioManager.GetNextScenarioFilename()
   Returns: "Amon_Scenario"

3. Player gets high corruption (50 points)
   ScoreManager fires event
   ScenarioManager receives event
   Injects police scenario at front of queue
   Queue becomes: [Police_Warning, Robin, ...]

4. DialogueManager loads police scenario next
```

#### Key Communication:
- **DialogueManager → ScenarioManager**: "Give me next scenario filename"
- **ScoreManager → ScenarioManager**: "Inject police scenario!" (event)
- **ScenarioManager → DialogueManager**: "Here's the filename to load"

---

### SYSTEM 3: SCORING & CORRUPTION
**Purpose:** Track player's unethical choices and trigger consequences

#### Scripts Involved:
```
ScoreManager.cs          ← BOSS (corruption tracker)
    ↑ reports
DialogueManager.cs       ← REPORTER (adds score when player chooses)
    ↓ notifies (events)
InboxUIManager.cs        ← LISTENER (shows email warnings)
ScenarioManager.cs       ← LISTENER (injects police scenarios)
```

#### How It Works:
```
1. Player makes bad choice
   Response has scoreValue: 15
   DialogueManager calls:
        ScoreManager.Instance.AddScore(15)

2. ScoreManager updates score
   currentScore += 15
   Checks if thresholds crossed

3. Threshold crossed (e.g., 30 points)
   ScoreManager fires event:
        OnInboxThresholdReached(threshold)

4. InboxUIManager receives event
   Creates warning email:
        "Kiosk Flagged for Review"
   Shows notification badge

5. High threshold crossed (50 points)
   ScoreManager fires event:
        OnPoliceThresholdReached(scenario)

6. ScenarioManager receives event
   Injects police scenario into queue
```

#### Key Communication:
- **DialogueManager → ScoreManager**: "Add 15 corruption points"
- **ScoreManager → InboxUIManager**: "Show email warning!" (event)
- **ScoreManager → ScenarioManager**: "Inject police scenario!" (event)

---

### SYSTEM 4: ID VERIFICATION
**Purpose:** Scan customer IDs and reveal personal data progressively

#### Scripts Involved:
```
IDScanner.cs            ← WORKER (detects ID card drag)
    ↓ reports to
DialogueManager.cs      ← BOSS (processes ID data)
    ↓ updates
IDInfoDisplay.cs        ← UI (shows ID panel)
    ↑ reads from
CustomerID.cs           ← DATA (ID information storage)
CustomerData.cs         ← DATA (runtime ID data with access flags)
```

#### How It Works:
```
1. Dialogue triggers ID request
   DialogueLineData has: askForID = true
   DialogueManager spawns ID prefab

2. Player drags ID to scanner
   IDScanner.OnTriggerEnter2D()
   Detects ID with CustomerID component

3. IDScanner notifies DialogueManager
   IDScanner calls:
        dialogueManager.OnIDScanned(customerID)

4. DialogueManager stores ID data
   Creates CustomerData from CustomerID
   Applies access permissions:
        grantNameAccess = true → show name
        grantDOBAccess = true → show birthdate

5. IDInfoDisplay updates panel
   DialogueManager calls:
        UpdateIDInfoPanel()
   Shows "[Access Denied]" for blocked fields
   Shows actual data for granted fields

6. ID destroyed
   GameObject.Destroy(idObject)
   Continue button appears
```

#### Key Communication:
- **DialogueManager → ID Prefab**: "Spawn ID card"
- **IDScanner → DialogueManager**: "ID scanned, here's the data"
- **DialogueManager → IDInfoDisplay**: "Update panel with this data"

---

### SYSTEM 5: AUTHORIZATION SYSTEM (Special IDs)
**Purpose:** Some IDs require loading verification (government authorization)

#### Scripts Involved:
```
AuthorizationID.cs           ← SPECIAL (authorization-required ID)
    ↓ triggers
AuthorizationUIManager.cs    ← UI (loading bar animation)
    ↓ notifies
DialogueManager.cs           ← LISTENER (waits for completion)
```

#### How It Works:
```
1. Special ID scanned
   IDScanner detects AuthorizationID component
   DialogueManager.OnIDScanned() checks:
        if (id.GetComponent<AuthorizationID>())

2. Authorization starts
   DialogueManager calls:
        authorizationID.StartAuthorization()
   DialogueManager subscribes to event:
        authorizationID.OnAuthorizationCompleted

3. Loading animation plays
   AuthorizationID calls:
        AuthorizationUIManager.StartAuthorization()
   Loading bar animates for 5 seconds
   Checkmark appears when complete

4. Authorization completes
   AuthorizationID fires event:
        OnAuthorizationCompleted?.Invoke()

5. DialogueManager receives completion
   Shows Continue button
   Dialogue resumes
```

#### Key Communication:
- **AuthorizationID → AuthorizationUIManager**: "Show loading animation"
- **AuthorizationUIManager → AuthorizationID**: "Animation complete"
- **AuthorizationID → DialogueManager**: "Authorization done!" (event)

---

### SYSTEM 6: ITEM MANAGEMENT & SALES
**Purpose:** Handle item pickup, checkout, and sales validation

#### Scripts Involved:
```
ItemPickupManager.cs     ← WORKER (pickup/drop logic)
    ↓ validates with
SaleManager.cs           ← VALIDATOR (checks if items match)
    ↓ reads from
NPCRequest.cs            ← DATA (what customer wants)
    ↑ attached to
ItemSlotData.cs          ← DATA (item info: category, price)
```

#### How It Works:
```
1. Player clicks item
   ItemPickupManager.Update() detects click
   Checks layer: "PickupItem"
   TryPickup(itemObject)

2. Item attaches to hand
   Determines free hand slot (left/right)
   Scales item up 1.5x
   Rotates item ±10°
   Updates carried items list

3. Player clicks checkout area
   ItemPickupManager detects layer: "CheckoutZone"
   DropItemAtCheckout()
   Item positions in slot
   Updates total price UI

4. Player clicks "Make Sale" response
   Response has: isMakeSaleResponse = true
   DialogueManager calls:
        SaleManager.AttemptSale()

5. SaleManager validates sale
   Gets requestedItems from NPCRequest
   Gets present items from checkout zone
   CheckRequestedItemsMatch()

6. If match: Success!
   Plays success sound
   ItemPickupManager.SellItems()
   Destroys sold items
   Adds money to totalFunds
   Dialogue continues

7. If mismatch: Rejection
   Plays rejection sound
   Items stay in checkout
   Dialogue shows rejection path
```

#### Key Communication:
- **ItemPickupManager → SoundManager**: "Play pickup sound"
- **SaleManager → ItemPickupManager**: "Items match, sell them"
- **SaleManager → NPCRequest**: "What does customer want?"
- **DialogueManager → SaleManager**: "Try to make sale"

---

### SYSTEM 7: INBOX/EMAIL SYSTEM
**Purpose:** Show player feedback about corruption via in-game emails

#### Scripts Involved:
```
InboxUIManager.cs        ← MANAGER (inbox control)
    ↑ triggers
ScoreManager.cs          ← TRIGGER (threshold events)
    ↓ creates
MessageItemUI.cs         ← UI (individual message display)
```

#### How It Works:
```
1. Player crosses threshold (30 points)
   ScoreManager.CheckThresholds()
   Fires event:
        OnInboxThresholdReached(threshold)

2. InboxUIManager receives event
   OnInboxThresholdReached(threshold)
   Extracts message data:
        sender: "NEU Compliance Office"
        subject: "Kiosk Flagged for Review"
        content: "Your kiosk..."
        type: Warning

3. InboxUIManager creates message
   AddMessage(sender, subject, content, type)
   Adds to messages list
   Increments unreadCount
   Updates notification badge (e.g., "2")

4. Player clicks mail icon
   OpenInbox()
   Shows inbox panel
   Calls MarkAllMessagesAsRead()
   Clears notification badge

5. Messages display
   RefreshMessageList()
   Instantiates MessageItemUI prefab for each message
   MessageItemUI.Setup(messageData)
   Shows messages in reverse order (newest first)
```

#### Key Communication:
- **ScoreManager → InboxUIManager**: "Add message!" (event)
- **InboxUIManager → MessageItemUI**: "Display this message"
- **MessageItemUI → (self)**: Handles own visual display

---

### SYSTEM 8: SOUND SYSTEM
**Purpose:** Play audio feedback for player actions

#### Scripts Involved:
```
SoundManager.cs          ← SINGLETON (audio controller)
    ↑ requests from
DialogueManager.cs       ← "Play continue button sound"
ItemPickupManager.cs     ← "Play pickup sound"
SaleManager.cs           ← "Play sale success sound"
IDScanner.cs             ← "Play ID scan sound"
InboxUIManager.cs        ← "Play notification sound"
```

#### How It Works:
```
1. Script needs sound effect
   Calls singleton:
        SoundManager.Instance.PlaySFX(clipName)

2. SoundManager plays audio
   Loads AudioClip from inspector references
   Plays on appropriate AudioSource:
        sfxSource → general effects
        bgMusicSource → looping music
        buttonClickSource → instant UI feedback

3. Multiple sounds supported
   Each AudioSource independent
   Sounds can overlap
   Button clicks highest priority
```

#### Key Communication:
- **Any Script → SoundManager**: "Play this sound" (direct call)
- **SoundManager → Unity Audio**: Plays audio clips

---

## SCRIPT-BY-SCRIPT REFERENCE

### MANAGER SCRIPTS (The Bosses)

---

#### **DialogueManager.cs** (1246 lines) - CORE
**Location:** `Garon Scripts/Gamescripts/`
**Purpose:** Controls all customer conversations and game flow
**Type:** Manager (main game controller)

**What It Does:**
- Loads JSON scenario files from Resources
- Displays dialogue lines with typewriter effect
- Creates response button choices dynamically
- Handles ID scanning requests
- Processes item sales
- Manages corruption scoring
- Shows score screens between customers
- Advances to next scenario when complete

**Dependencies:**
- ScenarioManager (gets next scenario filename)
- ScoreManager (adds corruption points)
- ItemPickupManager (adds shady money)
- SoundManager (plays audio feedback)
- TypeWriter (animates text)
- IDScanner (receives ID scan events)
- AuthorizationUIManager (authorization animations)
- SaleManager (validates item sales)

**Communication Style:** Direct references, Unity events
**Independent:** No - central hub, talks to everything

**Key Methods:**
```csharp
StartDialogue()                  // Begin conversation with customer
ShowNextLine()                   // Display next dialogue line (core engine)
OnIDScanned(CustomerID id)       // Called when ID scanned
OnScoreContinue()               // Called when score screen Continue clicked
LoadCustomerFromJSON(filename)   // Load scenario from Resources
```

---

#### **ScenarioManager.cs** (220 lines) - CORE
**Location:** `SaabScripts/`
**Purpose:** Manages which scenarios play and in what order
**Type:** Manager (queue controller)

**What It Does:**
- Loads ScenarioConfig.json at game start
- Builds queue of scenarios to play
- Shuffles scenarios if enabled
- Provides next scenario filename to DialogueManager
- Dynamically injects police scenarios when corruption high
- Tracks current scenario

**Dependencies:**
- ScoreManager (listens to police threshold events)
- DialogueManager (provides scenario filenames)

**Communication Style:** Events (listening), direct calls (providing data)
**Independent:** Mostly - waits for requests and events

**Key Methods:**
```csharp
LoadConfig()                     // Read ScenarioConfig.json
InitializeScenarioQueue()        // Build scenario queue
GetNextScenarioFilename()        // Return next scenario to play
OnPoliceThresholdReached(event)  // Inject police scenario (event listener)
HasMoreScenarios()               // Check if scenarios remain
```

---

#### **ScoreManager.cs** (367 lines) - CORE
**Location:** `SaabScripts/`
**Purpose:** Tracks player corruption and triggers consequences
**Type:** Singleton Manager (persists across scenes)

**What It Does:**
- Stores current corruption score
- Defines inbox message thresholds (15, 30, 45 points)
- Defines police scenario thresholds (50, 70 points)
- Fires events when thresholds crossed
- Updates score UI
- Provides test mode (keyboard shortcuts 1-5)

**Dependencies:**
- None (other scripts depend on it)

**Communication Style:** Events (broadcasting), singleton access
**Independent:** Yes - fully standalone, others call it

**Key Events:**
```csharp
OnScoreChanged(int newScore)                    // Fires when score updates
OnInboxThresholdReached(InboxMessageThreshold)  // Fires when email threshold hit
OnPoliceThresholdReached(PoliceScenarioThreshold) // Fires when police threshold hit
```

**Key Methods:**
```csharp
AddScore(int points)             // Increase corruption score
CheckThresholds(oldScore, newScore) // Check if thresholds crossed
ResetScore()                     // Reset to 0
SetScore(int score)              // Debug: manually set score
```

---

#### **InboxUIManager.cs** (378 lines)
**Location:** `SaabScripts/`
**Purpose:** Manages email inbox overlay and notifications
**Type:** Singleton Manager (scene-local)

**What It Does:**
- Shows/hides inbox panel
- Creates email messages when score thresholds crossed
- Displays unread count badge
- Lists messages in scrollable UI
- Marks messages as read when opened
- Plays notification sounds

**Dependencies:**
- ScoreManager (listens to inbox threshold events)
- MessageItemUI (displays individual messages)
- SoundManager (plays notification sounds)

**Communication Style:** Events (listening), direct instantiation
**Independent:** Mostly - reacts to ScoreManager events

**Key Methods:**
```csharp
OnInboxThresholdReached(threshold) // Event listener from ScoreManager
AddMessage(sender, subject, content, type) // Create new message
OpenInbox()                        // Show inbox panel
CloseInbox()                       // Hide inbox panel
MarkAllMessagesAsRead()            // Clear unread count
RefreshMessageList()               // Update UI with all messages
```

---

#### **SoundManager.cs** (96 lines)
**Location:** `SamyScripts/`
**Purpose:** Centralized audio playback controller
**Type:** Singleton Manager (persists across scenes)

**What It Does:**
- Manages 3 AudioSource components (SFX, Music, Button Clicks)
- Plays sound effects on request
- Plays background music (looping)
- Handles button click sounds instantly

**Dependencies:**
- None (other scripts call it for audio)

**Communication Style:** Singleton direct calls
**Independent:** Yes - pure utility, no game logic

**Key Methods:**
```csharp
PlaySFX(AudioClip clip)          // Play sound effect
PlayMusic(AudioClip clip)        // Play looping background music
PlayButtonClick()                // Play instant button sound
StopMusic()                      // Stop background music
```

---

#### **AuthorizationUIManager.cs** (234 lines)
**Location:** `SaabScripts/`
**Purpose:** Shows loading bar animation for authorization IDs
**Type:** Singleton Manager (scene-local)

**What It Does:**
- Displays loading bar overlay
- Animates progress bar (0% to 100% over 5 seconds)
- Shows checkmark when complete
- Notifies when animation finishes
- Hides UI when scenario ends

**Dependencies:**
- AuthorizationID (called by authorization IDs)
- UnityEngine.UI (Image fillAmount animation)

**Communication Style:** Callbacks, direct calls
**Independent:** Yes - reusable animation controller

**Key Methods:**
```csharp
StartAuthorization(onComplete)   // Begin loading animation
AuthorizationSequence(onComplete) // Coroutine: animate progress bar
HideAll()                        // Hide loading UI
```

---

### HANDLER SCRIPTS (The Workers)

---

#### **ItemPickupManager.cs** (247 lines)
**Location:** `SamyScripts/`
**Purpose:** Handles item pickup, checkout, and money tracking
**Type:** Handler (game object manager)

**What It Does:**
- Detects mouse clicks on items
- Picks up items (attach to hand slots)
- Drops items in checkout area
- Manages slot indexing in checkout
- Tracks total funds (legitimate + shady money)
- Updates checkout total price UI
- Sells items when validation passes

**Dependencies:**
- SoundManager (pickup/place/fail sounds)
- SaleManager (validation results)
- ItemSlotData (item data on objects)

**Communication Style:** Direct calls, layer collision detection
**Independent:** No - coordinates with SaleManager

**Key Methods:**
```csharp
TryPickup(GameObject item)       // Attempt to pick up item
DropItemAtCheckout()             // Place item in checkout zone
SellItems()                      // Called by SaleManager on success
AddShadyFunds(float amount)      // Add bribe/illegal money
UpdateCheckoutTotal()            // Recalculate total price
```

---

#### **SaleManager.cs** (81 lines)
**Location:** `SamyScripts/`
**Purpose:** Validates item sales against customer requests
**Type:** Handler (validation controller)

**What It Does:**
- Gets requested items from NPCRequest component
- Gets present items from checkout zone
- Compares lists to validate sale
- Calls ItemPickupManager.SellItems() on success
- Plays success/rejection sounds

**Dependencies:**
- ItemPickupManager (sell items, get checkout data)
- NPCRequest (customer's requested items)
- SoundManager (audio feedback)

**Communication Style:** Direct calls, GetComponent
**Independent:** No - bridges dialogue and item systems

**Key Methods:**
```csharp
AttemptSale()                    // Validate and process sale
CheckRequestedItemsMatch(req, present) // Compare item lists
GetItemCategoriesInCheckout()    // Get items from checkout zone
```

---

#### **IDScanner.cs** (30 lines)
**Location:** `Garon Scripts/Gamescripts/`
**Purpose:** Detects when ID card dragged to scanner zone
**Type:** Handler (collision detector)

**What It Does:**
- Has BoxCollider2D trigger zone
- Detects CustomerID objects entering zone
- Notifies DialogueManager when ID scanned
- Plays scan sound

**Dependencies:**
- DialogueManager (reports scanned IDs)
- SoundManager (scan sound)
- CustomerID (component on ID cards)

**Communication Style:** Direct reference (to DialogueManager)
**Independent:** No - reports to DialogueManager

**Key Methods:**
```csharp
OnTriggerEnter2D(Collider2D other) // Detect ID card collision
```

---

#### **CustomerUIHandler.cs** (42 lines)
**Location:** `Garon Scripts/Gamescripts/`
**Purpose:** Detects player clicks on customer NPC
**Type:** Handler (click detector)

**What It Does:**
- Attached to each customer NPC prefab
- Fades NPC in when spawned (2 second fade)
- Detects pointer clicks (implements IPointerClickHandler)
- Calls DialogueManager.StartDialogue() when clicked

**Dependencies:**
- DialogueManager (starts dialogue on click)

**Communication Style:** Direct reference, Unity pointer events
**Independent:** No - reports clicks to DialogueManager

**Key Methods:**
```csharp
OnPointerClick(PointerEventData) // Called when player clicks NPC
FadeIn()                         // Coroutine: fade in animation
```

---

#### **NPCRequest.cs** (10 lines)
**Location:** `SamyScripts/`
**Purpose:** Stores what items customer wants to buy
**Type:** Data holder (attached to NPC)

**What It Does:**
- Attached to customer NPC GameObject
- Stores list of requested ItemCategory enums
- Set by DialogueManager when scenario loads

**Dependencies:**
- None (pure data storage)

**Communication Style:** Direct access (public list)
**Independent:** Yes - passive data storage

**Key Fields:**
```csharp
List<ItemCategory> requestedItems // What customer wants
SetRequest(List<ItemCategory>)    // Update request list
```

---

### UI SCRIPTS (The Display)

---

#### **IDInfoDisplay.cs**
**Location:** `Garon Scripts/Gamescripts/`
**Purpose:** Displays ID information panel
**Type:** UI Controller

**What It Does:**
- Shows ID card data fields (name, DOB, address, etc.)
- Displays "[Access Denied]" for blocked fields
- Shows authorization status for special IDs
- Updates when access permissions change

**Dependencies:**
- CustomerData (reads ID data from)
- DialogueManager (receives update calls)

**Communication Style:** Direct calls from DialogueManager
**Independent:** Yes - pure display logic

**Key Methods:**
```csharp
UpdateDisplay(CustomerData data) // Refresh panel with ID data
ShowAccessDenied(fieldName)      // Display blocked field
ShowAuthorized()                 // Display authorization status
```

---

#### **MessageItemUI.cs** (145 lines)
**Location:** `SaabScripts/`
**Purpose:** Displays individual email message in inbox
**Type:** UI Controller (list item)

**What It Does:**
- Shows sender, subject, timestamp
- Color-codes by message type (Info/Warning/Alert/Critical)
- Shows icon based on severity (i, !, !!, !!!)
- Shows/hides unread indicator
- Adjusts background opacity for read messages

**Dependencies:**
- None (receives data, displays it)

**Communication Style:** Direct method calls
**Independent:** Yes - reusable UI component

**Key Methods:**
```csharp
Setup(MessageData data)          // Initialize message display
MarkAsRead()                     // Update visual to read state
GetColorForMessageType(type)     // Return color for message type
GetIconForMessageType(type)      // Return icon symbol
```

---

#### **TypeWriter.cs** (76 lines)
**Location:** `Garon Scripts/`
**Purpose:** Animates text with typewriter effect
**Type:** Helper (animation utility)

**What It Does:**
- Types text character-by-character
- Configurable speed (default 0.05s per character)
- Allows skipping (mouse click or Space key)
- Can type multiple text fields in sequence

**Dependencies:**
- TMPro (TextMeshPro text components)

**Communication Style:** Coroutine calls
**Independent:** Yes - reusable utility

**Key Methods:**
```csharp
TypeTextFromTMP(textComponent, text) // Animate text typing
TypeTwoTextsFromTMP(top, bottom)     // Type two texts in sequence
IsTyping()                           // Check if currently animating
CompleteTyping()                     // Skip to end instantly
```

---

#### **FadeController.cs**
**Location:** `Garon Scripts/`
**Purpose:** Screen fade in/out transitions
**Type:** Helper (visual effect)

**What It Does:**
- Fades screen to black
- Fades screen from black
- Used for scene transitions
- Configurable fade duration

**Dependencies:**
- UnityEngine.UI (Image with CanvasGroup)

**Communication Style:** Direct method calls
**Independent:** Yes - reusable utility

**Key Methods:**
```csharp
FadeOut(duration)                // Fade to black
FadeIn(duration)                 // Fade from black
```

---

#### **UIManager.cs**
**Location:** `Garon Scripts/`
**Purpose:** Main menu and story intro controller
**Type:** Scene Manager (StartScreen scene)

**What It Does:**
- Shows story introduction dialogue (3 screens)
- Handles Start Game button
- Fades to TutorialScene
- Handles Exit Game button

**Dependencies:**
- FadeController (screen transitions)
- TypeWriter (story text animation)
- SceneManager (scene loading)

**Communication Style:** Unity button events
**Independent:** Yes - self-contained for start screen

**Key Methods:**
```csharp
StartGame()                      // Begin game (load tutorial)
ExitGame()                       // Quit application
ShowStoryDialogue()              // Display intro screens
```

---

### DATA CLASSES (The Storage)

---

#### **CustomerScenarioDialogue.cs** (92 lines)
**Location:** `SaabScripts/`
**Purpose:** Data structure for JSON scenario files
**Type:** Serializable data classes

**Classes Defined:**
```csharp
CustomerScenarioData             // Top-level scenario data
DialogueLineData                 // Individual dialogue line
DialogueResponseData             // Player choice response
```

**What It Does:**
- Defines structure for JSON deserialization
- Stores customer info (name, prefabs, ID data)
- Stores dialogue lines array
- Stores response choices
- Includes all feature flags (ID access, business cards, etc.)

**Dependencies:**
- ItemCategory enum (from ItemScript.cs)

**Communication Style:** Data only (no methods)
**Independent:** Yes - pure data structure

---

#### **CustomerData.cs** (42 lines)
**Location:** `Garon Scripts/Gamescripts/`
**Purpose:** Runtime storage for customer ID information
**Type:** Serializable data class

**What It Does:**
- Stores ID card data (name, DOB, address, issuer, picture)
- Stores access permission flags (which fields visible)
- Stores authorization status for special IDs
- Created from CustomerID component when scanned

**Dependencies:**
- CustomerID (copied from)

**Communication Style:** Data only
**Independent:** Yes - pure data structure

**Key Fields:**
```csharp
string Name, DOB, Address, Issuer
Sprite ProfileImage
bool allowNameAccess, allowDOBAccess, etc.
bool isAuthorizationID
string authorizationStatus
```

---

#### **CustomerID.cs** (26 lines)
**Location:** `Garon Scripts/Gamescripts/`
**Purpose:** ID card component attached to ID prefabs
**Type:** MonoBehaviour data holder

**What It Does:**
- Attached to ID card prefabs
- Stores ID information (configured in Inspector)
- Used to create CustomerData when scanned

**Dependencies:**
- None (passive data)

**Communication Style:** Read by IDScanner
**Independent:** Yes - data storage only

---

#### **ItemSlotData.cs** (67 lines) - Also named ItemScript.cs
**Location:** `SamyScripts/`
**Purpose:** Item component with category and price
**Type:** MonoBehaviour data holder

**What It Does:**
- Defines ItemCategory enum (all 17 items)
- Attached to item prefabs
- Stores item category and slot index
- Calculates item price based on category
- Stores original scale/rotation/position

**Dependencies:**
- None (standalone)

**Communication Style:** Read by ItemPickupManager/SaleManager
**Independent:** Yes - data storage + enum definition

**Key Fields:**
```csharp
ItemCategory category            // What type of item
int slotIndex                    // Checkout zone slot number
float Value                      // Price (calculated from category)
Vector3 originalShelfPosition    // For returning to shelf
```

---

### HELPER SCRIPTS (The Utilities)

---

#### **AuthorizationID.cs** (147 lines)
**Location:** `SaabScripts/`
**Purpose:** Special ID component requiring authorization
**Type:** MonoBehaviour (attached to special IDs)

**What It Does:**
- Attached to authorization-required ID prefabs (e.g., Owner ID)
- Inherits from CustomerID
- Triggers authorization UI when scanned
- Fires event when authorization complete
- Updates CustomerID status field

**Dependencies:**
- CustomerID (base class)
- AuthorizationUIManager (triggers animation)

**Communication Style:** Events, direct calls
**Independent:** No - coordinates with AuthorizationUIManager

**Key Events:**
```csharp
OnAuthorizationStarted           // Fires when authorization begins
OnAuthorizationCompleted         // Fires when loading bar finishes
```

**Key Methods:**
```csharp
StartAuthorization()             // Begin authorization process
CompleteAuthorization()          // Mark as authorized
```

---

#### **DraggableItem.cs**
**Location:** `Garon Scripts/Gamescripts/`
**Purpose:** Makes items draggable with mouse
**Type:** MonoBehaviour (attached to items)

**What It Does:**
- Implements drag-and-drop for items
- Follows mouse position while dragging
- Used with ID cards and items

**Dependencies:**
- UnityEngine input system

**Communication Style:** Self-contained
**Independent:** Yes - reusable component

---

#### **CameraFade.cs**
**Location:** `SamyScripts/`
**Purpose:** Camera fade effect alternative
**Type:** Helper (visual effect)

**What It Does:**
- Similar to FadeController
- Camera-based fade (not UI-based)
- Used for specific scene transitions

**Dependencies:**
- Camera component

**Communication Style:** Direct calls
**Independent:** Yes - standalone utility

---

#### **ItemHover.cs** / **ItemHoverManager.cs**
**Location:** `SamyScripts/`
**Purpose:** Item hover effects (visual feedback)
**Type:** Helper (visual effect)

**What It Does:**
- Highlights items on mouse hover
- Shows item name/price tooltip
- Provides visual feedback for interaction

**Dependencies:**
- ItemSlotData (reads item info)

**Communication Style:** Mouse events
**Independent:** Yes - visual feedback only

---

#### **ReturnItemConfirmationUI.cs**
**Location:** `SamyScripts/`
**Purpose:** Confirmation dialog for returning items to shelf
**Type:** UI Controller

**What It Does:**
- Shows Yes/No dialog when player clicks item in checkout
- Confirms item return to shelf
- Returns item to original position on Yes

**Dependencies:**
- ItemPickupManager (coordinates item return)

**Communication Style:** Unity button events
**Independent:** Mostly - simple UI dialog

---

#### **GameRestartHotkey.cs** (17 lines) - RESTART.cs
**Location:** `SamyScripts/`
**Purpose:** Debug shortcuts for restarting game
**Type:** Utility (debug tool)

**What It Does:**
- Ctrl+R: Restart entire game (scene 0)
- Ctrl+T: Restart gameplay (scene 1)
- Active during testing

**Dependencies:**
- SceneManager (Unity scene loading)

**Communication Style:** Keyboard input
**Independent:** Yes - debug utility

---

## DATA FLOW EXAMPLES

### Example 1: Player Makes Corrupt Choice
```
┌─────────────────────────────────────────────────────────────┐
│ FLOW: Player Accepts Bribe                                  │
└─────────────────────────────────────────────────────────────┘

1. Player clicks response: "Accept bribe (€50)"
   ↓
2. DialogueManager.OnResponseClicked()
   Response has:
      scoreValue = 30
      shadyMoneyReward = 50.0
   ↓
3. DialogueManager calls:
   ScoreManager.Instance.AddScore(30)
   ItemPickupManager.Instance.AddShadyFunds(50.0)
   ↓
4. ScoreManager.AddScore()
   currentScore += 30 → now 45 points
   CheckThresholds(15, 45)
   ↓
5. ScoreManager detects threshold crossed (30 points)
   Fires event:
      OnInboxThresholdReached(warningThreshold)
   ↓
6. InboxUIManager receives event
   OnInboxThresholdReached(threshold)
   AddMessage("NEU Compliance", "Kiosk Flagged", ...)
   ↓
7. InboxUIManager updates UI
   unreadCount = 2
   Notification badge shows "2"
   RefreshMessageList()
   ↓
8. ItemPickupManager updates funds
   totalFunds += 50.0
   Updates funds UI: "€150.00"
   ↓
9. DialogueManager continues
   Advances to nextLineIndex
   ShowNextLine()
```

### Example 2: Player Scans ID
```
┌─────────────────────────────────────────────────────────────┐
│ FLOW: ID Verification                                        │
└─────────────────────────────────────────────────────────────┘

1. Dialogue line triggers ID request
   DialogueLineData:
      askForID = true
      grantNameAccess = true
      grantDOBAccess = true
   ↓
2. DialogueManager.ShowNextLine()
   Detects askForID = true
   Spawns ID prefab from Resources
   GameObject id = Instantiate(idPrefab)
   ↓
3. Player drags ID to scanner zone
   ↓
4. IDScanner.OnTriggerEnter2D()
   Detects collision with CustomerID component
   Plays scan sound
   ↓
5. IDScanner calls:
   dialogueManager.OnIDScanned(customerID)
   ↓
6. DialogueManager.OnIDScanned()
   Creates CustomerData from CustomerID
   Stores in currentScannedIDData
   ↓
7. DialogueManager applies access grants
   if (grantNameAccess)
      currentScannedIDData.allowNameAccess = true
   if (grantDOBAccess)
      currentScannedIDData.allowDOBAccess = true
   ↓
8. DialogueManager updates UI
   UpdateIDInfoPanel()
   IDInfoDisplay reads CustomerData
   Shows: Name = "Robin Banks", DOB = "Over 18"
   Shows: Address = "[Access Denied]"
   ↓
9. DialogueManager destroys ID
   Destroy(idObject)
   Shows Continue button
```

### Example 3: Player Completes Sale
```
┌─────────────────────────────────────────────────────────────┐
│ FLOW: Item Purchase                                          │
└─────────────────────────────────────────────────────────────┘

1. Customer requests: HotShot Cigarettes (ID 11)
   DialogueManager loads scenario
   Sets NPCRequest.requestedItems = [11]
   ↓
2. Player picks up HotShot Cigarettes
   ItemPickupManager.TryPickup(itemObject)
   Item attaches to hand slot
   ↓
3. Player clicks checkout
   ItemPickupManager.DropItemAtCheckout()
   Item positions in checkout zone
   Total updates: "€15.00"
   ↓
4. Player clicks "Make Sale" response
   Response has: isMakeSaleResponse = true
   ↓
5. DialogueManager calls:
   SaleManager.AttemptSale()
   ↓
6. SaleManager validates
   requestedItems = [11] (from NPCRequest)
   presentItems = [11] (from checkout zone)
   CheckRequestedItemsMatch() → TRUE
   ↓
7. SaleManager calls:
   ItemPickupManager.SellItems()
   Plays success sound
   ↓
8. ItemPickupManager processes sale
   totalFunds += 15.0
   Destroys sold items
   Updates UI
   ↓
9. SaleManager calls:
   DialogueManager.OnSaleSuccess()
   ↓
10. DialogueManager continues
    Advances to success dialogue path
    ShowNextLine()
```

### Example 4: Police Scenario Injection
```
┌─────────────────────────────────────────────────────────────┐
│ FLOW: Dynamic Police Scenario                                │
└─────────────────────────────────────────────────────────────┘

1. Player corruption reaches 50 points
   DialogueManager adds corruption
   ScoreManager.AddScore(20)
   currentScore = 50
   ↓
2. ScoreManager checks thresholds
   CheckThresholds(30, 50)
   Detects police threshold crossed
   ↓
3. ScoreManager fires event:
   OnPoliceThresholdReached(policeThreshold)
   policeThreshold.jsonFileName = "PoliceGuy_Warning_Scenario"
   ↓
4. ScenarioManager receives event
   OnPoliceThresholdReached(policeScenario)
   ↓
5. ScenarioManager injects scenario
   Creates new queue:
      tempQueue.Enqueue("PoliceGuy_Warning_Scenario")
      tempQueue.Enqueue(remaining scenarios...)
   scenarioQueue = tempQueue
   ↓
6. Current scenario completes
   DialogueManager.OnScoreContinue()
   Asks: "Any more scenarios?"
   ↓
7. ScenarioManager responds
   GetNextScenarioFilename()
   Returns: "PoliceGuy_Warning_Scenario"
   ↓
8. DialogueManager loads police scenario
   LoadCustomerFromJSON("PoliceGuy_Warning_Scenario")
   Police officer appears
   Warning conversation starts
```

---

## QUICK REFERENCE TABLES

### Table 1: Script Dependencies
| Script | Depends On | Depended On By |
|--------|-----------|----------------|
| DialogueManager | ScenarioManager, ScoreManager, ItemPickupManager, SoundManager, IDScanner, SaleManager | CustomerUIHandler, SaleManager |
| ScenarioManager | ScoreManager (events) | DialogueManager |
| ScoreManager | None | InboxUIManager, ScenarioManager, DialogueManager |
| InboxUIManager | ScoreManager (events), MessageItemUI | None |
| ItemPickupManager | SoundManager, ItemSlotData | SaleManager, DialogueManager |
| SaleManager | ItemPickupManager, NPCRequest, SoundManager | DialogueManager |
| SoundManager | None | Everyone (audio) |
| AuthorizationUIManager | None | AuthorizationID |

### Table 2: Communication Patterns
| Pattern | Scripts Using It | Example |
|---------|------------------|---------|
| Singleton | ScoreManager, SoundManager, InboxUIManager | `ScoreManager.Instance.AddScore(10)` |
| Events | ScoreManager → Listeners | `OnInboxThresholdReached?.Invoke()` |
| Direct Reference | CustomerUIHandler → DialogueManager | `dialogueManager.StartDialogue()` |
| GetComponent | SaleManager → NPCRequest | `npc.GetComponent<NPCRequest>()` |
| Unity Events | UI Buttons → Managers | Inspector: Button.onClick → DialogueManager.OnContinue |

### Table 3: Script Locations
| System | Scripts | Folder |
|--------|---------|--------|
| Dialogue | DialogueManager, CustomerUIHandler, TypeWriter, CustomerData, IDInfoDisplay | Garon Scripts/ |
| Scenarios | ScenarioManager, CustomerScenarioDialogue | SaabScripts/ |
| Scoring | ScoreManager, InboxUIManager, MessageItemUI | SaabScripts/ |
| Authorization | AuthorizationID, AuthorizationUIManager | SaabScripts/ |
| Items | ItemPickupManager, SaleManager, ItemSlotData, NPCRequest | SamyScripts/ |
| Sound | SoundManager | SamyScripts/ |
| UI | UIManager, FadeController | Garon Scripts/ |

### Table 4: Event Subscriptions
| Event Name | Fired By | Subscribed By | When |
|------------|----------|---------------|------|
| OnScoreChanged | ScoreManager | (UI elements) | Score updates |
| OnInboxThresholdReached | ScoreManager | InboxUIManager | Score crosses 15/30/45 |
| OnPoliceThresholdReached | ScoreManager | ScenarioManager | Score crosses 50/70 |
| OnAuthorizationStarted | AuthorizationID | DialogueManager | Special ID scanned |
| OnAuthorizationCompleted | AuthorizationID | DialogueManager | Loading bar finishes |

### Table 5: Manager Singleton Access
| Manager | Access Pattern | Scope |
|---------|---------------|-------|
| ScoreManager | `ScoreManager.Instance` | Global (DontDestroyOnLoad) |
| SoundManager | `SoundManager.Instance` | Global (DontDestroyOnLoad) |
| InboxUIManager | `InboxUIManager.Instance` | Scene-local |
| AuthorizationUIManager | `AuthorizationUIManager.Instance` | Scene-local |
| ItemPickupManager | `ItemPickupManager.Instance` | Scene-local |

### Table 6: Data Flow Summary
| Action | Path | Result |
|--------|------|--------|
| Player clicks NPC | CustomerUIHandler → DialogueManager | Conversation starts |
| Player makes choice | DialogueManager → ScoreManager | Corruption added |
| Corruption threshold | ScoreManager → InboxUIManager (event) | Email appears |
| High corruption | ScoreManager → ScenarioManager (event) | Police scenario injected |
| ID scan | IDScanner → DialogueManager | ID data stored |
| Item sale | DialogueManager → SaleManager → ItemPickupManager | Items sold, money added |
| Sound needed | Any Script → SoundManager (singleton) | Audio plays |

---

## TIPS FOR NEW DEVELOPERS

### Understanding Script Relationships
1. **Start with Managers** - They control major systems
2. **Follow the events** - Many systems communicate via events
3. **Check singleton access** - Global managers use `Instance` pattern
4. **Read data classes first** - Understanding data structures helps everything

### Debugging Communication
```csharp
// Add debug logs to trace communication:

// In DialogueManager (sender):
Debug.Log("[DialogueManager] Calling ScoreManager.AddScore(10)");
ScoreManager.Instance.AddScore(10);

// In ScoreManager (receiver):
public void AddScore(int points) {
    Debug.Log($"[ScoreManager] Received AddScore call: {points} points");
    // ... rest of method
}
```

### Common Patterns to Recognize
```csharp
// 1. SINGLETON ACCESS
ScoreManager.Instance.AddScore(10);

// 2. EVENT SUBSCRIPTION
void OnEnable() {
    ScoreManager.Instance.OnInboxThresholdReached += OnInboxThreshold;
}

// 3. DIRECT REFERENCE
public DialogueManager dialogueManager; // Set in Inspector
dialogueManager.StartDialogue();

// 4. GETCOMPONENT
NPCRequest request = npcObject.GetComponent<NPCRequest>();

// 5. UNITY EVENT
// Set in Inspector: Button.onClick → Method
public void OnButtonClick() { ... }
```

### When to Use Each Communication Method
- **Singleton**: Manager needs global access (ScoreManager, SoundManager)
- **Events**: One-to-many broadcasting (ScoreManager notifying multiple systems)
- **Direct Reference**: Parent-child relationships (CustomerUIHandler → DialogueManager)
- **GetComponent**: Finding scripts on same/related objects (SaleManager → NPCRequest)
- **Unity Events**: UI button clicks, trigger events

### Adding New Features
**Want to add a new scenario?**
- Modify: None! Just create JSON file
- Add to: ScenarioConfig.json

**Want to add a new item?**
- Modify: ItemSlotData.cs (add to enum + price)
- Create: Item prefab with ItemSlotData component

**Want to add a new score threshold?**
- Modify: ScoreManager.cs Inspector (add threshold)
- System automatically handles it

**Want to add a new sound effect?**
- Add: AudioClip to SoundManager Inspector
- Call: `SoundManager.Instance.PlaySFX(newClip)`

---

## FINAL NOTES

### Script Independence Level
**Fully Independent (reusable anywhere):**
- TypeWriter, FadeController, DraggableItem
- SoundManager, MessageItemUI
- CustomerData, ItemSlotData

**Partially Independent (needs specific data):**
- IDScanner, IDInfoDisplay
- ItemPickupManager, SaleManager
- InboxUIManager

**Highly Connected (core systems):**
- DialogueManager (talks to everything)
- ScenarioManager (coordinates scenarios)
- ScoreManager (central corruption tracker)

### Most Important Scripts to Understand
1. **DialogueManager** - Controls game flow
2. **ScoreManager** - Tracks corruption
3. **ScenarioManager** - Controls scenario order
4. **ItemPickupManager** - Handles item interactions
5. **SaleManager** - Validates purchases

### System Health
- ✅ Well-organized: Scripts grouped by developer/system
- ✅ Clear responsibilities: Each script has specific purpose
- ✅ Event-driven: Loose coupling via events
- ✅ Singleton managers: Easy global access
- ✅ Data-driven: JSON scenarios, easy content creation

---

**Last Updated:** [Current Date]
**Game Version:** 1.0
**Total Scripts:** 27
**For Questions:** Contact development team

**Remember:** When in doubt, add Debug.Log() statements to trace communication flow!