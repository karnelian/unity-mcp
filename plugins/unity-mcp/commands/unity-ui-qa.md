---
description: Focused Unity UI visual QA for uGUI, UI Toolkit, TextMeshPro, camera, and aspect issues
argument-hint: [ui focus]
---

Run focused UI QA through Unity MCP. Use `$ARGUMENTS` for focus such as `main menu`, `hud`, `mobile safe area`, `button`, `text`, `canvas`, or `ui toolkit`.

Do the following:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_debug_visualQaBundle` once.
3. Inspect UI-specific risks:
   - Canvas Scaler and reference resolution
   - EventSystem and input module
   - TextMeshPro missing fonts/materials
   - Button interactable/raycast target issues
   - layout groups/content size fitter overflow
   - UI Toolkit UIDocument/PanelSettings if present
   - safe area/aspect ratio/clipping issues in Game View
   - camera/canvas render mode mismatch
4. Report `blocking`, `visual`, `interaction`, `layout`, and `follow-up capture needed`.

Rules:

- QA only. Do not modify UI until the user approves the fix plan.
- Mention which captured view supports each finding.
