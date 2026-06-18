---
description: Capture Console, Hierarchy, Inspector, Game, and Scene views for Unity visual QA
argument-hint: [focus]
---

Run a visual QA pass through Karnelian Unity MCP.

Use `$ARGUMENTS` as an optional focus if provided, for example `ui`, `camera`, `lighting`, `2d`, `scene layout`, `mobile aspect`, or a GameObject name.

Do the following:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_debug_visualQaBundle` to capture or inspect, in one bundle:
   - Console
   - Hierarchy
   - selected Inspector
   - Game View
   - Scene View
3. If no object is selected and the focus requires Inspector review, select the most relevant object first only if it is obvious from `$ARGUMENTS` or the current scene; otherwise report that Inspector review is limited.
4. Analyze the bundle for:
   - Console errors/warnings
   - missing references or disabled/broken components in Inspector
   - abnormal hierarchy structure, duplicate names, or missing expected objects
   - Game View issues: black screen, wrong camera, UI clipping, canvas scale, aspect ratio, sorting/order problems
   - Scene View issues: object placement, scale, lighting, camera framing, 2D sorting, gizmo-visible problems
5. Produce a QA report grouped as:
   - `blocking`
   - `visual issues`
   - `warnings/risks`
   - `suggested fixes`

Rules:

- This command is QA-first. Do not modify files, scenes, prefabs, project settings, or assets unless the user explicitly asks for fixes after the report.
- Prefer the single `unity_debug_visualQaBundle` call over repeated individual screenshot/capture calls.
- Mention which captured view supports each finding.
- If a finding is uncertain from the capture, label it as uncertain and suggest the smallest follow-up check.
