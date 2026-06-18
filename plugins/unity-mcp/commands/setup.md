---
description: Set up Karnelian Unity MCP for the current Unity project
argument-hint: [profile]
---

Set up Karnelian Unity MCP for the current Unity project.

Do the following:

1. Confirm the current directory looks like a Unity project by checking for `Assets/` and `ProjectSettings/`.
2. If this is a UI-focused project, use profile `core,ui`; if 2D-focused, use `core,2d`; otherwise use `core` unless the user specified another profile in `$ARGUMENTS`.
3. Run the setup command from the Unity project root:

```bash
npx -y github:karnelian/unity-mcp setup --profile=<profile>
```

If `$ARGUMENTS` contains a profile, use it directly. Examples:

- `/unity-mcp:setup core,ui`
- `/unity-mcp:setup core,2d`
- `/unity-mcp:setup full`

After setup, tell the user to open Unity and check KarnelLabs MCP Server status. Prefer `unity_project_health` as the first MCP call once connected.
