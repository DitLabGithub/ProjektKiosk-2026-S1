# Score System Documentation

## Overview
The Score System tracks player corruption through their dialogue choices during gameplay. When players make morally questionable decisions (like skipping ID verification or accepting bribes), they accumulate corruption points. At specific thresholds, police scenarios are dynamically triggered.

---

## System Architecture

### Components

#### 1. **ScoreManager.cs** (`Assets/Garon Scripts/Gamescripts/ScoreManager.cs`)
**Purpose:** Singleton that tracks corruption score and triggers events when thresholds are reached.

**Key Features:**
- Manages current score (0-30 by default)
- Updates Morality Score UI in real-time
- Contains expandable list of police scenario thresholds
- Fires events when thresholds are crossed
- Persists across scenes with `DontDestroyOnLoad`

**Inspector Configuration:**
```
Max Score: 30
Score Text: [TMP Text Reference]
Police Scenarios:
  - Entry 0:
    - Scenario Name: "Police Warning"
    - Score Threshold: 15
    - Json File Name: "PoliceGuyScenario"
  - Entry 1:
    - Scenario Name: "Police Arrest"
    - Score Threshold: 30
    - Json File Name: "PoliceGuy2Scenario"
```

**Public Methods:**
- `AddScore(int points)` - Adds corruption points and checks thresholds
- `GetCurrentScore()` - Returns current score
- `ResetScore()` - Resets score to 0 (for new game)
- `SetScoreText(TextMeshProUGUI textComponent)` - Assigns UI text at runtime

**Events:**
- `OnScoreChanged(int newScore)` - Fired when score changes
- `OnPoliceThresholdReached(PoliceScenarioThreshold scenario)` - Fired when threshold crossed

---

#### 2. **DialogueManager.cs** (Modified)
**Purpose:** Handles dialogue flow and detects when responses with scoreValue are chosen.

**Key Changes:**

**DialogueResponse Class** (Line 14-25):
```csharp
public class DialogueResponse
{
    public string responseText;
    public int nextLineIndex = -1;
    public bool returnAfterResponse = false;
    public bool activateContinueAfterChoice = false;
    public bool isMakeSaleResponse = false;

    // Score System
    public int scoreValue = 0; // Points to add when this response is chosen
}
```

**Score Detection Logic** (Line 877-882):
```csharp
// Score System: Add points if this response has a scoreValue
if (currentResponse.scoreValue > 0 && ScoreManager.Instance != null)
{
    ScoreManager.Instance.AddScore(currentResponse.scoreValue);
    Debug.Log($"[DialogueManager] Added {currentResponse.scoreValue} points for response: '{currentResponse.responseText}'");
}
```

**JSON Data Conversion** (Line 769):
```csharp
scoreValue = r.scoreValue // Copy score value from JSON data
```

---

#### 3. **CustomerScenarioDialogue.cs** (Modified)
**Purpose:** Defines JSON data structure for scenarios.

**DialogueResponseData Class** (Line 8-18):
```csharp
[Serializable]
public class DialogueResponseData
{
    public string responseText;
    public int nextLineIndex = -1;
    public bool returnAfterResponse = false;
    public bool activateContinueAfterChoice = false;
    public bool isMakeSaleResponse = false;

    // Score System - corruption points awarded when this response is chosen
    public int scoreValue = 0;
}
```

---

#### 4. **ScenarioManager.cs** (Modified)
**Purpose:** Manages scenario queue and injects police scenarios when triggered.

**Key Features:**
- Subscribes to `ScoreManager.OnPoliceThresholdReached` event
- Injects police scenarios at front of queue when thresholds are hit
- Retries subscription if ScoreManager isn't ready yet

**Event Handler** (Line 192-203):
```csharp
private void OnPoliceThresholdReached(ScoreManager.PoliceScenarioThreshold policeScenario)
{
    Debug.Log($"[ScenarioManager] Injecting police scenario: {policeScenario.jsonFileName}");

    // Insert police scenario at the front of the queue
    Queue<string> tempQueue = new Queue<string>();
    tempQueue.Enqueue(policeScenario.jsonFileName);

    // Add remaining scenarios after the police scenario
    while (scenarioQueue.Count > 0)
    {
        tempQueue.Enqueue(scenarioQueue.Dequeue());
    }

    scenarioQueue = tempQueue;
}
```

