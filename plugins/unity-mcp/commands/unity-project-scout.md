---
description: Read-only scout pass for an existing Unity project before implementation
argument-hint: [focus]
---

Scout the Unity project before making changes. Use `$ARGUMENTS` as an optional focus such as `2d`, `ui`, `mobile`, `xr`, `netcode`, `addressables`, `performance`, or a feature name.

Do the following, read-only:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first.
3. Inspect project stack and conventions using the smallest relevant MCP queries:
   - Unity version, build target, render pipeline
   - packages and package versions
   - input system route
   - UI route: uGUI, UI Toolkit, TextMeshPro, mixed
   - 2D/3D/XR indicators
   - asmdef presence and runtime/editor/test split
   - major folders under `Assets/`
   - scene list and active scene
   - script namespaces and coding patterns when available
4. If `$ARGUMENTS` names a focus, add a focused subsection for that domain.
5. Report the recommended working strategy for this project.

Rules:

- Read-only only. Do not write scripts, scenes, assets, packages, or project settings.
- Do not scan all assets unless needed; prefer summaries, limits, and package/folder-level inspection.
- End with: `project map`, `risks`, `recommended next command`, and `implementation guardrails`.
