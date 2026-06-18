#!/usr/bin/env node

import { fileURLToPath } from "url";
import { dirname, join, resolve } from "path";
import { existsSync, mkdirSync, cpSync, writeFileSync, readFileSync } from "fs";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const command = process.argv[2];
const commandArgs = process.argv.slice(3);
const flags = new Set(commandArgs);

function readArg(name: string, args = process.argv.slice(2)): string | undefined {
  const flag = args.find(a => a.startsWith(`--${name}=`));
  return flag ? flag.slice(name.length + 3) : undefined;
}

function normalizeProfile(profile: string | undefined): string {
  return (profile || "core")
    .split(",")
    .map(p => p.trim().toLowerCase())
    .filter(Boolean)
    .join(",") || "core";
}

function normalizeTools(tools: string | undefined): string | undefined {
  const normalized = tools
    ?.split(",")
    .map(t => t.trim().toLowerCase())
    .filter(Boolean)
    .join(",");
  return normalized || undefined;
}

if (command === "setup") {
  setup();
} else if (command === "update") {
  update();
} else if (command === "instances") {
  listInstances();
} else {
  // Default: run MCP server
  // Support --port / --profile / --tools flags for multi-instance and tool-surface control.
  const port = readArg("port");
  if (port) process.env.UNITY_WS_PORT = port;

  const profile = readArg("profile");
  if (profile) process.env.UNITY_MCP_PROFILE = normalizeProfile(profile);

  const tools = normalizeTools(readArg("tools"));
  if (tools) process.env.UNITY_MCP_TOOLS = tools;

  await import("./index.js");
}

function findUnityPlugin(): string {
  // Check multiple possible locations:
  // 1. GitHub install: repo_root/unity-plugin/Editor (cli is at repo_root/server/dist/cli.js)
  const fromGithub = resolve(__dirname, "../../unity-plugin/Editor");
  if (existsSync(fromGithub)) return fromGithub;

  // 2. npm publish: package_root/unity-plugin/Editor (cli is at package_root/server/dist/cli.js)
  const fromNpm = resolve(__dirname, "../unity-plugin/Editor");
  if (existsSync(fromNpm)) return fromNpm;

  console.error("❌ Unity plugin files not found. Package may be corrupted.");
  process.exit(1);
}

function getGitHubUrl(): string {
  // Try to read repo URL from root package.json
  const rootPkg = resolve(__dirname, "../../package.json");
  if (existsSync(rootPkg)) {
    try {
      const pkg = JSON.parse(readFileSync(rootPkg, "utf-8"));
      if (pkg.repository?.url) {
        return pkg.repository.url
          .replace("git+", "")
          .replace(".git", "");
      }
    } catch {}
  }
  return "github:karnelian/unity-mcp";
}

function update() {
  const targetDir = process.cwd();
  const unityPluginSrc = findUnityPlugin();
  const assetsDir = join(targetDir, "Assets");

  if (!existsSync(assetsDir)) {
    console.error("❌ Assets/ folder not found. Run this command from a Unity project root.");
    process.exit(1);
  }

  const pluginDest = join(assetsDir, "KarnelLabsMCP", "Editor");
  if (!existsSync(pluginDest)) {
    console.error("❌ Plugin not installed. Run 'setup' first.");
    process.exit(1);
  }

  console.log("📦 Updating Unity plugin...");
  cpSync(unityPluginSrc, pluginDest, { recursive: true, force: true });
  console.log("   ✅ Plugin updated at Assets/KarnelLabsMCP/Editor/");
  console.log("");
  console.log("Reopen Unity to recompile the plugin.");
}

function listInstances() {
  const home = process.env.HOME || process.env.USERPROFILE || "";
  const registryPath = join(home, ".karnellabs-mcp", "registry.json");

  if (!existsSync(registryPath)) {
    console.log("No Unity instances registered.");
    return;
  }

  try {
    const entries = JSON.parse(readFileSync(registryPath, "utf-8"));
    if (!Array.isArray(entries) || entries.length === 0) {
      console.log("No Unity instances registered.");
      return;
    }

    console.log("🎮 Registered Unity instances:\n");
    for (const e of entries) {
      const stale = Date.now() - new Date(e.lastHeartbeat).getTime() > 120_000;
      const status = stale ? "⚠️  stale" : "✅ active";
      console.log(`  ${status}  port:${e.port}  ${e.projectName} (Unity ${e.unityVersion})`);
      console.log(`           ${e.projectPath}`);
      console.log("");
    }
  } catch {
    console.log("Failed to read registry.");
  }
}

