---
description: Fix Unity C# compile errors with a write-and-compile loop
argument-hint: [optional error focus or file path]
---

Fix Unity C# compile errors through Karnelian Unity MCP.

Use `$ARGUMENTS` as an optional error focus, class name, namespace, or file path if provided.

Do the following loop:

1. Confirm Unity MCP is connected. If it is not connected, tell the user to open Unity and start `Tools > KarnelLabs MCP > Server Window`, then stop.
2. Run `unity_mcp_safety_manifest` and note that this workflow should normally use only script mutating tools (`unity_script_edit` or `unity_script_writeAndCompile`).
3. Run `unity_script_compileCheck` or `unity_project_health` to get the current compile status and Console compiler errors.
4. Pick the smallest coherent error group to fix first. If `$ARGUMENTS` is provided, prioritize matching errors/files.
5. Read only the directly relevant C# files.
6. Explain the suspected root cause briefly before editing.
7. Before the first script write, call the intended script write/edit tool with `dryRun: true` when supported and verify it targets only the intended file(s).
8. Make the smallest safe C# change.
9. Write scripts with `unity_script_writeAndCompile`.
10. Re-run `unity_script_compileCheck` or `unity_project_health`.
11. If compile errors remain, repeat from step 4 until either:
   - compilation is clean, or
   - the remaining errors require a product/design decision from the user.

Rules:

- Edit C# scripts only unless the user explicitly asks for asset/scene/prefab changes.
- Do not rename serialized fields, public APIs, assets, GameObjects, or scenes unless necessary for compilation.
- Do not do broad refactors while fixing compile errors.
- Preserve existing behavior wherever possible.
- If Unity is still compiling after `unity_script_writeAndCompile`, say so and follow with a compile check once it is ready.
- Stop and ask before deleting code, changing assembly definitions, removing packages, or changing project settings.

Final response:

- List files changed.
- List the compile errors fixed.
- Report the final compile status.
- Mention any remaining warnings/errors and recommended next step.