---

## How to Add Score to Scenarios

### Step 1: Add scoreValue to JSON Response

Open any scenario JSON file (e.g., `Assets/Resources/fridgy_scenario.json`) and add `scoreValue` to responses:

```json
{
  "editorIndex": 50,
  "speaker": "Fridgy",
  "text": "TWO IDs? Beep boop, beer where?",
  "responses": [
    {
      "responseText": "Would you share your story?",
      "nextLineIndex": 60,
      "returnAfterResponse": true
    },
    {
      "responseText": "Continue",
      "nextLineIndex": 140,
      "scoreValue": 15
    }
  ],
  "disableContinueButton": true
}
```

**Important:**
- `scoreValue` is optional - only add it to morally questionable choices
- Can be any integer (5, 10, 15, 30, etc.)
- Only works in **Gameplay scene** (Tutorial has no ScoreManager)

### Step 2: Save and Test

1. Save the JSON file
2. Run Gameplay scene
3. Make the choice with scoreValue
4. Check console for: `[DialogueManager] Added X points for response: 'Response Text'`
5. Watch Morality Score UI update

---

## Police Scenario Configuration

### Adding New Police Scenarios

1. **Create the JSON file** in `Assets/Resources/`
   - Example: `PoliceInterrogationScenario.json`

2. **Open Gameplay Scene in Unity**

3. **Select ScoreManager GameObject**

4. **In Inspector, expand Police Scenarios list**

5. **Click "+" to add new entry:**
   ```
   Scenario Name: "Police Interrogation"
   Score Threshold: 20
   Json File Name: "PoliceInterrogationScenario"
   ```
   ⚠️ **Do NOT include .json extension!**

6. **Save Scene**

### Threshold Behavior

- **Police scenarios trigger ONCE per threshold**
- **Thresholds are cumulative** (player must reach exact threshold or higher)
- **Scenarios inject immediately** after current scenario completes
- **Game continues** after scenario unless it has ending logic

**Example Flow:**
```
Player Score: 0
  ↓ (plays Fridgy, chooses corrupt option)
Player Score: 15
  → Police Warning injects
  → Police Warning plays
  → Game continues
  ↓ (plays Amon, accepts bribe)
Player Score: 45 (15 + 30)
  → Police Arrest injects
  → Police Arrest plays
  → Game ends
```

---

## Game Flow

### Tutorial Scene
**Order:** Hoss → Robin → Shaun Bakers
**Score System:** ❌ Disabled (no ScoreManager in Tutorial)
**Purpose:** Teach mechanics without consequences

### Gameplay Scene
**Scenarios:** Fridgy, Amon (shuffle randomly)
**Score System:** ✅ Enabled
**Police Scenarios:** Injected dynamically based on score

---

## Score Values by Scenario

### Current Implementation

#### **Fridgy Scenario** (`fridgy_scenario.json`)
- **Line 50** - "Continue" (skip proper ID verification): `+15 points`
- Triggers at editorIndex 50 when player skips double ID requirement

#### **Amon Scenario** (`AmonScenario.json`)
- **Line 250** - "Make the sale" (sell to underage with bribe): `+30 points`
- Triggers at editorIndex 250 when player accepts bribe and makes illegal sale

### Recommended Score Values

| Action Type | Score | Example |
|-------------|-------|---------|
| Minor violation | 5 | Rushed ID check |
| Moderate violation | 10-15 | Skipping verification step |
| Major violation | 20-25 | Accepting small bribe |
| Severe violation | 30+ | Major illegal sale |

---

## Debugging

### Console Logs to Watch For

**ScoreManager Initialization:**
```
[ScoreManager] Instance created and set to DontDestroyOnLoad
[ScoreManager] Initialized with 2 police scenarios:
  - Police Warning @ 15 points → PoliceGuyScenario
  - Police Arrest @ 30 points → PoliceGuy2Scenario
```

