---
description: Inspect Unity package stack, optional dependencies, API risk, and target-platform compatibility
argument-hint: [package or domain]
---

Analyze Unity package risks. Use `$ARGUMENTS` as package/domain focus such as `cinemachine`, `entities`, `addressables`, `input`, `urp`, `netcode`, `mobile`, or `unity 6`.

Do the following, read-only:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first.
3. Inspect package list, versions, render pipeline, build target, and compile/console status.
4. Identify:
   - missing optional packages for requested work
   - packages likely to affect compile/API compatibility
   - Unity 6 migration/API rename risks
   - platform-specific package risks
   - package changes that may trigger domain reload or long imports
5. Return `safe`, `watch`, `risky`, and `requires decision` sections.

Rules:

- Do not install/remove packages unless explicitly asked.
- If a package change is needed, propose exact package IDs and a rollback plan.
