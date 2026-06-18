import WebSocket from "ws";

/**
 * JSON-RPC error codes (design doc Section 7):
 *   -32001  Unity Editor not connected
 *   -32002  Request timeout
 *   -32003  GameObject not found  (thrown by Unity C# side)
 *   -32000  Generic Unity API exception
 *   -32601  Unknown method
 *   -32602  Invalid params
 */
export class McpError extends Error {
  constructor(
    public readonly code: number,
    message: string,
    public readonly data?: unknown
  ) {
    super(message);
    this.name = "McpError";
  }
}

export interface BridgeConfig {
  host: string;
  port: number;
  resolvePort?: () => number | Promise<number>;
  timeouts: {
    default: number;
    build: number;
  };
  reconnect: {
    interval: number;
    maxInterval?: number;
    maxAttempts: number;
  };
}

interface PendingRequest {
  resolve: (value: any) => void;
  reject: (reason: any) => void;
  timer: ReturnType<typeof setTimeout>;
}

const LONG_RUNNING_METHODS = new Set([
  "editor.build",
  "editor.runTests",
  "asset.import",
  "asset.refresh",
]);

export class UnityBridge {
  private ws: WebSocket | null = null;
  private config: BridgeConfig;
  private requestId = 0;
  private pending = new Map<number, PendingRequest>();
  private _connected = false;
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;
  private shouldReconnect = true;
  private reconnectAttempts = 0;
  private connecting = false;

  constructor(config: BridgeConfig) {
    this.config = config;
    void this.connect();
  }

  get connected(): boolean {
    return this._connected;
  }

  async request(method: string, params: Record<string, any> = {}): Promise<any> {
    if (!this._connected || !this.ws) {
      throw new McpError(
        -32001,
        "Unity Editor not connected. Checklist: (1) Open Unity project, (2) Tools > KarnelLabs MCP > Server Window — ensure it shows 'Listening', (3) Check port " + this.config.port + " matches.",
        {
          codeName: "UNITY_NOT_CONNECTED",
          retryable: true,
          suggestedNextTool: "Open Tools > KarnelLabs MCP > Server Window, confirm it is Listening, then call unity_editor_diagnostics or unity_project_health.",
        }
      );
    }

    this.requestId = (this.requestId + 1) % Number.MAX_SAFE_INTEGER;
    const id = this.requestId;
    const timeout = LONG_RUNNING_METHODS.has(method)
      ? this.config.timeouts.build
      : this.config.timeouts.default;

    return new Promise((resolve, reject) => {
      const timer = setTimeout(() => {
        this.pending.delete(id);
        reject(new McpError(-32002, `Timeout: ${method} after ${timeout / 1000}s. Unity may be busy (compiling, importing, or in a modal dialog). Try again after Unity is idle.`, {
          codeName: "UNITY_REQUEST_TIMEOUT",
          retryable: true,
          method,
          suggestedNextTool: "Wait for Unity to finish compiling/importing, then call unity_editor_diagnostics or retry with a narrower request.",
        }));
      }, timeout);

      this.pending.set(id, { resolve, reject, timer });

      this.ws!.send(
        JSON.stringify({
          jsonrpc: "2.0",
          id,
          method,
          params,
        })
      );
    });
  }

  getStatus(): { connected: boolean; host: string; port: number } {
    return {
      connected: this._connected,
      host: this.config.host,
      port: this.config.port,
    };
  }

  dispose(): void {
    this.shouldReconnect = false;
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }
    if (this.ws) {
      this.ws.close();
      this.ws = null;
    }
    this.rejectAllPending("Bridge disposed");
  }

  private async connect(): Promise<void> {
    if (this.connecting || !this.shouldReconnect) return;
    this.connecting = true;

    try {
      if (this.config.resolvePort) {
        const nextPort = await this.config.resolvePort();
        if (Number.isInteger(nextPort) && nextPort > 0 && nextPort <= 65535 && nextPort !== this.config.port) {
          console.error(`[Unity Bridge] Retargeting Unity port ${this.config.port} → ${nextPort}`);
          this.config.port = nextPort;
        }
      }

      const url = `ws://${this.config.host}:${this.config.port}`;
      this.ws = new WebSocket(url);

      this.ws.on("open", () => {
        this._connected = true;
        this.connecting = false;
        this.reconnectAttempts = 0;
        console.error(`[Unity Bridge] Connected to ${url}`);
      });

      this.ws.on("message", (data: WebSocket.Data) => {
        this.onMessage(data.toString());
      });

      this.ws.on("close", (code: number, reason: Buffer) => {
        this.onClose(code, reason.toString());
      });

      this.ws.on("error", (err: Error) => {
        if (!this._connected) {
          this.connecting = false;
          console.error(`[Unity Bridge] Connection failed: ${err.message}`);
        }
      });
    } catch (err) {
      this.connecting = false;
      console.error(`[Unity Bridge] Failed before connect: ${(err as Error).message}`);
      this.scheduleReconnect();
    }
  }

  private onMessage(data: string): void {
    try {
      const msg = JSON.parse(data);
      const pending = this.pending.get(msg.id);
      if (pending) {
        clearTimeout(pending.timer);
        this.pending.delete(msg.id);
        if (msg.error) {
          // Preserve the JSON-RPC error code from Unity (-32001/-32002/-32003/-32000)
          pending.reject(
            new McpError(
              msg.error.code ?? -32000,
              msg.error.message || "Unity error",
              msg.error.data
            )
          );
        } else {
          pending.resolve(msg.result);
        }
      }
    } catch (err) {
      console.error("[Unity Bridge] Failed to parse message:", err);
    }
  }

  private onClose(code?: number, reason?: string): void {
    const wasConnected = this._connected;
    this._connected = false;
    this.connecting = false;
    this.ws = null;

    this.rejectAllPending("연결 끊김");

    if (wasConnected) {
      const detail = code ? ` (code ${code}${reason ? `: ${reason}` : ""})` : "";
      console.error(`[Unity Bridge] Disconnected from Unity${detail}`);
    } else if (code || reason) {
      console.error(`[Unity Bridge] Unity connection closed before ready${code ? ` (code ${code}${reason ? `: ${reason}` : ""})` : ""}`);
    }

    if (this.shouldReconnect) {
      this.scheduleReconnect();
    }
  }

  private rejectAllPending(reason: string): void {
    for (const [id, pending] of this.pending) {
      clearTimeout(pending.timer);
      pending.reject(new Error(reason));
    }
    this.pending.clear();
  }

  private scheduleReconnect(): void {
    if (this.reconnectTimer || !this.shouldReconnect) return;
    if (this.reconnectAttempts >= this.config.reconnect.maxAttempts) return;

    this.reconnectAttempts += 1;
    const base = this.config.reconnect.interval;
    const max = this.config.reconnect.maxInterval ?? base;
    const delay = Math.min(max, base * 2 ** Math.max(0, this.reconnectAttempts - 1));

    this.reconnectTimer = setTimeout(() => {
      this.reconnectTimer = null;
      void this.connect();
    }, delay);
  }
}
