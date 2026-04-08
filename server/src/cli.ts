#!/usr/bin/env node

import { fileURLToPath } from "url";
import { dirname, join, resolve } from "path";
import { existsSync, mkdirSync, cpSync, writeFileSync, readFileSync } from "fs";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const command = process.argv[2];

if (command === "setup") {
  setup();
} else {
  // Default: run MCP server
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
  return "github:anthropics/unity-mcp";
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

  // 2. Create .mcp.json
  const mcpJsonPath = join(targetDir, ".mcp.json");
  const ghUrl = getGitHubUrl();
  const mcpConfig = {
    mcpServers: {
      "karnellabs-unity-mcp": {
        command: "npx",
        args: ["-y", ghUrl],
      },
    },
  };

  if (existsSync(mcpJsonPath)) {
    try {
      const existing = JSON.parse(readFileSync(mcpJsonPath, "utf-8"));
      if (existing.mcpServers?.["karnellabs-unity-mcp"]) {
        console.log("   ⏭️  .mcp.json already configured, skipping.");
      } else {
        existing.mcpServers = existing.mcpServers || {};
        existing.mcpServers["karnellabs-unity-mcp"] = mcpConfig.mcpServers["karnellabs-unity-mcp"];
        writeFileSync(mcpJsonPath, JSON.stringify(existing, null, 2) + "\n");
        console.log("   ✅ Added karnellabs-unity-mcp to existing .mcp.json");
      }
    } catch {
      writeFileSync(mcpJsonPath, JSON.stringify(mcpConfig, null, 2) + "\n");
      console.log("   ✅ Created .mcp.json");
    }
  } else {
    writeFileSync(mcpJsonPath, JSON.stringify(mcpConfig, null, 2) + "\n");
    console.log("   ✅ Created .mcp.json");
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
}
