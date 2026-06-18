import { existsSync, readFileSync } from "fs";
import { homedir } from "os";
import { join, normalize, resolve } from "path";

const REGISTRY_STALE_MS = 120_000;
const DEFAULT_PORT = 8099;

interface RegistryEntry {
  instanceId?: string;
  port?: number;
  pid?: number;
  projectPath?: string;
  projectName?: string;
  unityVersion?: string;
  lastHeartbeat?: string;
}

export interface UnityTarget {
  host: string;
  port: number;
  source: "explicit" | "cwd" | "single" | "recent" | "default";
  instance?: RegistryEntry;
}

function registryPath(): string {
  return join(homedir(), ".karnellabs-mcp", "registry.json");
}

function canonicalPath(path: string | undefined): string | undefined {
  if (!path) return undefined;
  return normalize(resolve(path)).replace(/[\\/]+$/, "").toLowerCase();
}

function isSameOrChild(child: string, parent: string): boolean {
  return child === parent || child.startsWith(`${parent}/`) || child.startsWith(`${parent}\\`);
}

function heartbeatTime(entry: RegistryEntry): number {
  const value = entry.lastHeartbeat ? Date.parse(entry.lastHeartbeat) : Number.NaN;
  return Number.isFinite(value) ? value : 0;
}

function readActiveInstances(): RegistryEntry[] {
  const path = registryPath();
  if (!existsSync(path)) return [];

  try {
    const entries = JSON.parse(readFileSync(path, "utf-8"));
    if (!Array.isArray(entries)) return [];
    const now = Date.now();
    return entries
      .filter((entry: RegistryEntry) => {
        const heartbeat = heartbeatTime(entry);
        return Number.isInteger(entry.port) && entry.port! > 0 && entry.port! <= 65535 && heartbeat > 0 && now - heartbeat <= REGISTRY_STALE_MS;
      })
      .sort((a: RegistryEntry, b: RegistryEntry) => heartbeatTime(b) - heartbeatTime(a));
  } catch (err) {
    console.error(`[Unity MCP] Failed to read Unity registry ${path}: ${(err as Error).message}`);
    return [];
  }
}

function matchScore(entry: RegistryEntry, cwd: string): number {
  const projectPath = canonicalPath(entry.projectPath);
  if (!projectPath) return 0;
  if (projectPath === cwd) return 3;
  if (isSameOrChild(cwd, projectPath)) return 2;
  if (isSameOrChild(projectPath, cwd)) return 1;
  return 0;
}

export function resolveUnityTarget(options: {
  host?: string;
  explicitPort?: string | number;
  cwd?: string;
} = {}): UnityTarget {
  const host = options.host || process.env.UNITY_WS_HOST || "127.0.0.1";

  if (options.explicitPort !== undefined && options.explicitPort !== "") {
    const port = typeof options.explicitPort === "number" ? options.explicitPort : parseInt(options.explicitPort, 10);
    if (Number.isInteger(port) && port > 0 && port <= 65535) {
      return { host, port, source: "explicit" };
    }
    console.error(`[Unity MCP] Ignoring invalid explicit Unity port: ${options.explicitPort}`);
  }

  const active = readActiveInstances();
  const cwd = canonicalPath(options.cwd || process.cwd());

  if (cwd) {
    const matches = active
      .map(instance => ({ instance, score: matchScore(instance, cwd) }))
      .filter(match => match.score > 0)
      .sort((a, b) => b.score - a.score || heartbeatTime(b.instance) - heartbeatTime(a.instance));

    if (matches.length > 0) {
      const instance = matches[0].instance;
      return { host, port: instance.port!, source: "cwd", instance };
    }
  }

  if (active.length === 1) {
    return { host, port: active[0].port!, source: "single", instance: active[0] };
  }

  if (active.length > 1) {
    const instance = active[0];
    console.error(
      `[Unity MCP] Multiple active Unity instances but none matched cwd '${process.cwd()}'. ` +
      `Using most recent: ${instance.projectName || instance.projectPath || instance.instanceId} on port ${instance.port}. ` +
      `Run 'npx github:karnelian/unity-mcp instances' or pass --port to select explicitly.`
    );
    return { host, port: instance.port!, source: "recent", instance };
  }

  return { host, port: DEFAULT_PORT, source: "default" };
}

export function describeUnityTarget(target: UnityTarget): string {
  const instance = target.instance;
  const project = instance ? `${instance.projectName || "Unity"}${instance.projectPath ? ` (${instance.projectPath})` : ""}` : "no registry match";
  return `${target.host}:${target.port} via ${target.source}${instance ? ` — ${project}` : ""}`;
}
