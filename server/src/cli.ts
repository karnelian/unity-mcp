#!/usr/bin/env node

import { fileURLToPath } from "url";
import { dirname, join, resolve } from "path";
import { existsSync, mkdirSync, cpSync, writeFileSync, readFileSync } from "fs";

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const command = process.argv[2];
const commandArgs = process.argv.slice(3);
const flags = new Set(commandArgs);
const NEWTONSOFT_PACKAGE = "com.unity.nuget.newtonsoft-json";
const NEWTONSOFT_VERSION = "3.2.1";

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

type McpTransport = "stdio" | "http" | "sse";

function normalizeMcpTransport(value: string | undefined): McpTransport {
  const transport = (value || "stdio").trim().toLowerCase();
  if (transport === "http" || transport === "streamable-http" || transport === "streamable_http") return "http";
  if (transport === "sse") return "sse";
  if (transport && transport !== "stdio") {
    console.warn(`⚠️  Unknown MCP transport '${transport}', using stdio. Expected: stdio, http, sse.`);
  }
  return "stdio";
}

function readIntArg(name: string, args = process.argv.slice(2), fallback: number): number {
  const raw = readArg(name, args);
  if (!raw) return fallback;
  const parsed = Number.parseInt(raw, 10);
  if (!Number.isFinite(parsed) || parsed <= 0) {
    console.warn(`⚠️  Invalid --${name}=${raw}, using ${fallback}.`);
    return fallback;
  }
  return parsed;
}

function normalizeEndpoint(value: string | undefined, fallback: string): string {
  const endpoint = (value || fallback).trim() || fallback;
  return endpoint.startsWith("/") ? endpoint : `/${endpoint}`;
}

if (command === "setup") {
  setup();
} else if (command === "update") {
  update();
} else if (command === "instances") {
  listInstances();
} else {
  // Default: run MCP server
  // Support --port / --profile / --tools plus MCP client transport flags.
  const port = readArg("port");
  if (port) process.env.UNITY_WS_PORT = port;

  const profile = readArg("profile");
  if (profile) process.env.UNITY_MCP_PROFILE = normalizeProfile(profile);

  const tools = normalizeTools(readArg("tools"));
  if (tools) process.env.UNITY_MCP_TOOLS = tools;

  const transport = readArg("transport");
  if (transport) process.env.UNITY_MCP_TRANSPORT = transport;

  const mcpHost = readArg("mcp-host");
  if (mcpHost) process.env.UNITY_MCP_HTTP_HOST = mcpHost;

  const mcpPort = readArg("mcp-port");
  if (mcpPort) process.env.UNITY_MCP_HTTP_PORT = mcpPort;

  const mcpEndpoint = readArg("mcp-endpoint");
  if (mcpEndpoint) process.env.UNITY_MCP_HTTP_ENDPOINT = mcpEndpoint;

  const sseEndpoint = readArg("sse-endpoint");
  if (sseEndpoint) process.env.UNITY_MCP_SSE_ENDPOINT = sseEndpoint;

  const messageEndpoint = readArg("message-endpoint");
  if (messageEndpoint) process.env.UNITY_MCP_SSE_MESSAGE_ENDPOINT = messageEndpoint;

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
        const url = pkg.repository.url
          .replace("git+", "")
          .replace(".git", "");
        const githubMatch = url.match(/^https:\/\/github\.com\/([^/]+\/[^/]+)$/);
        if (githubMatch) return `github:${githubMatch[1]}`;
        return url;
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
  ensureNewtonsoftPackage(targetDir);

  const requestedTransport = readArg("mcp-transport", commandArgs) || readArg("transport", commandArgs);
  const shouldUpdateMcpConfig = flags.has("--mcp-config") || Boolean(requestedTransport);
  if (shouldUpdateMcpConfig) {
    writeMcpJsonConfig(targetDir, normalizeMcpTransport(requestedTransport));
  } else {
    console.log("   ⏭️  Kept existing MCP client config. Pass --mcp-config or --mcp-transport=http|sse to update .mcp.json too.");
  }

  console.log("");
  console.log("Reopen Unity to recompile the plugin.");
}

