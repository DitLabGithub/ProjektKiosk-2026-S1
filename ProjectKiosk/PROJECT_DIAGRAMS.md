# Project Kiosk - Visual Diagrams

This document contains visual diagrams for the Project Kiosk game architecture, flows, and systems.

---

## 1. System Architecture Overview

```mermaid
graph TB
    subgraph "Core Systems"
        DM[DialogueManager]
        SM[ScenarioManager]
        IPM[ItemPickupManager]
        SndM[SoundManager]
        UM[UIManager]
    end

    subgraph "ID System"
        IDS[IDScanner]
        CID[CustomerID]
        AID[AuthorizationID]
        IDINFO[IDInfoDisplay]
    end

    subgraph "Data Sources"
        JSON[JSON Scenarios]
        VO[Voice-Over Audio]
        NPCP[NPC Prefabs]
        IDP[ID Prefabs]
    end

    subgraph "UI Components"
        SCORE[Score Screens]
        CARD[Business Cards]
        CHECKOUT[Checkout Zone]
        DIALOGUE[Dialogue Panel]
    end

    SM -->|Load Next Scenario| DM
    JSON -->|Customer Data| SM
    DM -->|Spawn ID| IDS
    DM -->|Play Audio| VO
    DM -->|Show Score| SCORE
    DM -->|Display Card| CARD
    IDS -->|Scan| CID
    CID -->|Display Info| IDINFO
    AID -->|Authorize| IDINFO
    IPM -->|Validate Sale| DM
    CHECKOUT -->|Items| IPM
    DM -->|Update UI| DIALOGUE
    SndM -->|Audio Feedback| DM

    style DM fill:#4a90e2
    style SM fill:#4a90e2
    style JSON fill:#50c878
    style VO fill:#50c878
```

---

## 2. Game Flow - Scene Transitions

```mermaid
stateDiagram-v2
    [*] --> StartScreen
    StartScreen --> TutorialScene: Start Game

    TutorialScene --> Customer1: Load First Customer
    Customer1 --> Dialogue1: Start Dialogue
    Dialogue1 --> IDCheck1: Request ID
    IDCheck1 --> ItemSale1: ID Verified
    ItemSale1 --> ScoreScreen1: Transaction Complete
    ScoreScreen1 --> GameplayScene: Tutorial Complete

    GameplayScene --> Customer2: Load Next Customer
    Customer2 --> Dialogue2: Start Dialogue
    Dialogue2 --> IDCheck2: Request ID
    IDCheck2 --> ItemSale2: ID Verified
    ItemSale2 --> ScoreScreen2: Transaction Complete

    ScoreScreen2 --> Customer3: More Scenarios
    ScoreScreen2 --> EndCredits: All Customers Served

    Customer3 --> Dialogue3
    Dialogue3 --> ScoreScreen3
    ScoreScreen3 --> Customer2: Loop

    EndCredits --> [*]
```

---

## 3. Dialogue System Architecture

```mermaid
graph LR
    subgraph "DialogueManager Components"
        DL[Dialogue Lines]
        RESP[Response Choices]
        STACK[Return Stack]
        FLAGS[Access Flags]
    end

    subgraph "Line Features"
        TXT[Text Display]
        TW[Typewriter Effect]
        VO[Voice-Over]
        SPR[NPC Sprite Change]
    end

    subgraph "Player Actions"
        CONT[Continue Button]
        CHOICE[Choice Buttons]
        BACK[Go Back Button]
        SCAN[ID Scan Action]
    end

    subgraph "Flow Control"
        NEXT[Next Line Index]
        RET[Return After Response]
        END[End Conversation]
        AUTO[Auto Advance]
    end

    DL --> TXT
    TXT --> TW
    TXT --> VO
    TXT --> SPR

    DL --> RESP
    RESP --> CHOICE

    CONT --> NEXT
    CHOICE --> NEXT
    CHOICE --> RET
    RET --> STACK
    BACK --> STACK

    DL --> SCAN
    SCAN --> FLAGS
    FLAGS --> IDINFO[ID Info Display]

    DL --> END
    END --> SCORE[Score Screen]

    TW --> AUTO
    AUTO --> NEXT

    style DL fill:#ff6b6b
    style RESP fill:#ffd93d
    style SCAN fill:#6bcf7f
```

---

## 4. Shaun Bakers Scenario - Decision Tree

