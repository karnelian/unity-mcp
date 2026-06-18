---
name: unity-performance
description: "Unity performance review guardrails for rendering, assets, UI, physics, scripts, and target-platform readiness."
---

# Unity Performance

Use before release/build checks or when the user reports frame drops.

## Priorities

1. Measure/inspect before changing settings.
2. Separate Editor-only symptoms from player/runtime risks.
3. For mobile/Quest, prefer URP/mobile-friendly settings and asset compression checks.
4. Look for high-impact risks: shadows/lights, post-processing, large textures, duplicate materials, mesh counts, UI canvas rebuilds, physics hotspots, per-frame allocations.
5. Propose staged fixes with validation, not broad cleanup.
