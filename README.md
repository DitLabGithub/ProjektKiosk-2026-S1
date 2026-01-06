# 🏪 KIOSK CLERK GAME

A narrative game where you play as a kiosk clerk. Make moral choices, scan IDs, sell items, and try not to get arrested!

---

## 🚀 GETTING STARTED

### 1. Prerequisites
- **Unity Hub** installed
- **Unity 2022.3.58f1** (exact version required)
- **WebGL Build Support** module (if building for web)

### 2. Clone the Repository
```bash
git clone https://github.com/your-repo/ProjektKiosk-P-S.git
cd ProjektKiosk-P-S
```

### 3. Open in Unity
1. Open **Unity Hub**
2. Click **"Add"** → Navigate to `ProjektKiosk-P-S/ProjectKiosk` folder
3. Select Unity version **2022.3.58f1**
4. Click the project to open it
5. Wait for Unity to import assets (may take a few minutes)

### 4. Run the Game
1. In Unity, go to **File → Build Settings**
2. Ensure scenes are in this order:
   - `MainMenuScene` (index 0)
   - `TutorialScene` (index 1)
   - `GameplayScene` (index 2)
3. Close Build Settings
4. Open **`Assets/Scenes/MainMenuScene.unity`**
5. Press the **Play ▶️** button at the top

**That's it!** The game should start.

---

## 🎮 GAME OVERVIEW

### What's the Game About?
You're a kiosk clerk in 2030. Customers come in, you check their IDs, sell them items. But some customers will offer you bribes or try to manipulate you. Will you stay honest or get rich through corruption?

### Game Mechanics
- **Dialogue Choices:** Talk with customers, choose responses
- **ID Scanning:** Drag IDs to scanner to verify customers
- **Item Checkout:** Pick items from shelves, place in checkout, make sales
- **Corruption Score:** Bad choices give you points (reach 70 = arrested)
- **Money:** Earn from legitimate sales (€5-15) or shady deals (€50-1000)

---

## 📂 SCENES BREAKDOWN

### 1. **MainMenuScene** (Start Here)
**Location:** `Assets/Scenes/MainMenuScene.unity`

**What it does:**
- Displays main menu with "Start Game" button
- Shows story intro (3 dialogue screens about year 2030)
- Loads TutorialScene when you click "Start Game"

**How to test:**
1. Open this scene
2. Press Play
3. Click through the story dialogues
4. Click "Start Game" → loads tutorial

---

### 2. **TutorialScene**
**Location:** `Assets/Scenes/TutorialScene.unity`

**What it does:**
- First customer interaction (practice scenario)
- Teaches you how to:
  - Talk to customers
  - Scan IDs
  - Pick up items
  - Make sales
- When finished, loads GameplayScene

**How to test:**
1. Open this scene
2. Press Play
3. Click the NPC to start dialogue
4. Follow instructions
5. Complete the sale
6. See score screen → continues to main game

---

### 3. **GameplayScene** (Main Game)
**Location:** `Assets/Scenes/GameplayScene.unity`

**What it does:**
- This is the full game loop
- Multiple customers appear one after another
- Each customer is a scenario from `Assets/Resources/`
- Scenarios are queued by `ScenarioManager`
- Police scenarios trigger at score thresholds (50, 70)
- Game ends when all scenarios complete or you get arrested

**How to test:**
1. Open this scene
2. Press Play
3. Game loads first scenario from queue
4. Click NPC → dialogue starts
5. Make choices, scan IDs, sell items
6. Score screen appears at end
7. Click Continue → next scenario loads
8. Repeat until game ends

**What's in this scene:**
- **UI Canvas:** All dialogue UI, buttons, text
- **ScenarioManager:** Controls scenario queue
- **ScoreManager:** Tracks corruption (press 4, 5, 0 to test)
- **ItemPickupManager:** Handles money and items
- **DialogueManager:** Runs conversations
- **InboxUIManager:** Email notifications (top-right mail icon)
- **Checkout Area:** Where you place items to sell
- **Shelves:** Items you can pick up (click to grab)

---

## 🗂️ GAME ELEMENTS

### Customer Scenarios (JSON Files)
**Location:** `Assets/Resources/*_Scenario.json`

Each file is a complete customer interaction:
- **ConspiracyGuy_Scenario.json** - Conspiracy theorist offering data deals
- **Robin_Scenario.json** - Tourist buying cigarettes
- **ShadyGuy_Intro_Scenario.json** - Shady data broker (big money offer)
- **Amon_Scenario.json** - Regular customer
- **Fridgy_Scenario.json** - AI fridge scenario
- **PoliceGuy_Warning_Scenario.json** - Police warning (auto-triggers at 50 points)
- **PoliceGuy_Arrest_Scenario.json** - Police arrest (auto-triggers at 70 points)

### Scenario Queue Config
**Location:** `Assets/Resources/ScenarioConfig.json`

