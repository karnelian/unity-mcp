---
description: Validate scene contracts such as camera, EventSystem, managers, UI roots, and missing references
argument-hint: [contract or focus]
---

Validate the active Unity scene against playable-scene contracts. Use `$ARGUMENTS` for a custom contract/focus, for example `2d platformer`, `ui`, `mobile`, `bootstrap`, `combat scene`, or `main menu`.

Do the following, read-only by default:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` and inspect active scene/hierarchy summary.
3. Check scene contract items:
   - exactly one intended Main Camera or a clear camera stack setup
   - AudioListener duplication
   - EventSystem for uGUI scenes
   - Canvas/UIRoot or UIDocument/PanelSettings for UI scenes
   - bootstrap/GameManager/Systems/Managers roots if the project convention suggests them
   - spawn points, player root, gameplay root, or custom contract objects from `$ARGUMENTS`
   - missing scripts/references and disabled critical components
   - dirty/unsaved scene status if available
   - duplicate names or very deep hierarchy hotspots
4. Separate findings into `blocking`, `contract gaps`, `warnings`, and `optional cleanup`.
5. Propose the minimal safe fix plan, but do not modify anything unless the user asks.

Rules:

- Read-only unless the user explicitly requests fixes.
- If the user requests fixes, run `unity_mcp_safety_manifest` first, then use `dryRun: true` for planned scene/component mutations before applying them.
- If fixing later, prefer workflow begin → small change → validation → compile/console check.
- For deletes, undo-session, broad scene changes, or project settings changes, stop for explicit approval and use the manifest/describe `confirmationToken` if confirmation is enabled.
- Do not invent required objects; label assumptions from `$ARGUMENTS`.
