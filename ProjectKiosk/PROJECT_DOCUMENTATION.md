# Project Kiosk - Development Documentation

## Project Overview
**Project Type:** Unity 2D Game - Kiosk Simulation
**Engine:** Unity
**Platform:** WebGL (GitHub Pages Deployment)
**Development Branch:** Development
**Stable Branch:** Stable

## Game Description
A kiosk simulation game where players work as a clerk at a local store, interacting with various customers through a dialogue system. The game explores themes of data privacy, ethical decision-making, and customer service in a futuristic setting with NEU (New European Union) regulations.

---

## Recent Development Work (Last 5 Commits)

### 1. **Build & WebGL Deployment**
- Created WebGL build for GitHub Pages deployment
- Added build files to `GameBuild/` and `docs/` directories
- Includes Unity WebGL template files (logos, icons, styling)

### 2. **Voice-Over System**
- Implemented voice-over audio playback system
- Added voice-over files for Shaun Bakers scenario:
  - `ShaunBaker_Line1.mp3` through `ShaunBaker_Line4.mp3`
- Voice-overs stored in `Assets/Resources/VoiceOvers/`
- Integrated voice-over triggers in DialogueManager
- Audio automatically plays during dialogue lines when specified

### 3. **Authorization System**
- Created authorization mechanic for ID verification
- New components:
  - `AuthorizationID.cs` - Handles authorization ID cards
  - `AuthorizationUIManager.cs` - Manages authorization UI
- Authorization loading screen and verification process
- Integration with existing ID scanning system

### 4. **Score Screens & Scene Transitions**
- Implemented multiple score screens (indices 0-9)
- Automatic Continue button detection in score screens
- Scene transitions between TutorialScene and GameplayScene
- End credits screen for gameplay completion

### 5. **UI & Sound Management Updates**
- Enhanced `UIManager.cs` for better UI control
- Updated `SoundManager.cs` with 31 lines of changes
- Improved scene management in GameplayScene and TutorialScene

---

## Core Systems

### 1. Dialogue System
**Main Script:** `DialogueManager.cs` (1,246 lines)

**Features:**
- **JSON-Based Scenarios** - Customer dialogues loaded from JSON files
- **Branching Conversations** - Multiple choice responses with conditional paths
- **Typewriter Effect** - Animated text display with configurable duration
- **Voice-Over Integration** - Automatic audio playback for dialogue lines
- **Return Stack** - Navigate back to previous dialogue points
- **Business Card System** - Display business cards during conversations
- **NPC Sprite Changes** - Dynamic NPC expressions during dialogue

**Dialogue Structure:**
```
DialogueLine:
- editorIndex: Unique identifier
- speaker: Character name
- text: Dialogue content
- responses: List of player choices
- askForID: Triggers ID scanning
- showGoBackButton: Enable back navigation
- grantNameAccess/DOBAccess/etc: Progressive ID data access
- voiceOverPath: Audio file path
- npcSprite: Sprite change trigger
- endConversationHere: End dialogue and show score
```

### 2. Scenario Management System
**Main Script:** `ScenarioManager.cs` (158 lines)

**Features:**
- **Scenario Queue** - Manages customer scenario order
- **Shuffle Support** - Optional randomization via `ScenarioConfig.json`
- **Follow-Up Scenarios** - Chain scenarios in sequence
- **JSON Configuration** - Define scenarios with display names and order

**Configuration:**
```json
{
  "shuffleEnabled": true/false,
  "scenarios": [
    {
      "filename": "ShaunBakersScenario",
      "displayName": "Shaun Bakers",
      "followUpScenario": "NextScenarioName"
    }
  ]
}
```

### 3. ID Scanning & Verification
**Related Scripts:**
- `IDScanner.cs` - Handles ID scanning mechanics
- `CustomerID.cs` - ID data structure
- `IDInfoDisplay.cs` - UI display for ID information
- `AuthorizationID.cs` - Special authorization IDs

**Features:**
- Drag-and-drop ID scanning
- Progressive data access (name, DOB, address, issuer, picture)
- Authorization verification with loading screen
- Multiple ID prefabs per scenario

### 4. Item Management System
**Related Scripts:**
- `ItemPickupManager.cs` - Handle item pickup/drop
- `ItemScript.cs` - Item data
- `SaleManager.cs` - Transaction processing
- `DraggableItem.cs` - Drag functionality

**Features:**
- Pick up items from shelves
- Checkout drop zone
- Item category verification
- Sale validation (correct items check)

### 5. Sound System
**Main Script:** `SoundManager.cs`

**Features:**
- Global sound manager (Singleton pattern)
- Separate voice-over audio source
- Sale success/rejection sounds
- Background music support

---

## Project Structure

```
ProjectKiosk/
├── Assets/
│   ├── Garon Scripts/
│   │   ├── Gamescripts/
│   │   │   ├── DialogueManager.cs
│   │   │   ├── CustomerUIHandler.cs
│   │   │   ├── CustomerID.cs
│   │   │   ├── CustomerData.cs
│   │   │   └── IDScanner.cs
│   │   ├── UIManager.cs
│   │   ├── FadeController.cs
│   │   └── TypeWriter.cs
│   │
│   ├── SaabScripts/
│   │   ├── ScenarioManager.cs
│   │   ├── CustomerScenarioDialogue.cs
│   │   ├── AuthorizationID.cs
│   │   └── AuthorizationUIManager.cs
│   │
│   ├── SamyScripts/
│   │   ├── SoundManager.cs
│   │   ├── ItemPickupManager.cs
│   │   ├── ItemScript.cs
│   │   ├── SaleManager.cs
│   │   └── CameraFade.cs
│   │
│   ├── Resources/
│   │   ├── VoiceOvers/
│   │   │   └── ShaunBaker_Line[1-4].mp3
│   │   ├── ShaunBakersScenario.json
│   │   ├── ScenarioConfig.json
│   │   ├── NPC_Prefabs/
│   │   ├── ID_Prefabs/
│   │   └── Cards/
│   │
│   └── Scenes/
│       ├── StartScreen.unity
│       ├── TutorialScene.unity
│       ├── GameplayScene.unity
│       ├── AltMainScreen.unity
│       └── GeneralTesting.unity
│
├── GameBuild/ - WebGL build output
├── docs/ - GitHub Pages deployment
└── PROJECT_DOCUMENTATION.md
```

