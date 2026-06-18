---
description: Run or plan Unity EditMode/PlayMode tests after compile health checks
argument-hint: [editmode|playmode|test filter]
---

Run or plan Unity tests. Use `$ARGUMENTS` as mode/filter; default to inspect available tests first.

Do the following:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` or compile check first. If compile errors exist, stop and recommend `/unity-compile-fix`.
3. Run `unity_mcp_safety_manifest` and inspect `unity_editor_runTests`; treat test execution as medium-risk because it can enter PlayMode or mutate editor state.
4. Discover available EditMode/PlayMode tests if possible.
5. If the user requested a specific run, call `unity_editor_runTests` with `dryRun: true` for the smallest matching test set first, then run it for real only if the dry-run is scoped correctly.
6. Summarize passing/failing tests, failure messages, and next fix target.

Rules:

- Do not create tests unless explicitly asked.
- Prefer EditMode before PlayMode unless the user asked PlayMode.
- If no test runner MCP tool is available, provide the exact Unity Test Runner path/plan instead of pretending it ran.
