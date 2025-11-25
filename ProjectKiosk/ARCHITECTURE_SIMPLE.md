# Project Kiosk - Simple Architecture Overview

A clean, high-level view of the game's architecture for quick reference.

---

## Main System Architecture

```mermaid
graph TB
    subgraph "Core Systems"
        DM[DialogueManager<br/>Manages all customer interactions]
        SM[ScenarioManager<br/>Loads and queues scenarios]
        UM[UIManager<br/>Controls UI elements]
    end

    subgraph "ID Verification System"
        IDS[IDScanner<br/>Drag-drop ID scanning]
        CID[CustomerID<br/>ID data storage]
        AID[AuthorizationID<br/>Special auth IDs]
        INFO[IDInfoDisplay<br/>Shows ID to player]
    end

    subgraph "Item & Sales System"
        IPM[ItemPickupManager<br/>Pickup and checkout]
        SALE[SaleManager<br/>Transaction processing]
        ITEM[ItemScript<br/>Item data]
    end

    subgraph "Audio System"
        SOUND[SoundManager<br/>Global audio control]
        VO[Voice-Over Audio<br/>Dialogue audio]
    end

    subgraph "Data Sources"
        JSON[JSON Scenarios<br/>Customer data files]
        CONFIG[ScenarioConfig<br/>Scenario queue setup]
    end

    %% Main Flow
    JSON --> SM
    CONFIG --> SM
    SM --> DM
    DM --> UM

    %% ID Flow
    DM --> IDS
    IDS --> CID
    CID --> AID
    CID --> INFO

    %% Item Flow
    DM --> IPM
    IPM --> SALE
    IPM --> ITEM

    %% Audio Flow
    DM --> SOUND
    DM --> VO

    style DM fill:#ff6b6b,color:#fff,stroke:#333,stroke-width:3px
    style SM fill:#4a90e2,color:#fff,stroke:#333,stroke-width:3px
    style JSON fill:#50c878,color:#fff,stroke:#333,stroke-width:2px
    style SOUND fill:#ffd93d,stroke:#333,stroke-width:2px
    style IPM fill:#9b59b6,color:#fff,stroke:#333,stroke-width:2px
```

---

## Class Organization by Team

```mermaid
mindmap
  root((Project Kiosk))
    Garon's Work
      DialogueManager
      ID System
        CustomerID
        IDScanner
        IDInfoDisplay
      UI Components
        UIManager
        CustomerUIHandler
      Effects
        TypeWriter
        FadeController
    Saab's Work
      ScenarioManager
      Authorization System
        AuthorizationID
        AuthorizationUIManager
      JSON Integration
        CustomerScenarioDialogue
    Samy's Work
      SoundManager
      Item System
        ItemPickupManager
        ItemScript
        SaleManager
        DraggableItem
      Camera Effects
        CameraFade
        ItemHover
```

---

## Data Flow - Simplified

```mermaid
flowchart LR
    A[JSON Files] -->|Load| B[ScenarioManager]
    B -->|Next Scenario| C[DialogueManager]
    C -->|Show Text| D[Player]
    D -->|Make Choice| C
    C -->|Request ID| E[ID System]
    E -->|Verify| C
    C -->|Validate Sale| F[Item System]
    F -->|Result| C
    C -->|Complete| G[Score Screen]
    G -->|Continue| B

    style A fill:#50c878,color:#fff
    style C fill:#ff6b6b,color:#fff
    style G fill:#ffd93d
```

---

## Key Components Summary

| Component | Type | Purpose | Team Member |
|-----------|------|---------|-------------|
| **DialogueManager** | MonoBehaviour | Core dialogue and customer interaction manager | Garon |
| **ScenarioManager** | MonoBehaviour | Loads and queues customer scenarios | Saab |
| **CustomerID** | MonoBehaviour | Stores and manages ID card data | Garon |
| **AuthorizationID** | MonoBehaviour (inherits CustomerID) | Special ID with authorization check | Saab |
| **ItemPickupManager** | MonoBehaviour | Handles item pickup and checkout zone | Samy |
| **SoundManager** | Singleton | Global audio management | Samy |
| **IDScanner** | MonoBehaviour | Drag-and-drop ID scanning mechanics | Garon |
| **UIManager** | MonoBehaviour | Controls UI elements and panels | Garon |

---

## System Interactions - Quick Reference

```mermaid
sequenceDiagram
    actor Player
    participant SM as ScenarioManager
    participant DM as DialogueManager
    participant ID as ID System
    participant Items as Item System
    participant UI as UI/Audio

    SM->>DM: Load Next Customer
    DM->>UI: Show Dialogue
    UI->>Player: Display Text + Choices
    Player->>DM: Select Choice
    DM->>ID: Request ID Verification
    ID->>Player: Show ID
    Player->>ID: Scan ID
    ID->>DM: ID Verified
    DM->>Items: Validate Items
    Items->>DM: Items OK
    DM->>UI: Show Score Screen
    UI->>Player: Score + Continue
    Player->>SM: Next Customer
```

---

## Color Legend

- 🔴 **Red (DialogueManager)** - Core system, central to all interactions
- 🔵 **Blue (ScenarioManager)** - Scenario loading and management
- 🟢 **Green (Data/JSON)** - Data sources and configuration
- 🟡 **Yellow (Audio)** - Sound and voice-over systems
- 🟣 **Purple (Items)** - Item and sales systems

---

**Quick View:** This simplified architecture shows the main components without implementation details.
**Full Details:** See `PROJECT_DIAGRAMS.md` for complete system diagrams.
**Documentation:** See `PROJECT_DOCUMENTATION.md` for comprehensive text documentation.

**Created:** 2025-11-25
**Version:** 1.0
