---
name: unity-project-scout-guide
description: "Inspect an existing Unity project before implementation: stack, folders, packages, asmdefs, scenes, and conventions."
---

# Unity Project Scout

Use before implementing in an unfamiliar Unity project.

## Checklist

- Run `unity_project_health` first.
- Detect Unity version, build target, render pipeline, input system, UI route, 2D/3D/XR indicators.
- Inspect packages and asmdef structure before writing scripts.
- Read folder/scene/script conventions before proposing architecture.
- Prefer summaries and limits; avoid full asset scans.

## Output

Return `project map`, `risks`, `recommended workflow`, and `guardrails`.
