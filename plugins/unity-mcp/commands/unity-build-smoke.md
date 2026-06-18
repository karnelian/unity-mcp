---
description: Plan or run a minimal Unity build smoke check after health/compile validation
argument-hint: [target platform]
---

Prepare a build smoke check for the current Unity project. Use `$ARGUMENTS` as target platform if provided.

Do the following:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first.
3. If compile errors exist, stop and recommend `/unity-compile-fix` first.
4. Check build target, scenes in build, packages, render pipeline, and platform-specific obvious blockers.
5. If a build tool is available and the user explicitly requested running the build, run the smallest safe smoke build. Otherwise, only produce a build smoke plan.
6. Report expected output path, target, blockers, and follow-up checks.

Rules:

- Do not switch build target or start a long build unless the user explicitly asked to run it.
- Prefer a plan/checklist over expensive build execution by default.
- For mobile/Web/Quest, note that Editor success is not platform success.
