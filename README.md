# Unity MCP — 574 Tools for AI-Powered Game Development

MCP (Model Context Protocol) server that connects **Claude Code** directly to the **Unity Editor**. Control every aspect of Unity — scenes, assets, materials, physics, lighting, animation, UI, and more — through natural language.

## Quick Start

```bash
# In your Unity project root:
npx github:karnelian/unity-mcp setup
```

This single command:
1. Installs the Unity Editor plugin to `Assets/KarnelLabsMCP/Editor/`
2. Creates `.mcp.json` for Claude Code auto-connection

Then:
1. Open the project in Unity
2. Wait for compilation → **Tools > KarnelLabs MCP > Server Window**
3. Open Claude Code in the same folder and start building!

## Commands

```bash
npx github:karnelian/unity-mcp setup       # First-time install (plugin + .mcp.json)
npx github:karnelian/unity-mcp update       # Update plugin only (keeps .mcp.json)
npx github:karnelian/unity-mcp instances    # List running Unity instances
```

## Architecture

```
Claude Code ←→ MCP Server (Node.js, stdio) ←→ WebSocket (port 8099) ←→ Unity Editor Plugin (C#)
```

Multi-instance support: each Unity project auto-registers to `~/.karnellabs-mcp/registry.json` with heartbeat. Connect to different instances via `--port=PORT`.

## 574 Tools Across 48 Categories

| Category | Tools | Description |
|----------|-------|-------------|
| **Scene** | 20 | Create, find, delete, duplicate, hierarchy, selection, transform, parent, undo/redo |
| **Script** | 8 | Create, read, edit, delete, rename, search, compile check, list |
| **Asset** | 12 | Search, info, move, copy, delete, create folder/material/prefab, labels, import settings |
| **Prefab** | 14 | Create, instantiate, batch instantiate, apply, revert, unpack, overrides, variants |
| **Material** | 20 | Create, assign, duplicate, set color/float/texture/shader/emission, batch operations |
| **Light** | 18 | Create, delete, find, set properties/shadows/cookie/ambient, probes, reflection probes |
| **Camera** | 12 | Create, find, set properties/viewport/culling, look at, screenshot |
| **Physics** | 16 | Add collider/rigidbody, raycast, spherecast, boxcast, overlap, gravity, collision matrix |
| **Audio** | 18 | Add source/listener, play/pause/stop, set clip/properties, mixer control |
| **Animator** | 16 | Create controller, add state/layer/parameter/transition, blend tree, assign |
| **Animation 2D** | 10 | Create clips, sprite keyframes, slice sprite sheets, sprite atlas management |
| **UI (uGUI)** | 16 | Create canvas/button/text/image/slider/toggle/dropdown/input/panel, layout |
| **UI Toolkit** | 8 | Create UXML/USS/UIDocument/PanelSettings, hierarchy, info |
| **Terrain** | 18 | Create, set height/size, paint texture, perlin noise, smooth, flatten, trees, details |
| **NavMesh** | 10 | Bake, clear, add agent/obstacle/link, set destination, find path |
| **Component** | 12 | Get, list, add, remove, enable, copy/paste, move, batch operations |
| **Shader** | 12 | Find, list, get info/properties/keywords/source, find materials, global properties |
| **Particle** | 18 | Create, find, play, set main/emission/shape/color/size/velocity/noise/trails/renderer |
| **VFX Graph** | 10 | Create, find, get info, play/stop, set float/int/bool/vector |
| **Spline** | 12 | Create, find, add/remove/set knot, tangent mode, extrude, instantiate, animate |
| **Cinemachine** | 20 | Create virtual/freelook/dolly/clearshot camera, set follow/look/aim/body/lens/noise |
| **Timeline** | 10 | Create clip, find, get info/curves/events, set settings/curve, duplicate, delete key |
| **ProBuilder** | 18 | Create cube/sphere/cylinder/plane/stair/arch/door/pipe, extrude, bevel, subdivide |
| **Tilemap** | 8 | Create, find, get info/tile, set tile/tiles batch, clear, set renderer |
| **Sprite** | 4 | Create, find, set properties, set sorting order |
| **Texture** | 10 | Find, get info/settings/memory, set settings/sprite/normal map/platform/readable, resize |
| **Model** | 8 | Find, get info/mesh/animations/settings, set scale/rig/material/animation settings |
| **Project** | 14 | Info, player settings, quality settings, render pipeline, time, tags, layers, build target |
| **Editor** | 10 | Play mode, console, build, build settings, diagnostics, connection health check, tests |
| **Debug** | 16 | Log, screenshot, system info, clear console, prefs, editor prefs, defines, log capture |
| **Package** | 8 | List, info, add, remove, search, resolve, built-in, version |
| **Batch** | 3 | Batch create, delete, set transform |
| **Workflow** | 5 | Begin, end, status, undo last, undo session |
| **Placement** | 10 | Grid, circle, scatter, align, distribute, snap, stack, mirror, randomize, ground snap |
| **Validation** | 10 | Missing scripts/references, duplicate names, empty objects, large meshes, shader errors |
| **Optimization** | 10 | Find large assets, compress textures/audio, GPU instancing, mesh compression, mipmaps |
| **Profiler** | 10 | Memory, rendering stats, texture/mesh memory, object/material/asset count, complexity |
| **Cleaner** | 10 | Unused assets/materials, duplicates, empty folders, missing scripts, large files, dependencies |
| **Rendering** | 10 | Create volume, overrides, fog, skybox, pipeline info, global shader properties |
| **ScriptableObject** | 10 | Create, find, get info/properties/types, set property, duplicate, delete, JSON import/export |
| **Input System** | 10 | Create action asset, add map/action/binding, find assets, get info, player input |
| **Event** | 5 | List events, add/remove listener, get listeners, set state |
| **Addressables** | 10 | List groups, create/remove group, mark/remove addressable, set address/label, find, settings |
| **Localization** | 10 | Locales, string tables, entries, CSV export (requires com.unity.localization) |
| **Version Control** | 8 | Git status, changes, history, branches, remotes, diff, stash |
| **XR** | 18 | Create XR rig, interactors, interactables, teleport, locomotion, hand tracking |
| **Perception** | 5 | Scene summary, context, hierarchy describe, materials, script analyze |
| **Smart** | 2 | Scene query, reference bind |