```mermaid
graph TD
    START[Clerk: Hello, how can I help?] --> L1[Shady: I offer information...]
    L1 --> L2[Clerk: What is that?]
    L2 --> L3[Shady: Information for business!]
    L3 --> L4[Clerk: Not interested in shady business]
    L4 --> L5[Shady: I lost my job after NEU crash]
    L5 --> L6[Shady: I can sell you parts too]

    L6 --> C1{Player Choice 1}
    C1 -->|No need to hear more| REFUSE1[Clerk: Goodbye Shady]
    C1 -->|Learn more| LEARN1[Clerk: You steal data...]

    REFUSE1 --> REFUSE2[Shady: You're losing valuable offer]
    REFUSE2 --> C2{Player Choice 2}
    C2 -->|Let him go away| THREAT[Shady: Here's my card... It doesn't end here]
    C2 -->|Learn more| LEARN1

    THREAT --> ANGRY[Clerk: Piss off... ANGRY]
    ANGRY --> FINAL1[Shady: It doesn't end here, cashier...]
    FINAL1 --> END1[Score Screen 6: Best Ending]

    LEARN1 --> LEARN2[Shady: Mind your tongue!]
    LEARN2 --> LEARN3[Clerk: You're not local]
    LEARN3 --> LEARN4[Shady: I work with Brootle]
    LEARN4 --> LEARN5[Clerk: Exploitation service?]
    LEARN5 --> LEARN6[Shady: My sources will take your data anyway]
    LEARN6 --> LEARN7[Clerk: Why don't you do it then?]
    LEARN7 --> OFFER[Shady: Easier if you give it now...]

    OFFER --> C3{Player Choice 3}
    C3 -->|Not interested| SMART[Clerk: Not falling for manipulation]
    C3 -->|Sell Data| SELL1[Clerk: How much money?]
    C3 -->|Buy Data| BUY1[Clerk: There are cameras everywhere]

    SMART --> CARD[Shady: You're smart. Here's my card]
    CARD --> END2[Score Screen 7: Good Ending]

    SELL1 --> SELL2[Shady: 1000 NEU initial payment]
    SELL2 --> SELL3[Clerk: That's a lot of money]
    SELL3 --> SELL4[Shady: Companies obsessed with data]
    SELL4 --> SELL5[Clerk: Morals man...]
    SELL5 --> SELL6[Shady: User never had full consent]
    SELL6 --> SELL7[Clerk: You know it's bad]
    SELL7 --> SELL8[Shady: Companies win anyway]
    SELL8 --> SELL9[Clerk: Bullshit... but ok, I fold]
    SELL9 --> SELL10[Shady: Here's the money]
    SELL10 --> SELL11[Clerk: Data transferred]
    SELL11 --> END3[Score Screen 8: Bad Ending - Sold Data]

    BUY1 --> BUY2[Shady: Remote places have no surveillance]
    BUY2 --> BUY3[Clerk: Unfortunately I agree]
    BUY3 --> BUY4[Shady: I can teach you]
    BUY4 --> BUY5[Clerk: It's still a problem but yes]
    BUY5 --> BUY6[Shady: Final answer?]
    BUY6 --> BUY7[Clerk: Yes, what are the prices?]
    BUY7 --> BUY8[Shady: 500 NEUR for first time]
    BUY8 --> BUY9[Clerk: That's a lot... for salary increase]
    BUY9 --> BUY10[Shady: To be honest means being hungry]
    BUY10 --> BUY11[Clerk: Can barely pay rent... here's payment]
    BUY11 --> BUY12[Shady: Check the data... We'll do more business]
    BUY12 --> END4[Score Screen 9: Bad Ending - Bought Data]

    style START fill:#4a90e2
    style END1 fill:#50c878
    style END2 fill:#ffd93d
    style END3 fill:#ff6b6b
    style END4 fill:#ff6b6b
    style C1 fill:#9b59b6
    style C2 fill:#9b59b6
    style C3 fill:#9b59b6
```

---

## 5. ID Scanning & Authorization Flow

