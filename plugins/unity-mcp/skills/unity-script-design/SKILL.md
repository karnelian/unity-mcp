---
name: unity-script-design
description: "Unity script architecture guardrails: MonoBehaviour roles, ScriptableObjects, services, testability, inspector UX, and serialized API safety."
---

# Unity Script Design

Use before refactoring or writing non-trivial Unity C#.

## Rules

- Decide class role first: MonoBehaviour, ScriptableObject, plain C# service, editor utility, installer/bootstrap.
- Keep serialized field names stable unless migration is planned.
- Separate runtime logic from scene wiring where possible for testability.
- Prefer explicit scene contracts over hidden `FindObjectOfType` dependencies.
- Use events/state machines/object pools/observer only when the project scale benefits.
- After script edits: compile check first, then scene/visual QA if behavior is visible.
