---
name: unity-mobile-guide
description: "Android, iOS, and Quest readiness checks for Unity projects."
---

# Unity Mobile

Use for Android, iOS, Quest, or general mobile readiness.

## Checks

- Build target and platform settings.
- IL2CPP/architecture/stripping implications.
- URP/mobile render settings, MSAA, shadows, post-processing.
- Input route and safe-area/aspect handling.
- Android Gradle/manifest/custom templates when present.
- iOS networking/signing/orientation risks when visible.
- Quest XR plugin and XR Interaction Toolkit status.

Do not switch build targets automatically; it can trigger long imports.
