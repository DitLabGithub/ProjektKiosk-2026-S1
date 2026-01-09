# JSON Scenario Creation Guide for Interns

**Welcome, new developer!** You're about to create customer scenarios for the Kiosk Clerk game. This guide will teach you everything you need to know about our JSON scenario system - no previous experience required.

## What You'll Learn
- How to create a customer from scratch
- How to write branching dialogue with player choices
- How to integrate ID scanning, item purchases, and corruption
- Pro tips and common pitfalls to avoid

**Time to complete**: 15-20 minutes to read, 30 minutes to create your first scenario

---

## Table of Contents
1. [Quick Start](#quick-start)
2. [JSON Scenario Anatomy](#json-scenario-anatomy)
3. [Dialogue Lines Explained](#dialogue-lines-explained)
4. [Response Choices](#response-choices)
5. [Common Patterns](#common-patterns)
6. [Advanced Features](#advanced-features)
7. [Full Example Scenario](#full-example-scenario)
8. [Troubleshooting](#troubleshooting)

---

## Quick Start

**What is a JSON scenario?**
A JSON file that defines:
- Who the customer is (name, appearance, ID)
- What they want to buy
- What they say (dialogue)
- How you can respond (choices)
- What happens next (branching paths)

**Where do scenarios live?**
```
ProjectKiosk/Assets/Resources/YourScenario_Scenario.json
```

**How to add your scenario to the game:**
1. Create your JSON file in `Assets/Resources/`
2. Open `ScenarioConfig.json`
3. Add your scenario to the list:
```json
{
  "filename": "YourScenario_Scenario",
  "displayName": "Your Customer Name",
  "followUpScenario": ""
}
```
4. Test in Unity!

---

## JSON Scenario Anatomy

Here's the skeleton of every scenario file:

```json
{
  "customerName": "Who is this customer?",
  "npcPrefabPath": "NPC_Prefabs/TheirVisualPrefab",
  "idPrefabPath": "ID_Prefabs/TheirIDPrefab",
  "idName": "Name on their ID",
  "idDOB": "Age or birthdate",
  "idAddress": "Where they live",
  "idIssuer": "Who issued the ID (NEU ID, Police Department, etc.)",
  "requestedItems": [11, 7],
  "lines": [
    { /* Dialogue line 1 */ },
    { /* Dialogue line 2 */ },
    { /* ... more lines */ }
  ]
}
```

### Field Breakdown

| Field | What It Does | Example |
|-------|--------------|---------|
| `customerName` | Customer's name shown in UI | `"Robin Banks"` |
| `npcPrefabPath` | Visual character prefab | `"NPC_Prefabs/RobinBanks"` |
| `idPrefabPath` | ID card prefab | `"ID_Prefabs/RobinBanksID"` |
| `idName` | Name on ID (can differ from customerName!) | `"Robin Banks"` |
| `idDOB` | Date of birth or age | `"Over 18"` or `"16 years old"` |
| `idAddress` | Address on ID | `"Tourist Address"` |
| `idIssuer` | ID issuing authority | `"NEU ID"` or `"Police Department"` |
| `requestedItems` | Item IDs they want to buy (see Item List below) | `[11]` = HotShot Cigarettes |

### Item ID Reference (requestedItems)

```
0  = ChipsBag (€3.00)          9  = WineBottle (€7.50)
1  = Sproingles (€2.50)        10 = BluePort Cigarettes (€14.00)
2  = BoxOfChocolates (€10.00)  11 = HotShot Cigarettes (€15.00)
3  = FamilyChips (€5.00)       12 = Rambolo Cigarettes (€16.00)
4  = SodaCan (€2.50)           13 = DirtyMagazine (€12.00)
5  = BeerBottle (€3.50)        14 = NerdComics (€5.50)
6  = Haynako Beer (€15.00)     15 = Chicken Jerky (€6.00)
7  = Giddy Beer (€3.00)        16 = Package (€0.00)
8  = Akira Beer (€16.00)
```

**Pro Tip:** Use item IDs that match your scenario theme. Selling alcohol to minors? Use beer IDs (5-8). Police officer? Cigarettes (10-12) or snacks (0-3).

---

## Dialogue Lines Explained

Each dialogue line is a JSON object in the `lines` array. Let's start simple and build up.

### Level 1: Basic Dialogue (Customer Says Something)

```json
{
  "editorIndex": 10,
  "speaker": "Customer:",
  "text": "Hey there! Nice weather today, huh?",
  "responses": [],
  "displayDuration": 1.5,
  "autoAdvanceToNext": true
}
```

**What This Does:**
- Customer says: "Hey there! Nice weather today, huh?"
- Text types out over 1.5 seconds (typewriter effect)
- Automatically continues to next line (no Continue button)

**Field Breakdown:**
- `editorIndex`: **Unique ID** for this dialogue line (use multiples of 10: 10, 20, 30, 40...)
- `speaker`: Who's talking (`"Customer:"` or `"You:"`)
- `text`: What they say (can be multiple sentences)
- `responses`: Player choices (empty `[]` means no choices)
- `displayDuration`: Animation time in seconds (1.5 = fast, 3.0 = slow)
- `autoAdvanceToNext`: `true` = skip Continue button, `false` = show Continue button

**When to use `autoAdvanceToNext: true`:**
- Short lines that flow naturally
- Building tension with rapid dialogue
- NPC talking to themselves

**When to use `autoAdvanceToNext: false`:**
- Important information player should read
- Before showing player choices
- Before ID scanning or item purchasing

### Level 2: Player Response (Choices!)

```json
{
  "editorIndex": 20,
  "speaker": "Customer:",
  "text": "Can I get a pack of cigarettes?",
  "responses": [
    {
      "responseText": "Sure, which brand?",
      "nextLineIndex": 30,
      "scoreValue": 0
    },
    {
      "responseText": "I need to see your ID first.",
      "nextLineIndex": 40,
      "scoreValue": 0
    },
    {
      "responseText": "Here you go. (Skip ID check)",
      "nextLineIndex": 50,
      "scoreValue": 10
    }
  ],
  "displayDuration": 2.0,
  "autoAdvanceToNext": false
}
```

**What This Does:**
- Customer asks for cigarettes
- Player sees **3 button choices**
- Clicking a button jumps to different dialogue lines
- Third choice gives +10 corruption points (bad decision!)

**Response Fields:**
- `responseText`: Button text (keep it short! 2-7 words ideal)
- `nextLineIndex`: Which `editorIndex` to jump to next
- `scoreValue`: Corruption points to add (0 = ethical, 5+ = unethical)

**Design Tip:** Always give players an ethical choice (scoreValue: 0) and a tempting unethical shortcut (scoreValue: 10+).

### Level 3: ID Scanning

```json
{
  "editorIndex": 40,
  "speaker": "You:",
  "text": "Sure, let me see your ID.",
  "responses": [],
  "askForID": true,
  "grantNameAccess": true,
  "grantDOBAccess": true,
  "displayDuration": 1.0,
  "autoAdvanceToNext": false
}
```

**What This Does:**
- ID card spawns on screen
- Player drags ID to scanner
- ID panel shows Name and Date of Birth (other fields blocked)
- Continue button appears after scanning

**ID Access Fields (all default to `false`):**
- `askForID`: `true` = spawn the ID card
- `grantNameAccess`: `true` = show customer's name
- `grantDOBAccess`: `true` = show date of birth
- `grantAddressAccess`: `true` = show address
- `grantIssuerAccess`: `true` = show ID issuer
- `grantPictureAccess`: `true` = show profile picture

**Progressive Access Pattern:**
```json
// First ID request: Only show name
{ "askForID": true, "grantNameAccess": true }

// Second ID request: Also show age
{ "grantDOBAccess": true }

// Third ID request: Show everything
{ "grantAddressAccess": true, "grantIssuerAccess": true, "grantPictureAccess": true }
```

**Why Progressive Access?**
This creates player agency - they choose how much personal data to request vs. trusting the customer. More data = safer verification, but feels invasive. Less data = faster, but risky.

### Level 4: Item Purchase

```json
{
  "editorIndex": 100,
  "speaker": "You:",
  "text": "Would you like to complete the purchase?",
  "responses": [
    {
      "responseText": "Make Sale",
      "nextLineIndex": 110,
      "isMakeSaleResponse": true,
      "scoreValue": 0
    },
    {
      "responseText": "Actually, I can't sell this to you.",
      "nextLineIndex": 120,
      "scoreValue": 0
    }
  ],
  "displayDuration": 1.5,
  "autoAdvanceToNext": false
}
```

**What This Does:**
- "Make Sale" button validates items in checkout
- If correct items: Sale completes, money added, dialogue continues
- If wrong items: Error sound, dialogue shows rejection path

**Make Sale Response:**
- `isMakeSaleResponse`: `true` = check if items match `requestedItems`
- `nextLineIndex`: Where to go **if sale succeeds**
- If sale fails: Dialogue stays on current line (player tries again)

**Pro Tip:** Always give players a way to refuse the sale (`"I can't sell this to you"`) for ethical playthroughs.

### Level 5: Ending the Scenario

```json
{
  "editorIndex": 110,
  "speaker": "Customer:",
  "text": "Thanks! Have a great day!",
  "responses": [],
  "endConversationHere": true,
  "scoreScreenIndex": 2,
  "displayDuration": 1.5,
  "autoAdvanceToNext": false
}
```

**What This Does:**
- Customer says goodbye
- Conversation ends
- Score screen #2 appears (Good Outcome)
- "Continue" button advances to next customer

**Ending Fields:**
- `endConversationHere`: `true` = end this scenario
- `scoreScreenIndex`: Which outcome screen to show (0-8, see below)

**Score Screen Index Guide:**
```
0-2 = Robin Banks outcomes (Awful, Bad, Good)
3-5 = Sussy Amon outcomes (Awful, Bad, Good)
6   = Generic good outcome
7   = Generic bad outcome
8   = Special outcome
```

**Design Pattern:** Create multiple ending lines with different `scoreScreenIndex` values based on player choices.

---

## Response Choices

Responses are the heart of branching dialogue. Here's everything you can do with them:

### Basic Response

```json
{
  "responseText": "Tell me more.",
  "nextLineIndex": 50,
  "scoreValue": 0
}
```

### Response with Corruption

```json
{
  "responseText": "I'll skip the ID check for €20.",
  "nextLineIndex": 60,
  "scoreValue": 15,
  "shadyMoneyReward": 20.0
}
```
- `scoreValue: 15` = +15 corruption points
- `shadyMoneyReward: 20.0` = Player gets €20 (from bribe)

### Response with Return Stack (Side Story)

```json
{
  "responseText": "Can you tell me your story?",
  "nextLineIndex": 200,
  "returnAfterResponse": true
}
```
- `returnAfterResponse: true` = Player can return to this point later
- Used for **optional lore** that doesn't affect main story
- Add a "Go Back" button on the side story lines (see Advanced Features)

### Make Sale Response

```json
{
  "responseText": "Make Sale",
  "nextLineIndex": 100,
  "isMakeSaleResponse": true,
  "scoreValue": 0
}
```
- Validates items in checkout match `requestedItems`

### Response That Shows Continue Button

```json
{
  "responseText": "Okay.",
  "nextLineIndex": 70,
  "activateContinueAfterChoice": true
}
```
- `activateContinueAfterChoice: true` = Show Continue button after clicking response
- Useful for confirmations or acknowledgments

---

## Common Patterns

### Pattern 1: Age Verification (Classic Scenario)

```json
{
  "editorIndex": 10,
  "speaker": "Customer:",
  "text": "Hey, can I get some beer?",
  "responses": [
    {
      "responseText": "Sure! Let me see your ID first.",
      "nextLineIndex": 20
    },
    {
      "responseText": "Here you go. (Skip ID)",
      "nextLineIndex": 100,
      "scoreValue": 15
    }
  ]
},
{
  "editorIndex": 20,
  "speaker": "You:",
  "text": "Can I see your ID please?",
  "askForID": true,
  "grantNameAccess": true,
  "grantDOBAccess": true,
  "responses": []
},
{
  "editorIndex": 30,
  "speaker": "You:",
  "text": "Hmm, this ID says you're underage.",
  "responses": [
    {
      "responseText": "Sorry, I can't sell to you.",
      "nextLineIndex": 40,
      "scoreValue": 0
    },
    {
      "responseText": "I'll make an exception... (Sell anyway)",
      "nextLineIndex": 50,
      "scoreValue": 25
    }
  ]
}
```

**Flow:** Request beer → Check ID → Discover problem → Refuse or accept bribe

### Pattern 2: Bribery Offer

```json
{
  "editorIndex": 60,
  "speaker": "Customer:",
  "text": "Come on, I'll give you €50 if you just sell it to me.",
  "responses": [
    {
      "responseText": "Accept bribe (€50)",
      "nextLineIndex": 70,
      "scoreValue": 30,
      "shadyMoneyReward": 50.0
    },
    {
      "responseText": "No. I'm not risking my job.",
      "nextLineIndex": 80,
      "scoreValue": 0
    }
  ]
}
```

**Bribe Pattern:** Always make bribes worth it (high shadyMoney) but dangerous (high scoreValue).

### Pattern 3: Information Gathering (No Choices)

```json
{
  "editorIndex": 100,
  "speaker": "Customer:",
  "text": "I used to work at a place like this.",
  "displayDuration": 2.0,
  "autoAdvanceToNext": true
},
{
  "editorIndex": 110,
  "speaker": "Customer:",
  "text": "The hours were terrible, but the people were nice.",
  "displayDuration": 2.0,
  "autoAdvanceToNext": true
},
{
  "editorIndex": 120,
  "speaker": "Customer:",
  "text": "Anyway, that's why I appreciate folks like you.",
  "displayDuration": 2.0,
  "autoAdvanceToNext": false
}
```

**Storytelling Pattern:** Auto-advance through backstory, then show Continue button at the end.

### Pattern 4: Multi-Stage ID Verification

```json
{
  "editorIndex": 10,
  "speaker": "You:",
  "text": "I need to verify a few things. What's your name?",
  "askForID": true,
  "grantNameAccess": true,
  "responses": []
},
{
  "editorIndex": 20,
  "speaker": "You:",
  "text": "And you're over 18, correct?",
  "grantDOBAccess": true,
  "responses": []
},
{
  "editorIndex": 30,
  "speaker": "You:",
  "text": "Finally, I need your full address for records.",
  "grantAddressAccess": true,
  "grantIssuerAccess": true,
  "responses": []
}
```

**Progressive Reveal:** Each line grants more ID access. Makes verification feel thorough.

### Pattern 5: Ethical Dilemma

```json
{
  "editorIndex": 50,
  "speaker": "Customer:",
  "text": "Please, my kid is sick and I need this medicine. I left my ID at home.",
  "responses": [
    {
      "responseText": "I understand, but I need to follow protocol.",
      "nextLineIndex": 60,
      "scoreValue": 0
    },
    {
      "responseText": "Just this once. (Sell without ID)",
      "nextLineIndex": 70,
      "scoreValue": 12
    },
    {
      "responseText": "Can you go get your ID and come back?",
      "nextLineIndex": 80,
      "scoreValue": 0
    }
  ]
}
```

**Dilemma Pattern:** Sympathetic situation + rule-breaking option + compromise option.

---

## Advanced Features

### NPC Sprite Changes (Emotions)

```json
{
  "editorIndex": 40,
  "speaker": "Customer:",
  "text": "WHAT?! That's ridiculous!",
  "npcSprite": "Angry",
  "responses": []
}
```

**Available Sprites:** `"Normal"`, `"Angry"`, `"Happy"`, `"Sad"` (check your NPC prefab for available sprites)

**Use Cases:**
- Customer gets angry when refused
- Customer gets happy when sale completes
- Adds visual variety to long scenarios

### Business Card Display

```json
{
  "editorIndex": 100,
  "speaker": "Customer:",
  "text": "Here's my card if you change your mind.",
  "showBusinessCard": true,
  "businessCardImagePath": "Cards/ShadyCard",
  "waitForCardDismissal": true,
  "responses": []
}
```

**What This Does:**
- Overlay appears with business card image
- Player clicks card to dismiss
- Dialogue continues after dismissal

**Use Cases:**
- Shady character offers illegal services
- Police officer gives contact info
- Customer complaints department

### Go Back Button (Return to Main Path)

```json
{
  "editorIndex": 200,
  "speaker": "Customer:",
  "text": "(End of side story. That's all I wanted to share.)",
  "showGoBackButton": true,
  "goBackTargetIndex": 50,
  "disableContinueButton": true,
  "responses": []
}
```

**Fields:**
- `showGoBackButton: true` = Show "Go Back" button
- `goBackTargetIndex: 50` = Jump back to line with editorIndex 50
- `disableContinueButton: true` = Hide Continue button (forces "Go Back")

**Use With:** `returnAfterResponse: true` on the response that led to this side story.

### Voice-Over Support (Future Feature)

```json
{
  "editorIndex": 10,
  "speaker": "Customer:",
  "text": "Hello there!",
  "voiceOverPath": "VoiceOvers/CustomerGreeting",
  "responses": []
}
```

**Status:** Code is ready, but feature is currently disabled. Audio assets need to be added to `Resources/VoiceOvers/`.

**When Enabled:** Audio will play synchronized with typewriter text.

### Authorization System (Special IDs)

Used for special verification (government IDs, authorization tokens, etc.). See `Fridgy_Scenario.json` for a working example.

**Setup:**
1. Your ID prefab needs `AuthorizationID` component (ask tech lead to set up)
2. Use normal ID spawning in dialogue
3. System shows loading bar animation (5 seconds)
4. Checkmark appears when authorized
5. Dialogue continues

**Example:**
```json
{
  "editorIndex": 50,
  "speaker": "You:",
  "text": "I need to verify this authorization token...",
  "askForID": true,
  "grantNameAccess": true,
  "responses": []
}
```

If the ID has `AuthorizationID` component, the loading animation plays automatically.

---

## Full Example Scenario

Here's a complete scenario from start to finish. Copy this template to create your own!

```json
{
  "customerName": "Jamie Dodger",
  "npcPrefabPath": "NPC_Prefabs/JamieDodger",
  "idPrefabPath": "ID_Prefabs/JamieDodgerID",
  "idName": "Jamie Dodger",
  "idDOB": "22 years old",
  "idAddress": "123 Main Street, Neo City",
  "idIssuer": "NEU ID",
  "requestedItems": [11],
  "lines": [
    {
      "editorIndex": 10,
      "speaker": "Customer:",
      "text": "Hey! How's it going?",
      "responses": [],
      "displayDuration": 1.5,
      "autoAdvanceToNext": true,
      "npcSprite": "Normal"
    },
    {
      "editorIndex": 20,
      "speaker": "Customer:",
      "text": "Can I get a pack of HotShot cigarettes?",
      "responses": [
        {
          "responseText": "Sure, let me see your ID first.",
          "nextLineIndex": 30,
          "scoreValue": 0
        },
        {
          "responseText": "No problem! (Skip ID check)",
          "nextLineIndex": 100,
          "scoreValue": 8
        }
      ],
      "displayDuration": 2.0,
      "autoAdvanceToNext": false,
      "npcSprite": "Normal"
    },
    {
      "editorIndex": 30,
      "speaker": "You:",
      "text": "Sure, let me verify your age.",
      "responses": [],
      "askForID": true,
      "grantNameAccess": true,
      "grantDOBAccess": true,
      "displayDuration": 1.5,
      "autoAdvanceToNext": false
    },
    {
      "editorIndex": 40,
      "speaker": "You:",
      "text": "Okay, you're good. That'll be €15.",
      "responses": [],
      "displayDuration": 1.5,
      "autoAdvanceToNext": true
    },
    {
      "editorIndex": 50,
      "speaker": "You:",
      "text": "Would you like to complete your purchase?",
      "responses": [
        {
          "responseText": "Make Sale",
          "nextLineIndex": 60,
          "isMakeSaleResponse": true,
          "scoreValue": 0
        }
      ],
      "displayDuration": 1.5,
      "autoAdvanceToNext": false
    },
    {
      "editorIndex": 60,
      "speaker": "Customer:",
      "text": "Thanks! Have a good one!",
      "responses": [],
      "endConversationHere": true,
      "scoreScreenIndex": 6,
      "displayDuration": 1.5,
      "autoAdvanceToNext": false,
      "npcSprite": "Happy"
    },
    {
      "editorIndex": 100,
      "speaker": "You:",
      "text": "Here you go! That'll be €15.",
      "responses": [],
      "displayDuration": 1.5,
      "autoAdvanceToNext": true
    },
    {
      "editorIndex": 110,
      "speaker": "You:",
      "text": "Would you like to complete your purchase?",
      "responses": [
        {
          "responseText": "Make Sale",
          "nextLineIndex": 120,
          "isMakeSaleResponse": true,
          "scoreValue": 0
        }
      ],
      "displayDuration": 1.5,
      "autoAdvanceToNext": false
    },
    {
      "editorIndex": 120,
      "speaker": "Customer:",
      "text": "Thanks! You're the best!",
      "responses": [],
      "endConversationHere": true,
      "scoreScreenIndex": 7,
      "displayDuration": 1.5,
      "autoAdvanceToNext": false,
      "npcSprite": "Happy"
    }
  ]
}
```

**What This Scenario Does:**
1. Jamie asks for cigarettes
2. **Branch A (Ethical):** Check ID → Verify age → Make sale → Good outcome (screen 6)
3. **Branch B (Shortcut):** Skip ID check → Make sale → Bad outcome (screen 7) + 8 corruption points

**Your Turn:** Modify this template to create your own scenario!

---

## Troubleshooting

### "Scenario doesn't load in game"
**Check:**
- File is in `ProjectKiosk/Assets/Resources/` folder
- Filename ends with `_Scenario.json`
- File is added to `ScenarioConfig.json` scenario list
- JSON syntax is valid (no missing commas, brackets)

**Test:** Paste your JSON into [jsonlint.com](https://jsonlint.com) to validate syntax.

### "Dialogue jumps to wrong line"
**Check:**
- `nextLineIndex` values match actual `editorIndex` values
- No duplicate `editorIndex` numbers
- `editorIndex` values are unique across all lines

**Pro Tip:** Use multiples of 10 for editorIndex (10, 20, 30...) to leave room for inserting lines later.

### "ID doesn't spawn"
**Check:**
- `askForID: true` is present
- `idPrefabPath` points to valid prefab in `Resources/ID_Prefabs/`
- At least one `grant*Access` field is `true`

### "Make Sale button doesn't work"
**Check:**
- `isMakeSaleResponse: true` is set on response
- `requestedItems` array at top of JSON contains correct item IDs
- Player put correct items in checkout area (check item IDs!)

**Debug:** Open `ItemPickupManager.cs` and check console logs during sale.

### "Wrong score screen appears"
**Check:**
- `scoreScreenIndex` value is 0-8
- Value matches intended outcome (see Score Screen Index Guide above)
- `endConversationHere: true` is present on ending line

### "Text types too slowly/quickly"
**Fix:**
- Adjust `displayDuration` value
- Short lines: 1.0-1.5 seconds
- Medium lines: 1.5-2.5 seconds
- Long lines: 2.5-4.0 seconds

### "Player can't go back in dialogue"
**Fix:**
- Add `showGoBackButton: true` on target line
- Set `goBackTargetIndex` to correct editorIndex
- Set `disableContinueButton: true` to force using Go Back
- Ensure original response had `returnAfterResponse: true`

---

## Pro Tips for Great Scenarios

### Writing Dialogue
- **Keep lines short:** 1-2 sentences per line
- **Use personality:** Give customers unique voices
- **Build tension:** Start friendly, escalate gradually
- **End with impact:** Last line should feel satisfying/regretful

### Designing Choices
- **Always include an ethical option:** Let players be good
- **Make corruption tempting:** High rewards, clear shortcuts
- **Add 3-4 choices max:** Too many = decision paralysis
- **Use descriptive text:** Not just "Yes/No" - explain the action

### Balancing Corruption
- **Minor shortcuts:** 5-10 points (skip partial check)
- **Moderate violations:** 10-20 points (skip full verification)
- **Major crimes:** 20-30 points (accept bribe, sell to minors)
- **Severe corruption:** 30+ points (data selling, document forgery)

**Remember:** At 50 points, police warning triggers. At 70 points, player gets arrested!

### Testing Your Scenario
1. Play through **all branches** - don't assume paths work without testing
2. Check **every ending** reaches a valid conclusion
3. Verify **item IDs** match what you request
4. Test with **both quick and slow reading speeds**
5. Try **skipping typewriter** (spacebar) to ensure no bugs

### EditorIndex Numbering Strategy
Use this pattern for organized scenarios:

```
10-90:   Intro conversation
100-190: First branch (ethical path)
200-290: Second branch (shortcut path)
300-390: Third branch (bribery path)
400-490: Side story/optional lore
500-590: Endings (multiple outcomes)
```

Gaps between ranges let you insert lines later without renumbering everything!

---

## Quick Reference Card

### Minimal Required Fields
```json
{
  "editorIndex": 10,
  "speaker": "Customer:",
  "text": "Hello!",
  "responses": []
}
```

### Common Field Defaults
```json
{
  "displayDuration": 2.0,
  "autoAdvanceToNext": false,
  "askForID": false,
  "endConversationHere": false,
  "showGoBackButton": false,
  "showBusinessCard": false,
  "npcSprite": "Normal"
}
```

### Copy-Paste Templates

**Simple Line:**
```json
{ "editorIndex": 10, "speaker": "Customer:", "text": "", "responses": [], "displayDuration": 2.0, "autoAdvanceToNext": false }
```

**Choice Line:**
```json
{ "editorIndex": 20, "speaker": "Customer:", "text": "", "responses": [{"responseText": "", "nextLineIndex": 30, "scoreValue": 0}], "displayDuration": 2.0 }
```

**ID Check Line:**
```json
{ "editorIndex": 30, "speaker": "You:", "text": "Let me see your ID.", "responses": [], "askForID": true, "grantNameAccess": true, "grantDOBAccess": true }
```

**Make Sale Line:**
```json
{ "editorIndex": 40, "speaker": "You:", "text": "Ready to purchase?", "responses": [{"responseText": "Make Sale", "nextLineIndex": 50, "isMakeSaleResponse": true}] }
```

**Ending Line:**
```json
{ "editorIndex": 50, "speaker": "Customer:", "text": "Thanks!", "responses": [], "endConversationHere": true, "scoreScreenIndex": 6 }
```

---

## Next Steps

You're ready to create scenarios! Here's what to do:

1. **Copy the Full Example Scenario** from above
2. **Rename the customer** and update paths to your prefabs
3. **Write 5-10 dialogue lines** with at least 2 branching paths
4. **Test in Unity** - play through all paths
5. **Iterate** based on playtesting feedback
6. **Show your work** to the team for review

**Need Help?**
- Check existing scenarios: `Robin_Scenario.json`, `Amon_Scenario.json`, `Fridgy_Scenario.json`
- Ask tech lead about creating new prefabs (NPCs, IDs)
- Review `PROJECT_DOCUMENTATION.md` for system details
- Test score triggers with keyboard shortcuts (1-5 keys in Gameplay scene)

**Good luck, and have fun creating memorable characters!**

---

*Last Updated: [Current Date]*
*Game Version: 1.0*
*For questions, contact the development team*