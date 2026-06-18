---
name: unity-safety-risk-guide
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
- Prefer dry-run/plan before medium/high risk changes. Public mutating tools expose `dryRun` and `confirmationToken` where supported.
- Before high-risk operations, call `unity_mcp_safety_manifest` or `unity_mcp_safety_describe` to inspect risk and confirmation token details.
- Ask before irreversible changes; if high-risk confirmation is enabled, re-run with the exact `confirmationToken` only after explaining the risk.
- Validate after every small transaction: compile, console, scene/visual QA.
- Report files/assets/scenes changed and rollback hints.
