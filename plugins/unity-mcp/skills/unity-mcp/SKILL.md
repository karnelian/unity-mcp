---
name: unity-mcp
description: "Use Karnelian Unity MCP from Claude Code. Invoke when the user asks Claude to inspect or edit a Unity project through the Unity Editor."
---

# Unity MCP

Use this plugin when working on Unity projects through the Unity Editor.

## First-time project setup

The Claude Code plugin starts the MCP server automatically, but each Unity project still needs the Unity Editor package installed once.

From the Unity project root, run:

```bash
npx -y github:karnelian/unity-mcp setup --profile=core
```

For UI-heavy projects:

```bash
npx -y github:karnelian/unity-mcp setup --profile=core,ui
```

For 2D projects:

```bash
npx -y github:karnelian/unity-mcp setup --profile=core,2d
```

For all tools during broad debugging:

```bash
npx -y github:karnelian/unity-mcp setup --profile=full
```

Then open Unity and start the KarnelLabs MCP server window if it is not already listening.

## Preferred workflow

1. Start with `unity_project_health` for a compact project snapshot.
2. Use summary/pagination options before requesting full hierarchy, asset, package, or log dumps.
3. Prefer `unity_script_writeAndCompile` for C# script writes, then follow with `unity_script_compileCheck` or `unity_project_health` if Unity is still compiling.
4. Use `unity_debug_visualQaBundle` for visual/editor-state debugging instead of repeated individual screenshots.
5. Keep MCP profiles narrow. Use `core` by default and add groups like `ui`, `2d`, `xr`, `rendering`, or `cinemachine` only when needed.

## Common profile choices

- `core`: normal Unity editing, scripts, scenes, assets, validation, workflow tools.
- `core,ui`: uGUI / UI Toolkit / TextMeshPro work.
- `core,2d`: sprite, tilemap, 2D physics, 2D animation.
- `core,xr,rendering`: Quest/XR and render-pipeline tuning.
- `full`: temporary broad exploration/debugging only.

## Workflow Command Map

Use these Claude Code commands as the default operating loops:

- `/unity-mcp:unity-project-scout` — first pass on unfamiliar projects; read-only stack/convention scan.
- `/unity-mcp:unity-health-check` — compact health snapshot before mutation.
- `/unity-mcp:unity-compile-fix` — C# compile-error loop using `unity_script_writeAndCompile`; C# only.
- `/unity-mcp:unity-visual-qa` — Console/Hierarchy/Inspector/Game/Scene visual QA bundle.
- `/unity-mcp:unity-ui-qa` — focused UI/canvas/UI Toolkit/TextMeshPro QA.
- `/unity-mcp:unity-scene-contract` — playable-scene contract validation.
- `/unity-mcp:unity-package-risk` — package/version/optional dependency risk scan.
- `/unity-mcp:unity-mobile-check` — Android/iOS/Quest readiness check.
- `/unity-mcp:unity-performance-scan` — read-only performance risk scan.
- `/unity-mcp:unity-build-smoke` — build-smoke plan or explicit minimal build run.
- `/unity-mcp:unity-test-run` — Unity test discovery/run flow.
- `/unity-mcp:unity-prefab-audit` — prefab/override/missing-reference audit.
- `/unity-mcp:unity-refactor-plan` — script refactor plan using design guardrails; plan only.

Default sequence for feature work:

1. `unity-project-scout` for unfamiliar projects.
2. `unity-health-check` before edits.
3. Implement in small transactions.
4. `unity-compile-fix` until clean.
5. `unity-scene-contract` and `unity-visual-qa` for scene/visual work.
6. `unity-performance-scan`, `unity-mobile-check`, or `unity-build-smoke` before platform validation.

## Advisory Skill Modules

Load these on demand before non-trivial decisions:

- `unity-project-scout` — project conventions and stack discovery.
- `unity-scene-contracts` — scene wiring and playable-scene requirements.
- `unity-performance` — performance review and measurement-first guidance.
- `unity-mobile` — Android/iOS/Quest constraints.
- `unity-script-design` — class roles, testability, serialized API safety.
- `unity-safety-risk` — dry-run, confirmation, rollback, and validation rules.

## Install from marketplace

After this repository is added as a Claude Code marketplace:

```bash
claude plugin marketplace add karnelian/unity-mcp
claude plugin install unity-mcp@karnelian-unity-mcp -s user
```

Restart Claude Code after installing or updating the plugin.
