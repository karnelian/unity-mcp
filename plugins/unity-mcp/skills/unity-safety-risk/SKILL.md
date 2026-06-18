---
name: unity-safety-risk
description: "Safety model for AI-driven Unity changes: read-only first, batch-first, dry-run, confirmation, rollback, and audit-friendly reporting."
---

# Unity Safety & Risk

Use whenever a task may modify scenes, assets, project settings, packages, prefabs, or build targets.

## Risk Tiers

- Read-only: health, console, hierarchy, package info, screenshots.
- Low: script edits with compile check, no serialized API changes.
- Medium: scene/component/prefab edits, import settings, quality settings.
- High: deletes, package removal, build target switch, project settings rewrites, prefab apply/revert at scale.

## Operating Rules

- Start read-only.
- Use batch tools for 2+ similar operations.
- Prefer dry-run/plan before medium/high risk changes.
- Ask before irreversible changes.
- Validate after every small transaction: compile, console, scene/visual QA.
- Report files/assets/scenes changed and rollback hints.
