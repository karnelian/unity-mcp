---
name: unity-mcp
description: Use Karnelian Unity MCP from Claude Code. Invoke when the user asks Claude to inspect or edit a Unity project through the Unity Editor.
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

## Install from marketplace

After this repository is added as a Claude Code marketplace:

```bash
claude plugin marketplace add karnelian/unity-mcp
claude plugin install unity-mcp@karnelian-unity-mcp -s user
```

Restart Claude Code after installing or updating the plugin.