function ensureNewtonsoftPackage(targetDir: string) {
  const packagesDir = join(targetDir, "Packages");
  const manifestPath = join(packagesDir, "manifest.json");

  if (!existsSync(packagesDir)) {
    mkdirSync(packagesDir, { recursive: true });
  }

  let manifest: any = { dependencies: {} };
  if (existsSync(manifestPath)) {
    try {
      manifest = JSON.parse(readFileSync(manifestPath, "utf-8"));
    } catch {
      console.error(`❌ Failed to parse ${manifestPath}.`);
      console.error(`   Add this dependency manually under dependencies: "${NEWTONSOFT_PACKAGE}": "${NEWTONSOFT_VERSION}"`);
      process.exit(1);
    }
  }

  manifest.dependencies = manifest.dependencies || {};
  if (manifest.dependencies[NEWTONSOFT_PACKAGE]) {
    console.log(`   ✅ Newtonsoft Json package already present (${NEWTONSOFT_PACKAGE}@${manifest.dependencies[NEWTONSOFT_PACKAGE]})`);
    return;
  }

  manifest.dependencies[NEWTONSOFT_PACKAGE] = NEWTONSOFT_VERSION;
  writeFileSync(manifestPath, JSON.stringify(manifest, null, 2) + "\n");
  console.log(`   ✅ Added Unity Newtonsoft Json package (${NEWTONSOFT_PACKAGE}@${NEWTONSOFT_VERSION}) to Packages/manifest.json`);
  console.log("      Unity Package Manager resolves this package; no external nuget CLI is required.");
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

function buildMcpServerConfig(transport: McpTransport) {
  const ghUrl = getGitHubUrl();
  const profile = normalizeProfile(readArg("profile", commandArgs));
  const tools = normalizeTools(readArg("tools", commandArgs));
  const mcpHost = readArg("mcp-host", commandArgs) || "127.0.0.1";
  const mcpPort = readIntArg("mcp-port", commandArgs, 3001);
  const mcpEndpoint = normalizeEndpoint(readArg("mcp-endpoint", commandArgs), "/mcp");
  const sseEndpoint = normalizeEndpoint(readArg("sse-endpoint", commandArgs), "/sse");

  if (transport === "http") {
    return {
      url: `http://${mcpHost}:${mcpPort}${mcpEndpoint}`,
      transport: "http",
    };
  }

  if (transport === "sse") {
    return {
      url: `http://${mcpHost}:${mcpPort}${sseEndpoint}`,
      transport: "sse",
    };
  }

  const serverArgs = ["-y", ghUrl, `--profile=${profile}`];
  if (tools) serverArgs.push(`--tools=${tools}`);
  return {
    command: "npx",
    args: serverArgs,
  };
}

function writeMcpJsonConfig(targetDir: string, transport: McpTransport) {
  const mcpJsonPath = join(targetDir, ".mcp.json");
  const serverConfig = buildMcpServerConfig(transport);
  const mcpConfig = {
    mcpServers: {
      "karnellabs-unity-mcp": serverConfig,
    },
  };

  const profile = normalizeProfile(readArg("profile", commandArgs));
  const tools = normalizeTools(readArg("tools", commandArgs));
  const transportLabel = transport === "stdio" ? `stdio (${profile}${tools ? ` + tools:${tools}` : ""})` : transport;

  if (existsSync(mcpJsonPath)) {
    try {
      const existing = JSON.parse(readFileSync(mcpJsonPath, "utf-8"));
      existing.mcpServers = existing.mcpServers || {};
      existing.mcpServers["karnellabs-unity-mcp"] = serverConfig;
      writeFileSync(mcpJsonPath, JSON.stringify(existing, null, 2) + "\n");
      console.log(`   ✅ Updated karnellabs-unity-mcp in .mcp.json (${transportLabel})`);
    } catch {
      writeFileSync(mcpJsonPath, JSON.stringify(mcpConfig, null, 2) + "\n");
      console.log(`   ✅ Recreated .mcp.json (${transportLabel})`);
    }
  } else {
    writeFileSync(mcpJsonPath, JSON.stringify(mcpConfig, null, 2) + "\n");
    console.log(`   ✅ Created .mcp.json (${transportLabel})`);
  }

  if (transport !== "stdio") {
    const url = (serverConfig as { url: string }).url;
    console.log(`   ℹ️  ${transport.toUpperCase()} clients should connect to: ${url}`);
    console.log("   ℹ️  Start the URL transport with the matching built-in command when your client does not launch servers itself:");
    console.log(`      npx github:karnelian/unity-mcp --transport=${transport} --mcp-port=${readIntArg("mcp-port", commandArgs, 3001)}`);
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

  // 2. Ensure Unity's official Newtonsoft.Json package is available.
  // The editor bridge uses Newtonsoft.Json/JObject for JSON-RPC parsing.
  ensureNewtonsoftPackage(targetDir);

  // 3. Optional MCP client config creation.
  // Default setup keeps the zero-config Claude plugin workflow and does not write
  // a project-local .mcp.json. Passing --mcp-config writes config explicitly.
  // Passing --mcp-transport=http|sse (or --transport=http|sse during setup) also
  // writes URL-based config so users do not have to hand-author long commands.
  if (!flags.has("--update")) {
    const requestedTransport = readArg("mcp-transport", commandArgs) || readArg("transport", commandArgs);
    const mcpTransport = normalizeMcpTransport(requestedTransport);
    const shouldWriteMcpConfig = flags.has("--mcp-config") || Boolean(requestedTransport);

    if (shouldWriteMcpConfig) {
      writeMcpJsonConfig(targetDir, mcpTransport);
    } else {
      console.log("   ⏭️  Skipped project .mcp.json. Use the Claude plugin globally, or pass --mcp-config / --mcp-transport=http for explicit client config.");
    }
  }

  // 4. Done
  console.log("");
  console.log("🎉 Setup complete!");
  console.log("");
  console.log("Next steps:");
  console.log("  1. Open this project in Unity");
  console.log("  2. Wait for compilation (Tools > KarnelLabs MCP > Server Window)");
  console.log("  3. Open Claude Code in this folder and start working!");
  console.log("");
  console.log("Commands:");
  console.log("  npx github:karnelian/unity-mcp setup --profile=core,ui           — Install/update Unity plugin (no local .mcp.json by default)");
  console.log("  npx github:karnelian/unity-mcp setup --mcp-config                — Also write explicit stdio .mcp.json");
  console.log("  npx github:karnelian/unity-mcp setup --mcp-transport=http        — Write URL-based HTTP .mcp.json");
  console.log("  npx github:karnelian/unity-mcp setup --mcp-transport=sse         — Write URL-based SSE .mcp.json");
  console.log("  npx github:karnelian/unity-mcp update                            — Update plugin only, keep existing MCP config");
  console.log("  npx github:karnelian/unity-mcp update --mcp-transport=http       — Update plugin and rewrite HTTP .mcp.json");
  console.log("  npx github:karnelian/unity-mcp instances                         — List running Unity instances");
  console.log("");
}