```mermaid
sequenceDiagram
    participant Player
    participant DialogueManager
    participant IDScanner
    participant CustomerID
    participant AuthorizationID
    participant IDInfoDisplay
    participant AuthUIManager

    DialogueManager->>DialogueManager: Dialogue line with askForID=true
    DialogueManager->>IDScanner: SpawnID()
    IDScanner->>CustomerID: Instantiate ID Prefab
    CustomerID->>Player: ID appears on screen

    Player->>IDScanner: Drag ID to scanner
    IDScanner->>CustomerID: OnIDScanned()

    alt Regular ID
        CustomerID->>DialogueManager: OnIDScanned(CustomerID)
        DialogueManager->>IDInfoDisplay: UpdateIDInfoPanel()
        IDInfoDisplay->>Player: Show ID information
        DialogueManager->>Player: Show Continue button
    else Authorization ID
        CustomerID->>AuthorizationID: Detect AuthorizationID component
        AuthorizationID->>AuthUIManager: StartAuthorization()
        AuthUIManager->>Player: Show loading screen
        AuthUIManager->>Player: Show verification animation
        AuthorizationID->>DialogueManager: OnAuthorizationCompleted event
        DialogueManager->>IDInfoDisplay: UpdateIDInfoPanel() with auth status
        IDInfoDisplay->>Player: Show "Authorized" status
        DialogueManager->>Player: Show Continue button
    end

    Player->>DialogueManager: Click Continue
    DialogueManager->>DialogueManager: Grant progressive data access
    DialogueManager->>IDInfoDisplay: UpdateIDInfoPanel()
    IDInfoDisplay->>Player: Show additional ID fields
```

---

## 6. Scenario Loading System

```mermaid
graph TB
    START[Game Start] --> SM[ScenarioManager.Awake]
    SM --> LOAD[Load ScenarioConfig.json]

    LOAD --> CHECK{Shuffle Enabled?}
    CHECK -->|Yes| SHUFFLE[Fisher-Yates Shuffle]
    CHECK -->|No| QUEUE[Create Queue in Order]
    SHUFFLE --> QUEUE

    QUEUE --> READY[Scenarios Ready]

    READY --> DM[DialogueManager.OnScoreContinue]
    DM --> NEXT{Has More Scenarios?}

    NEXT -->|Yes| GET[GetNextScenarioFilename]
    NEXT -->|No| END[Show End Credits]

    GET --> LOADJSON[Load Customer JSON]
    LOADJSON --> SPAWNNPC[Instantiate NPC Prefab]
    SPAWNNPC --> SHOWDIALOGUE[Show First Dialogue Line]

    SHOWDIALOGUE --> COMPLETE[Scenario Complete]
    COMPLETE --> NOTIFY[Notify ScenarioManager]

    NOTIFY --> FOLLOWUP{Has Follow-Up?}
    FOLLOWUP -->|Yes| INSERT[Insert Follow-Up to Front of Queue]
    FOLLOWUP -->|No| CONTINUE[Continue to Next]

    INSERT --> DM
    CONTINUE --> DM

    style START fill:#4a90e2
    style LOADJSON fill:#50c878
    style SHOWDIALOGUE fill:#ffd93d
    style END fill:#ff6b6b
```

---

## 7. Item Sale Transaction Flow

```mermaid
stateDiagram-v2
    [*] --> CustomerRequests
    CustomerRequests: Customer Requests Items

    CustomerRequests --> PlayerPicksItems
    PlayerPicksItems: Player Picks Items from Shelf

    PlayerPicksItems --> ItemsInCheckout
    ItemsInCheckout: Items in Checkout Zone

    ItemsInCheckout --> PlayerClicksSale
    PlayerClicksSale: Player Clicks "Make Sale" Response

    PlayerClicksSale --> ValidateItems
    ValidateItems: Validate Items Match Request

    ValidateItems --> CorrectItems: Items Match
    ValidateItems --> WrongItems: Items Don't Match

    CorrectItems --> PlaySuccessSound
    PlaySuccessSound: Play Sale Success Sound

    PlaySuccessSound --> RemoveItems
    RemoveItems: Remove Items from Checkout

    RemoveItems --> ShowSuccess
    ShowSuccess: Show Success Message

    ShowSuccess --> ContinueDialogue

    WrongItems --> PlayRejectionSound
    PlayRejectionSound: Play Sale Rejected Sound

    PlayRejectionSound --> ShowRejection
    ShowRejection: "I asked for X, that's it"

    ShowRejection --> ItemsInCheckout

    ContinueDialogue --> [*]
```

---

## 8. Main C# Classes - Simplified Overview