function setup() {
  const targetDir = process.cwd();
  const unityPluginSrc = findUnityPlugin();
  const assetsDir = join(targetDir, "Assets");

  // Verify this looks like a Unity project
  if (!existsSync(assetsDir)) {
    console.error("❌ Assets/ folder not found. Run this command from a Unity project root.");
    process.exit(1);
  }

  // 1. Copy Unity plugin
  const pluginDest = join(assetsDir, "KarnelLabsMCP", "Editor");
  console.log("📦 Installing Unity plugin...");

  if (existsSync(pluginDest)) {
    console.log("   Updating existing installation...");
  }

  mkdirSync(pluginDest, { recursive: true });
  cpSync(unityPluginSrc, pluginDest, { recursive: true, force: true });
  console.log("   ✅ Plugin installed to Assets/KarnelLabsMCP/Editor/");

  // 2. Create .mcp.json (skip if --update flag)
  if (!flags.has("--update")) {
    const mcpJsonPath = join(targetDir, ".mcp.json");
    const ghUrl = getGitHubUrl();
    const profile = normalizeProfile(readArg("profile", commandArgs));
    const tools = normalizeTools(readArg("tools", commandArgs));
    const serverArgs = ["-y", ghUrl, `--profile=${profile}`];
    if (tools) serverArgs.push(`--tools=${tools}`);
    const shouldUpdateExistingConfig = Boolean(readArg("profile", commandArgs) || readArg("tools", commandArgs));
    const mcpConfig = {
      mcpServers: {
        "karnellabs-unity-mcp": {
          command: "npx",
          args: serverArgs,
        },
      },
    };

    if (existsSync(mcpJsonPath)) {
      try {
        const existing = JSON.parse(readFileSync(mcpJsonPath, "utf-8"));
        if (existing.mcpServers?.["karnellabs-unity-mcp"] && !shouldUpdateExistingConfig) {
          console.log("   ⏭️  .mcp.json already configured, skipping. Use setup --profile=... to update the tool profile.");
        } else {
          existing.mcpServers = existing.mcpServers || {};
          existing.mcpServers["karnellabs-unity-mcp"] = mcpConfig.mcpServers["karnellabs-unity-mcp"];
          writeFileSync(mcpJsonPath, JSON.stringify(existing, null, 2) + "\n");
          console.log(existing.mcpServers?.["karnellabs-unity-mcp"] && shouldUpdateExistingConfig
            ? `   ✅ Updated karnellabs-unity-mcp profile in .mcp.json (${profile}${tools ? ` + tools:${tools}` : ""})`
            : "   ✅ Added karnellabs-unity-mcp to existing .mcp.json");
        }
      } catch {
        writeFileSync(mcpJsonPath, JSON.stringify(mcpConfig, null, 2) + "\n");
        console.log("   ✅ Created .mcp.json");
      }
    } else {
      writeFileSync(mcpJsonPath, JSON.stringify(mcpConfig, null, 2) + "\n");
      console.log("   ✅ Created .mcp.json");
    }
  }

  // 3. Done
  console.log("");
  console.log("🎉 Setup complete!");
  console.log("");
  console.log("Next steps:");
  console.log("  1. Open this project in Unity");
  console.log("  2. Wait for compilation (Tools > KarnelLabs MCP > Server Window)");
  console.log("  3. Open Claude Code in this folder and start working!");
  console.log("");
  console.log("Commands:");
  console.log("  npx github:karnelian/unity-mcp setup --profile=core,ui  — Set/update tool profile");
  console.log("  npx github:karnelian/unity-mcp update                   — Update plugin only");
  console.log("  npx github:karnelian/unity-mcp instances                — List running Unity instances");
  console.log("");
}