**ScenarioManager Subscription:**
```
[ScenarioManager] Successfully subscribed to police threshold events
```

**Score Addition:**
```
[DialogueManager] Added 15 points for response: 'Continue'
[ScoreManager] Score increased by 15. Total: 15/30
```

**Police Trigger:**
```
[ScoreManager] 'Police Warning' threshold reached at 15 points!
[ScenarioManager] Injecting police scenario: PoliceGuyScenario (Threshold: 15)
[ScenarioManager] Police scenario queued. Next scenario will be: PoliceGuyScenario
```

### Common Issues

**❌ Score not updating:**
- Check ScoreManager exists in Gameplay scene
- Verify scoreText is assigned in Inspector
- Confirm scoreValue is in JSON file
- Check console for `[DialogueManager] Added X points` message

**❌ Police scenario not triggering:**
- Verify police scenarios are configured in ScoreManager Inspector
- Check Json File Name matches the file in Resources folder (no .json extension)
- Confirm threshold was reached with console logs
- Verify ScenarioManager subscribed successfully

**❌ Game ends after Police Warning:**
- This was fixed in `DialogueManager.cs` line 535-537
- Game now checks `scenarioManager.HasMoreScenarios()` instead of fixed count

**❌ "JSON file not found" error:**
- Double-check Json File Name in Inspector (case-sensitive)
- Verify JSON file exists in `Assets/Resources/`
- Ensure no .json extension in Inspector field
- Check for typos or extra spaces

---

## Future Enhancements

### Potential Features

1. **Score Decay** - Points decrease over time for good behavior
2. **Score Categories** - Separate scores for different violation types
3. **Score Visibility Toggle** - Hide score from player for tension
4. **Multiple Endings** - Different end screens based on final score
5. **Achievement System** - Unlock achievements at score milestones
6. **Save Score** - Persist score across game sessions

### Adding More Police Scenarios

Simply expand the Police Scenarios list in ScoreManager Inspector:
```
Entry 2:
  Scenario Name: "Police Investigation"
  Score Threshold: 22
  Json File Name: "PoliceInvestigationScenario"
```

Police scenarios will trigger in order based on thresholds.

---

## Technical Notes

### Singleton Pattern
ScoreManager uses singleton pattern with `DontDestroyOnLoad` to persist across scenes.

### Event-Driven Architecture
System uses C# events for loose coupling:
- ScoreManager fires events
- ScenarioManager listens and reacts
- DialogueManager triggers score changes

### Dynamic Queue Injection
Police scenarios are injected into the front of the scenario queue, ensuring they play immediately after the current scenario completes.

### Score Persistence
Currently resets on game restart. To persist, implement `PlayerPrefs` or save system:
```csharp
PlayerPrefs.SetInt("CorruptionScore", currentScore);
```

---

## File Locations

```
ProjectKiosk/
├── Assets/
│   ├── Garon Scripts/
│   │   └── Gamescripts/
│   │       ├── DialogueManager.cs (Modified)
│   │       └── ScoreManager.cs (NEW)
│   ├── SaabScripts/
│   │   ├── CustomerScenarioDialogue.cs (Modified)
│   │   └── ScenarioManager.cs (Modified)
│   ├── Resources/
│   │   ├── fridgy_scenario.json (scoreValue: 15)
│   │   ├── AmonScenario.json (scoreValue: 30)
│   │   ├── PoliceGuyScenario.json (Warning)
│   │   └── PoliceGuy2Scenario.json (Arrest)
│   └── Scenes/
│       ├── TutorialScene.unity (No score system)
│       └── GameplayScene.unity (Score system active)
└── SCORE_SYSTEM_DOCUMENTATION.md (This file)
```

---

## Summary

The Score System adds dynamic consequences to player choices in the Gameplay scene. By assigning `scoreValue` to dialogue responses, you can track player morality and trigger special scenarios at specific thresholds. The system is expandable, allowing for easy addition of new police scenarios and score thresholds through Unity's Inspector.

**Key Takeaway:** Add `"scoreValue": X` to any JSON response to make it affect the player's corruption score!

---

*Last Updated: December 2025*
*Created by: Claude Code*
