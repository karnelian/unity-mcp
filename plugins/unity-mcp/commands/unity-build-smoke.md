---
description: Plan or run a minimal Unity build smoke check after health/compile validation
argument-hint: [target platform]
---

Prepare a build smoke check for the current Unity project. Use `$ARGUMENTS` as target platform if provided.

Do the following:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first.
3. Run `unity_mcp_safety_manifest` and inspect `unity_editor_build` plus any build-target/settings tools that may be needed.
4. If compile errors exist, stop and recommend `/unity-compile-fix` first.
5. Check build target, scenes in build, packages, render pipeline, and platform-specific obvious blockers.
6. If the user explicitly requested running a build, first call `unity_editor_build` with `dryRun: true` using the intended target/output. Summarize the dry-run risk/result before executing the real build.
7. If build-target switching or project-setting changes are required, do not perform them by default; explain that they are high-risk and require explicit user approval and, when configured, the manifest/describe `confirmationToken`.
8. Report expected output path, target, blockers, and follow-up checks.

Rules:

- Do not switch build target or start a long build unless the user explicitly asked to run it.
- Prefer a plan/checklist over expensive build execution by default.
- For mobile/Web/Quest, note that Editor success is not platform success.
