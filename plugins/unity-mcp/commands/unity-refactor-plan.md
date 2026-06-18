---
description: Plan a Unity script refactor using architecture, roles, testability, inspector, and scene-contract guardrails
argument-hint: [target scripts or feature]
---

Create a refactor plan before changing Unity scripts. Use `$ARGUMENTS` as the target script, folder, system, or feature.

Do the following, read-only:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_project_health` first. If compile errors exist, recommend `/unity-compile-fix` before refactor.
3. Inspect only relevant scripts and scene/package context.
4. Apply these design lenses:
   - MonoBehaviour vs ScriptableObject vs plain C# service role
   - serialized field/API stability
   - event/state-machine/object-pool/observer pattern fit
   - testability and EditMode/PlayMode test seam
   - asmdef/runtime/editor split if relevant
   - Inspector UX and scene contract impact
5. Output a bite-sized refactor plan with risks and validation steps.

Rules:

- Plan only. Do not edit files in this command.
- Keep public API and serialized field renames explicit and justified.
- Recommend `/unity-compile-fix` after implementation and `/unity-visual-qa` after visual/scene-impacting changes.