```mermaid
classDiagram
    %% Core Game Management
    class DialogueManager {
        <<MonoBehaviour>>
        Manages dialogue flow and customer interactions
    }

    class ScenarioManager {
        <<MonoBehaviour>>
        Handles scenario queue and shuffling
    }

    class UIManager {
        <<MonoBehaviour>>
        Controls UI elements
    }

    class SoundManager {
        <<Singleton>>
        Global audio management
    }

    %% ID System
    class IDScanner {
        <<MonoBehaviour>>
        Handles ID drag-and-drop scanning
    }

    class CustomerID {
        <<MonoBehaviour>>
        Stores customer ID data
    }

    class AuthorizationID {
        <<MonoBehaviour>>
        Special ID with authorization check
    }

    class IDInfoDisplay {
        <<MonoBehaviour>>
        Displays ID information to player
    }

    class AuthorizationUIManager {
        <<MonoBehaviour>>
        Manages authorization loading screen
    }

    %% Item System
    class ItemPickupManager {
        <<MonoBehaviour>>
        Handles item pickup and checkout
    }

    class ItemScript {
        <<MonoBehaviour>>
        Individual item data
    }

    class SaleManager {
        <<MonoBehaviour>>
        Processes sale transactions
    }

    class DraggableItem {
        <<MonoBehaviour>>
        Drag-and-drop functionality
    }

    %% Data Classes
    class CustomerScenarioData {
        <<Data>>
        JSON customer scenario data
    }

    class DialogueLine {
        <<Data>>
        Single dialogue node data
    }

    class CustomerData {
        <<Data>>
        Runtime customer ID data
    }

    class ScenarioConfig {
        <<Data>>
        Scenario queue configuration
    }

    %% UI Components
    class CustomerUIHandler {
        <<MonoBehaviour>>
        Handles customer-specific UI
    }

    %% Relationships - Core Flow
    DialogueManager --> ScenarioManager : loads scenarios
    ScenarioManager --> CustomerScenarioData : reads JSON data
    DialogueManager --> DialogueLine : processes dialogue
    DialogueManager --> UIManager : updates UI

    %% Relationships - ID System
    DialogueManager --> IDScanner : spawns IDs
    IDScanner --> CustomerID : scans
    CustomerID <|-- AuthorizationID : inherits
    CustomerID --> CustomerData : creates runtime data
    AuthorizationID --> AuthorizationUIManager : triggers loading screen
    CustomerID --> IDInfoDisplay : displays info

    %% Relationships - Item System
    DialogueManager --> ItemPickupManager : validates sales
    ItemPickupManager --> ItemScript : manages items
    ItemScript --> DraggableItem : drag functionality
    ItemPickupManager --> SaleManager : processes transactions

    %% Relationships - Audio
    DialogueManager --> SoundManager : plays audio
    SoundManager --> SoundManager : singleton instance

    %% Relationships - Customer UI
    CustomerScenarioData --> CustomerUIHandler : spawns with NPC

    %% Styling
    style DialogueManager fill:#ff6b6b,color:#fff
    style ScenarioManager fill:#4a90e2,color:#fff
    style SoundManager fill:#ffd93d
    style CustomerID fill:#50c878,color:#fff
    style AuthorizationID fill:#50c878,color:#fff
    style ItemPickupManager fill:#9b59b6,color:#fff
```

---

## 8b. Class Organization by Team Member

```mermaid
graph TB
    subgraph "Garon's Scripts"
        GM1[DialogueManager]
        GM2[CustomerUIHandler]
        GM3[CustomerID]
        GM4[CustomerData]
        GM5[IDScanner]
        GM6[IDInfoDisplay]
        GM7[UIManager]
        GM8[TypeWriter]
        GM9[FadeController]
    end

    subgraph "Saab's Scripts"
        SM1[ScenarioManager]
        SM2[CustomerScenarioDialogue]
        SM3[AuthorizationID]
        SM4[AuthorizationUIManager]
    end

    subgraph "Samy's Scripts"
        SAM1[SoundManager]
        SAM2[ItemPickupManager]
        SAM3[ItemScript]
        SAM4[SaleManager]
        SAM5[DraggableItem]
        SAM6[CameraFade]
        SAM7[ItemHover]
    end

    subgraph "Data Classes"
        D1[CustomerScenarioData]
        D2[DialogueLine]
        D3[DialogueResponse]
        D4[ScenarioConfig]
        D5[ScenarioEntry]
    end

    GM1 -.integration.-> SM1
    GM1 -.integration.-> SAM1
    GM1 -.integration.-> SAM2
    SM3 -.extends.-> GM3
    SM1 -.uses.-> D1
    SM1 -.uses.-> D4

    style GM1 fill:#ff6b6b,color:#fff
    style SM1 fill:#4a90e2,color:#fff
    style SAM1 fill:#ffd93d
    style SAM2 fill:#9b59b6,color:#fff
```

---

## 9. Voice-Over System Integration

