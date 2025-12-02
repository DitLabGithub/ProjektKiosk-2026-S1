using System;
using System.Collections.Generic;
using UnityEngine;

// NOTE: This assumes the ItemCategory enum (from ItemScript.cs) is accessible.

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

[Serializable]
public class DialogueLineData
{
    public int editorIndex;
    public string speaker;
    [TextArea(2, 5)]
    public string text;
    public List<DialogueResponseData> responses = new List<DialogueResponseData>();

    // ID Interaction Flags
    public bool askForID = false;
    public bool showGoBackButton = false;
    public int goBackTargetIndex = -1;

    // Data Access Granting
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
    public bool showBusinessCard = false; // If true, spawn business card image
    public string businessCardImagePath = ""; // Path to card image in Resources (e.g., "Cards/ShadyCard")
    public bool waitForCardDismissal = false; // If true, wait for player to click card before ending/continuing

    // NPC Sprite Change System
    public string npcSprite = ""; // Path to sprite in Resources/NPC_Sprites (e.g., "ShaunBaker_Angry")

    // Voice-Over System
    public string voiceOverPath = ""; // Path to load voice-over from Resources folder (e.g., "VoiceOvers/ShaunBaker_Line1")

    // Conversation End
    public bool endConversationHere = false;
    public int scoreScreenIndex = 0;
}

[Serializable]
public class CustomerScenarioData
{
    // Customer Identification and Prefab Loading
    public string customerName;
    public string npcPrefabPath; // Path in Resources to the customer's visual prefab (e.g., "NPC_Prefabs/Customer4")

    // ID Data for the ID that spawns
    public string idPrefabPath; // Path in Resources to the ID prefab (e.g., "ID_Prefabs/StandardID")

    // The actual data on the ID card
    public string idName;
    public string idDOB;
    public string idAddress;
    public string idIssuer;
    public bool idAllowNameAccess = false;
    public bool idAllowDOBAccess = false;
    public bool idAllowAddressAccess = false;
    public bool idAllowIssuerAccess = false;
    public bool idAllowPictureAccess = false;

    // Scenario Data
    public List<ItemCategory> requestedItems = new List<ItemCategory>();
    public List<DialogueLineData> lines = new List<DialogueLineData>();
}
