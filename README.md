# Pokemon Egg Enhancement Game

A mobile idle game built with Unity where players hatch, enhance, and evolve Generation I Pokemon.

## Gameplay

The core loop revolves around three actions:

- **Hatch** — Obtain Pokemon eggs and hatch them into instances
- **Enhance** — Level up Pokemon from level 1 to 15 with escalating costs and risk. Higher levels have lower success rates and a chance of destruction
- **Evolve** — Evolve Pokemon using level thresholds or specific items (stones, held items, etc.)

Failed enhancements at higher levels can destroy a Pokemon. Destroyed Pokemon can be revived using Max Revive items.

## Features

- **151 Pokemon** — Full Generation I roster with gender variants, shiny forms, and alternate forms
- **Enhancement System** — 15 enhancement levels with dynamic success/failure rates and gold costs scaling from 500 to 50,000,000
- **Evolution System** — Level-based and item-based evolution supporting 30+ evolution methods
- **Dual Inventory** — Separate bags for general items (enhancement, revival) and evolution items
- **Shop** — Purchase enhancement and evolution items with in-game gold
- **Pokedex** — Track seen and caught Pokemon with sprite previews and pagination
- **Save System** — Persistent save via Unity PlayerPrefs

## Project Structure

```
Assets/
└── 01.Script/
    ├── Dex/          # Pokedex UI and save manager
    ├── Enhance/      # Egg hatching, enhancement, revival logic
    ├── Game/         # Core managers (balance, sound, settings, tutorial)
    ├── Inventory/    # Item bag UI and inventory controllers
    ├── Pokemon/      # Pokemon data, enums, evolution, form utilities
    ├── Shop/         # Shop panels and item purchasing
    └── UI/           # Shared UI components and layout helpers
```

## Built With

- **Unity** (2D)
- **C#**
- Unity PlayerPrefs for persistence
- Unity Burst compiler