```mermaid
graph LR
    subgraph "JSON Data"
        JSON[DialogueLineData] --> VOP[voiceOverPath field]
    end

    subgraph "DialogueManager"
        DM[ShowNextLine] --> CHECK{Has voiceOverPath?}
        CHECK -->|Yes| LOAD[Load from Resources]
        CHECK -->|No| SKIP[Skip Audio]
        LOAD --> PLAY[AudioSource.PlayOneShot]
    end

    subgraph "Audio Resources"
        RES[Assets/Resources/VoiceOvers/] --> CLIP[AudioClip MP3]
    end

    VOP --> CHECK
    RES --> LOAD
    PLAY --> SPEAKER[Voice-Over Audio Source]
    SPEAKER --> OUTPUT[Game Audio Output]

    style JSON fill:#50c878
    style PLAY fill:#4a90e2
    style OUTPUT fill:#ffd93d
```

---

## 10. Data Flow - Complete Customer Interaction

```mermaid
graph TD
    START[Customer Scenario Starts] --> LOAD[Load JSON Data]
    LOAD --> SPAWN[Spawn NPC Prefab]
    SPAWN --> DIALOGUE1[Show First Dialogue Line]

    DIALOGUE1 --> PLAYVO[Play Voice-Over]
    PLAYVO --> TYPEWRITER[Typewriter Effect]
    TYPEWRITER --> WAIT{Has Choices?}

    WAIT -->|No| CONTINUE[Show Continue Button]
    WAIT -->|Yes| CHOICES[Show Choice Buttons]

    CONTINUE --> NEXT1[Next Dialogue Line]
    CHOICES --> PLAYER[Player Selects Choice]
    PLAYER --> BRANCH[Jump to Branch]
    BRANCH --> NEXT1

    NEXT1 --> IDREQ{Requires ID?}
    IDREQ -->|Yes| SPAWNID[Spawn ID Prefab]
    IDREQ -->|No| CHECK2{More Dialogue?}

    SPAWNID --> SCAN[Player Scans ID]
    SCAN --> AUTH{Authorization ID?}
    AUTH -->|Yes| AUTHFLOW[Authorization Process]
    AUTH -->|No| SHOWINFO[Show ID Info]

    AUTHFLOW --> SHOWINFO
    SHOWINFO --> GRANT[Grant Data Access]
    GRANT --> CHECK2

    CHECK2 -->|Yes| DIALOGUE1
    CHECK2 -->|No| SALE{Make Sale?}

    SALE -->|Yes| VALIDATE[Validate Items]
    SALE -->|No| ENDDIALOGUE[End Conversation]

    VALIDATE --> VALID{Items Correct?}
    VALID -->|Yes| SUCCESS[Sale Success]
    VALID -->|No| REJECT[Sale Rejected]

    REJECT --> DIALOGUE1
    SUCCESS --> ENDDIALOGUE

    ENDDIALOGUE --> SHOWCARD{Show Business Card?}
    SHOWCARD -->|Yes| CARD[Display Card]
    SHOWCARD -->|No| SCORE[Show Score Screen]

    CARD --> SCORE
    SCORE --> DESTROYNPC[Destroy NPC]
    DESTROYNPC --> NEXTCUST{More Customers?}

    NEXTCUST -->|Yes| START
    NEXTCUST -->|No| CREDITS[End Credits]
    CREDITS --> ENDGAME[Game Complete]

    style START fill:#4a90e2
    style LOAD fill:#50c878
    style PLAYVO fill:#9b59b6
    style SUCCESS fill:#50c878
    style REJECT fill:#ff6b6b
    style ENDGAME fill:#ffd93d
```

---

## How to View These Diagrams

### Option 1: GitHub (Recommended)
1. Push this file to your GitHub repository
2. View on GitHub - Mermaid diagrams render automatically
3. Share the link with your team

### Option 2: VS Code
1. Install "Markdown Preview Mermaid Support" extension
2. Open this file in VS Code
3. Click "Open Preview" (Ctrl+Shift+V)

### Option 3: Online Viewers
- https://mermaid.live/ - Paste diagram code to view/edit
- https://mermaid.ink/ - Generate PNG images from diagrams

### Option 4: Export to PDF
1. Use VS Code with Markdown PDF extension
2. Right-click in preview → "Markdown PDF: Export (pdf)"
3. Diagrams will be included in PDF

---

**Created:** 2025-11-25
**Version:** 1.0
**Companion Document:** PROJECT_DOCUMENTATION.md
