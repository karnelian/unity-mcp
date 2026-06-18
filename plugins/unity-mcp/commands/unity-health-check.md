---
description: Diagnose Unity project health through Unity MCP without making changes
argument-hint: [focus]
---

Run a read-only Unity project health check through Karnelian Unity MCP.

Use `$ARGUMENTS` as an optional focus area if provided, for example `2d`, `ui`, `android`, `ios`, `xr`, `rendering`, or `packages`.

Do the following:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first.
3. From the health result, summarize:
   - Unity Editor version
   - build target
   - render pipeline
   - compile status
   - recent Console errors and warnings
   - current scene and hierarchy summary
   - installed packages and notable package/version risks
   - missing scripts, missing references, shader errors, or validation issues
4. If `unity_project_health` indicates compile errors, use the compile status and Console information to identify the top failing files, but do not edit anything.
5. If `$ARGUMENTS` names a focus area, add a short focused risk section for that area.
6. End with a prioritized action list: `fix now`, `check next`, and `safe to ignore/monitor`.

Rules:

- This command is read-only. Do not write files, modify scenes, change assets, enter Play Mode, or run broad cleanup.
- Prefer compact summaries and pagination over full dumps.
- State which MCP tool results support each major finding.