---

## Current Scenarios

### 1. **Shaun Bakers Scenario** (ShaunBakersScenario.json)
**Character:** Shady data broker
**Theme:** Data privacy and ethical choices
**Dialogue Lines:** 40+ unique dialogue nodes

**Key Decision Points:**
- Listen to offer or refuse immediately
- Learn more about data brokering
- Sell customer data to broker
- Buy data from broker
- Call police or handle peacefully

**Outcomes:**
- Score Screen 6: Refuse completely (best outcome)
- Score Screen 7: Refuse after learning more
- Score Screen 8: Sell customer data (worst outcome)
- Score Screen 9: Buy data from broker

**Features Used:**
- Voice-overs (4 audio files)
- NPC sprite changes (Normal/Angry expressions)
- Business card display
- Multiple branching paths

---

## Technical Features

### Voice-Over System
- **Location:** `Assets/Resources/VoiceOvers/`
- **Format:** MP3
- **Integration:** Specified in dialogue JSON via `voiceOverPath`
- **Playback:** Automatic via DialogueManager's AudioSource

### Authorization System
- **Purpose:** Verify special authorization IDs
- **Process:** Loading screen → Verification → Status update
- **UI:** Managed by AuthorizationUIManager
- **Integration:** DialogueManager detects AuthorizationID component

### Score Screens
- **Count:** 10 screens (indices 0-9)
- **Auto-Detection:** Finds Continue buttons automatically
- **Flow:** Show score → Continue → Next scenario
- **Transitions:** Tutorial → Gameplay → End Credits

### JSON Scenario Format
Customer scenarios support:
- Custom NPC prefabs
- Custom ID prefabs
- Requested items list
- Dialogue tree with branches
- Voice-over paths
- Sprite changes
- Business card displays

---

## Development Team Contributions

### Garon's Work
- Dialogue system implementation
- Customer UI handling
- ID scanning mechanics
- Fade controllers and typewriter effects

### Saab's Work
- Scenario management system
- Authorization mechanics
- Customer scenario dialogue integration
- JSON-based scenario loading

### Samy's Work
- Sound management system
- Item pickup and sale mechanics
- Camera effects
- UI helpers (hover, restart)

---

## Build & Deployment

### WebGL Build
- **Location:** `GameBuild/` and `docs/`
- **Platform:** WebGL
- **Deployment:** GitHub Pages
- **Status:** Build complete and deployed

### Build Settings
- TutorialScene → GameplayScene flow
- WebGL optimized
- Responsive canvas scaling

---

## Git Workflow

### Branches
- **Development:** Active development branch
- **Stable:** Production-ready code
- **Feature Branches:** Individual feature development (e.g., SaabNewGameUpdate)

### Recent Merges
- SaabNewGameUpdate → Development (Authorization system, voice-overs)

---

## Future Considerations

### Potential Enhancements
1. More customer scenarios
2. Additional voice-over recordings
3. More authorization types
4. Expanded item catalog
5. Achievement system
6. Save/load functionality
7. Localization support

### Known Technical Debt
- Legacy customer system (pre-JSON) still exists
- Manual deprecation warnings in code
- Some hardcoded values (e.g., NPC scale in DialogueManager:579)

---

## How to Add New Scenarios

1. **Create JSON File** in `Assets/Resources/`
   - Follow ShaunBakersScenario.json structure
   - Use editorIndex for dialogue node IDs

2. **Create NPC Prefab** in `Assets/Resources/NPC_Prefabs/`
   - Add Image component
   - Add CustomerUIHandler component

3. **Create ID Prefab** in `Assets/Resources/ID_Prefabs/`
   - Add CustomerID component
   - Set up ID data

4. **Update ScenarioConfig.json**
   - Add scenario entry with filename
   - Set displayName and optional followUpScenario

5. **Add Voice-Overs** (optional)
   - Place in `Assets/Resources/VoiceOvers/`
   - Reference in JSON via voiceOverPath

6. **Test in Unity**
   - Load TutorialScene or GameplayScene
   - Verify dialogue flow and interactions

---

## Key Gameplay Mechanics

### ID Verification Flow
1. Customer asks for verification
2. ID spawns at designated location
3. Player scans ID (drag interaction)
4. Data progressively revealed based on dialogue choices
5. Authorization IDs trigger special verification process

### Sale Transaction Flow
1. Customer requests specific items
2. Player picks items from shelves
3. Items placed in checkout zone
4. "Make Sale" response validates items
5. Success: Items sold, positive feedback
6. Failure: Items returned, customer rejects

### Dialogue Navigation
1. Linear dialogue with Continue button
2. Choice nodes with multiple responses
3. Go Back buttons for returning to previous nodes
4. Return stack for complex branching
5. End conversation triggers score screen

---

## Conclusion

This project demonstrates a sophisticated dialogue-driven gameplay system with ethical decision-making at its core. The modular architecture allows for easy addition of new scenarios and features, while the JSON-based approach enables non-programmers to create new content.

**Last Updated:** 2025-11-25
**Documentation Version:** 1.0
**Project Status:** Active Development