Controls which scenarios play and in what order:
```json
{
  "shuffleEnabled": true,
  "scenarios": [
    {"filename": "ConspiracyGuy_Scenario", "displayName": "Conspiracy Guy"},
    {"filename": "Robin_Scenario", "displayName": "Robin Banks"}
  ]
}
```

- `shuffleEnabled: true` = Random order each playthrough
- `shuffleEnabled: false` = Always same order

### NPCs (Customer Visuals)
**Location:** `Assets/Resources/NPC_Prefabs/`

Visual prefabs for each customer (images, UI setup)

### ID Cards
**Location:** `Assets/Resources/ID_Prefabs/`

ID card prefabs customers show you when asked

### Items (Store Products)
**Location:** Prefabs placed in GameplayScene on shelves

17 items available:
- Snacks: ChipsBag, Sproingles, BoxOfChocolates, Chicken_Jerky
- Drinks: SodaCan, BeerBottle, various beers, WineBottle
- Tobacco: BluePort, Rambolo, HotShot cigarettes
- Other: DirtyMagazine, NerdComics, Package

---

## 🎯 HOW TO PLAY

### Starting the Game
1. **Main Menu** → Click "Start Game"
2. **Story Intro** → Read 3 dialogue screens about the world
3. **Tutorial** → First customer teaches you mechanics
4. **Main Game** → Scenarios play one after another

### During Gameplay

#### Talking to Customers
1. **Click NPC** to start conversation
2. **Read dialogue** (text types out character by character)
3. **Click "Continue"** to advance
4. **Click response buttons** to make choices

#### Scanning IDs
1. Customer says they'll show ID
2. **ID card appears** in the scene
3. **Drag ID to scanner** (rectangle on right side)
4. **ID info displays** (some fields may say "[Access Denied]")
5. **Click Continue** to proceed with dialogue

#### Selling Items
1. **Click items on shelves** to pick up (2 hands max)
2. **Click checkout counter** to place items down
3. Total price updates automatically
4. **In dialogue, click "Make Sale"** response when ready
5. System validates items match what customer requested
6. If correct: success sound, money added, items disappear
7. If wrong: rejection sound, try again

#### Making Moral Choices
Some dialogue choices have consequences:
- **Corruption Points:** `+5 to +15` points for bad choices
- **Money Rewards:** `€50 to €1000` for shady deals
- **Police Triggers:** Reach 50 points = warning, 70 points = arrest

Watch for:
- Selling without checking ID (illegal)
- Accepting bribes
- Selling customer data
- Asking unnecessary questions (privacy invasion)

### Score Thresholds
- **15 points:** First inbox warning
- **30 points:** Second inbox warning
- **45 points:** Third inbox warning
- **50 points:** Police visit scenario
- **70 points:** Police arrest → Game Over

### Inbox System
- **Mail icon** (top-right corner) shows notifications
- **Red badge** appears when new messages arrive
- **Click mail icon** to read warnings from government
- Messages get more serious as corruption increases

---

## 🧪 TESTING THE GAME

### Quick Test (5 minutes)
1. Open **GameplayScene.unity**
2. Press Play
3. Click NPC when it appears
4. Make dialogue choices
5. Test ID scanning
6. Test item pickup/checkout
7. Complete the scenario

### Test Score System
Press number keys during play mode in GameplayScene:
- **Press 1:** Jump to 15 points (first warning)
- **Press 2:** Jump to 30 points
- **Press 3:** Jump to 45 points
- **Press 4:** Jump to 50 points (police warning triggers)
- **Press 5:** Jump to 70 points (arrest triggers)
- **Press 0:** Reset to 0 points

This lets you test police scenarios without playing through the whole game.

### Test Specific Scenarios
1. Open `Resources/ScenarioConfig.json`
2. Set `"shuffleEnabled": false`
3. Rearrange `scenarios` array to put your test scenario first
4. Save and press Play

### Check Console Logs
Watch Unity Console while playing:
- `[DialogueManager]` - Dialogue events
- `[ScoreManager]` - Score changes
- `[ScenarioManager]` - Scenario loading
- `[ItemPickupManager]` - Money changes

All systems log what they're doing for debugging.

---

## 🐛 COMMON ISSUES

### "Scene not found" error when transitioning
**Fix:**
1. Go to **File → Build Settings**
2. Add all 3 scenes in order:
   - MainMenuScene (index 0)
   - TutorialScene (index 1)
   - GameplayScene (index 2)
3. Close and try again

### NPC doesn't appear in GameplayScene
**Fix:**
- Check `Resources/ScenarioConfig.json` has scenarios listed
- Verify `ScenarioManager` exists in scene hierarchy
- Check Console for errors

### ID doesn't scan
**Fix:**
- Make sure you're dragging ID to the **scanner rectangle** (right side)
- Check `IDScanner` GameObject exists in scene
- Verify `DialogueManager` reference is set in IDScanner

### Items won't pick up
**Fix:**
- Ensure items have `ItemSlotData` component
- Check Layer is set to `PickupItem`
- Verify both hands aren't already full (max 2 items)

