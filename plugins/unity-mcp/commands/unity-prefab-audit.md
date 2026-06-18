---
description: Audit Unity prefabs for overrides, missing references, variants, and instance consistency
argument-hint: [prefab path/name or focus]
---

Audit prefabs and prefab instances. Use `$ARGUMENTS` as a prefab path/name/focus if provided.

Do the following, read-only:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first.
3. Inspect relevant prefab assets/instances with limits:
   - missing scripts/references
   - unapplied overrides and variant relationships
   - duplicate prefab names or suspicious nesting
   - broken component references
   - scene instances that diverge from source prefab
4. Report findings and a safe fix plan.

Rules:

- Read-only. Do not apply/revert overrides, rename prefabs, or edit assets without explicit approval.
- Any future prefab apply/revert/delete must use dry-run/confirmation where available.
