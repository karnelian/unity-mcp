---
name: unity-scene-contracts-guide
description: "Validate Unity scenes against playable-scene contracts: camera, EventSystem, UI roots, managers, spawn points, missing references."
---

# Unity Scene Contracts

Use when deciding whether a scene is playable or ready for QA.

## Contract Checks

- Main camera/camera stack is intentional.
- No duplicate AudioListeners.
- EventSystem exists for uGUI.
- Canvas/UIRoot or UIDocument/PanelSettings exists for UI scenes.
- Bootstrap/System/Manager roots match project convention.
- Player/spawn/gameplay roots exist when required.
- No missing scripts/references on critical objects.
- Hierarchy depth and duplicate names are not suspicious.

Always label assumptions from the requested game/scene type.