## Requirements

- **Unity** 2021.3+ (tested on Unity 6)
- **Node.js** 18+
- **Claude Code** (or any MCP-compatible client)

## Configuration

Environment variables for the MCP server:

| Variable | Default | Description |
|----------|---------|-------------|
| `UNITY_WS_HOST` | `127.0.0.1` | Unity WebSocket host |
| `UNITY_WS_PORT` | `8099` | Unity WebSocket port |

## Conditional Features

Some tools require optional Unity packages. The plugin uses assembly definition `versionDefines` for automatic detection:

- **Cinemachine** — `com.unity.cinemachine`
- **ProBuilder** — `com.unity.probuilder`
- **Timeline** — `com.unity.timeline`
- **Splines** — `com.unity.splines`
- **VFX Graph** — `com.unity.visualeffectgraph`
- **Input System** — `com.unity.inputsystem`
- **Addressables** — `com.unity.addressables`
- **Localization** — `com.unity.localization`
- **XR Interaction Toolkit** — `com.unity.xr.interaction.toolkit`

## Manual Installation

If you prefer not to use `npx`:

```bash
git clone https://github.com/karnelian/unity-mcp.git
cd unity-mcp/server
npm install
npm run build
```

Then copy `unity-plugin/Editor/` to your Unity project's `Assets/KarnelLabsMCP/Editor/`.

Add to your `.mcp.json`:
```json
{
  "mcpServers": {
    "karnellabs-unity-mcp": {
      "command": "node",
      "args": ["/absolute/path/to/unity-mcp/server/dist/cli.js"]
    }
  }
}
```

## Multi-Instance

Running multiple Unity projects simultaneously? Each Unity instance registers itself automatically. Use the `instances` command to see all active projects, then connect to a specific one:

```json
{
  "mcpServers": {
    "karnellabs-unity-mcp": {
      "command": "npx",
      "args": ["-y", "github:karnelian/unity-mcp", "--port=8100"]
    }
  }
}
```

## License

MIT
