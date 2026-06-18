---
description: Read-only Unity performance risk scan for scene, assets, rendering, scripts, and build readiness
argument-hint: [focus]
---

Run a read-only performance risk scan. Use `$ARGUMENTS` as an optional focus such as `mobile`, `2d`, `ui`, `rendering`, `memory`, `physics`, `scripts`, `assets`, or `build`.

Do the following:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first.
3. Use relevant profiler/optimization/project/scene/asset summary tools, preferring summary-only and limits.
4. Check likely performance risks:
   - render pipeline and quality settings mismatch for target
   - camera count, lights, shadows, post-processing, URP/HDRP settings
   - object counts, hierarchy depth, duplicate heavy components
   - large textures, import compression, sprite atlas risks
   - mesh/material counts and duplicate materials
   - physics collider/rigidbody hotspots
   - UI canvas rebuild risks and overdraw clues
   - scripts with high-risk `Update`/allocation patterns if cheaply discoverable
5. Report `high impact`, `medium`, `low`, and `measure first` findings.

Rules:

- Read-only. Do not change import settings, quality settings, scripts, assets, or scenes.
- Do not run expensive full-asset scans unless user asks; use top-N summaries.
- Include the recommended measurement/build smoke step for the target platform.