### Sale keeps getting rejected
**Fix:**
- Items must **exactly match** what customer requested
- Check `requestedItems` array in scenario JSON
- Verify items are in **checkout zone**, not in hands

### Score not updating
**Fix:**
- Check Console for `[DialogueManager] Added X points` message
- Ensure `ScoreManager` GameObject exists in scene
- Verify dialogue response has `"scoreValue": 10` in JSON

---

## 🔧 BUILD FOR WEB

### WebGL Build
1. **File → Build Settings**
2. Select **WebGL** platform
3. Click **"Switch Platform"** (if not already selected)
4. Click **"Build"** or **"Build and Run"**
5. Choose output folder
6. Wait for build to complete (5-10 minutes)

### Testing WebGL Build
1. After build completes, navigate to output folder
2. Open `index.html` in **Chrome** or **Firefox**
3. **Note:** WebGL builds don't work if opened directly from file system in some browsers
4. Use a local server (like Python's `http.server` or Unity's "Build and Run")

---

## 📋 PROJECT STRUCTURE

```
ProjektKiosk-P-S/
└── ProjectKiosk/              ← Unity project folder
    └── Assets/
        ├── Scenes/
        │   ├── MainMenuScene.unity      ← Start menu
        │   ├── TutorialScene.unity      ← Tutorial
        │   └── GameplayScene.unity      ← Main game
        │
        ├── Resources/
        │   ├── ScenarioConfig.json      ← Scenario queue
        │   ├── *_Scenario.json          ← Customer scenarios
        │   ├── NPC_Prefabs/             ← Customer visuals
        │   ├── ID_Prefabs/              ← ID cards
        │   ├── NPC_Sprites/             ← Sprite variations
        │   └── Cards/                   ← Business cards
        │
        ├── SaabScripts/                 ← Core game logic
        ├── Garon Scripts/               ← Dialogue & UI
        └── SamyScripts/                 ← Items & checkout
```

---

## 🎓 UNDERSTANDING THE GAME FLOW

### Complete Playthrough Flow
```
1. Main Menu
   ↓ (Click "Start Game")
2. Story Intro (3 dialogue screens)
   ↓
3. Tutorial Scene
   → First customer
   → Learn mechanics
   → Complete sale
   ↓
4. Gameplay Scene Loop:
   → Customer appears
   → Dialogue starts
   → Make choices
   → Scan ID (if needed)
   → Sell items
   → Score screen
   → Next customer
   ↓
5. Police Scenarios (if score high enough)
   → Warning at 50 points
   → Arrest at 70 points
   ↓
6. Game End
   → End credits
   → OR arrested by police
```

### Scenario System Flow
```
ScenarioManager loads ScenarioConfig.json
   ↓
Builds queue of scenarios
   ↓
(Optional) Shuffles queue
   ↓
DialogueManager requests next scenario
   ↓
Loads JSON → spawns NPC → starts dialogue
   ↓
Player completes scenario
   ↓
Score screen appears
   ↓
Player clicks Continue
   ↓
ScenarioManager checks:
   - Follow-up scenario?
   - Police threshold crossed?
   - More scenarios in queue?
   ↓
Either: Load next scenario OR end game
```

---

## 💡 TIPS FOR FIRST-TIME TESTERS

### What to Look For
1. **Test all dialogue branches** - Click different responses
2. **Try selling wrong items** - See if validation works
3. **Test ID scanning** - Drag IDs around, see if it registers
4. **Check score triggers** - Use number keys to jump to 50/70 points
5. **Look at inbox** - Click mail icon to see warning messages
6. **Test full playthrough** - Start from main menu, play to end

### What to Report
- Scenarios that don't load
- Dialogue that gets stuck
- Items that can't be picked up
- Sales that don't validate correctly
- Crashes or freezes
- Visual glitches
- Typos in dialogue

---

## 🛠️ FOR DEVELOPERS

Want to add content or modify the game? Check these files:

### To Add New Scenarios
- Edit JSON files in `Assets/Resources/`
- See `Robin_Scenario.json` for simple example
- See `ShadyGuy_Intro_Scenario.json` for complex example

### To Modify Score Thresholds
- Open `GameplayScene.unity`
- Select `ScoreManager` in Hierarchy
- Edit `inboxThresholds[]` and `policeScenarios[]` in Inspector

### To Change Scenario Order
- Edit `Assets/Resources/ScenarioConfig.json`
- Reorder `scenarios` array
- Set `shuffleEnabled: false` for fixed order

### Key Scripts
- `DialogueManager.cs` - Dialogue system
- `ScoreManager.cs` - Corruption tracking
- `ScenarioManager.cs` - Scenario queue
- `ItemPickupManager.cs` - Money & items

---

## 📞 SUPPORT

**Repository:** ProjektKiosk-P-S
**Branches:**
- `Development` - Active development
- `Stable` - Production-ready

**Built by:** Saab, Garon, Samy

---

**Ready to test? Open MainMenuScene.unity and press Play!** ▶️
