---
description: Check Android, iOS, or Quest readiness for Unity projects
argument-hint: [android|ios|quest|mobile]
---

Check mobile/platform readiness. Interpret `$ARGUMENTS` as the target platform; default to `mobile` if omitted.

Do the following, read-only:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first.
3. Run `unity_mcp_safety_manifest` and inspect platform/project mutating tools (`unity_project_setBuildTarget`, `unity_project_setAndroidSettings`, `unity_project_setIOSSettings`, `unity_project_set_player_settings`) before recommending changes.
4. Inspect build target, render pipeline, quality settings, input route, package stack, and platform settings relevant to the target.
4. For Android/Quest, check:
   - Android build target, Gradle/JDK/AGP hints if exposed
   - min/target SDK, IL2CPP/Mono, ARM64, stripping
   - URP/mobile/Quest render settings, MSAA, shadows, post-processing
   - AndroidManifest/custom Gradle templates if present
   - XR plugin / XR Interaction Toolkit status for Quest
5. For iOS, check:
   - iOS build target, signing-related settings when visible
   - IL2CPP, stripping, architecture
   - UnityWebRequest/networking/package risks
   - orientation, safe area, resolution/UI scaling
6. Report `must fix before build`, `build-risk`, `runtime-risk`, and `nice to have`.

Rules:

- Read-only unless the user explicitly asks to apply settings.
- Do not switch build targets automatically; switching can trigger long imports.
- If changes are needed, propose a staged plan with dry-run/confirmation where possible.
