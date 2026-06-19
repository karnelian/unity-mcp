using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public class McpServerWindow : EditorWindow
    {
        private const string HttpPortPrefsKey = "KarnelLabs.MCP.HttpPort";
        private const string HttpAutoStartPrefsKey = "KarnelLabs.MCP.HttpAutoStart";

        private int _port;
        private int _httpPort;
        private bool _httpAutoStart;
        private Vector2 _windowScrollPos;
        private Vector2 _configScrollPos;
        private Vector2 _logScrollPos;
        private string _mcpJsonConfig;

        [MenuItem("Tools/KarnelLabs MCP/Server Window")]
        public static void ShowWindow()
        {
            GetWindow<McpServerWindow>("KarnelLabs MCP");
        }

        private void OnEnable()
        {
            _port = McpBridge.Port;
            _httpPort = EditorPrefs.GetInt(HttpPortPrefsKey, 8765);
            _httpAutoStart = EditorPrefs.GetBool(HttpAutoStartPrefsKey, false);
            HttpMcpServerLauncher.Configure(_httpPort, _httpAutoStart);
            UpdateMcpJsonConfig();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void UpdateMcpJsonConfig()
        {
            _mcpJsonConfig = $@"Claude Code plugin users usually do not need project .mcp.json.

Explicit stdio .mcp.json:
{{
  ""mcpServers"": {{
    ""karnellabs-unity-mcp"": {{
      ""command"": ""npx"",
      ""args"": [""-y"", ""github:karnelian/unity-mcp"", ""--profile=core""],
      ""env"": {{
        ""UNITY_WS_PORT"": ""{_port}""
      }}
    }}
  }}
}}

HTTP client config:
{{
  ""mcpServers"": {{
    ""karnellabs-unity-mcp"": {{
      ""url"": ""http://127.0.0.1:{_httpPort}/mcp"",
      ""transport"": ""http""
    }}
  }}
}}

SSE client config:
{{
  ""mcpServers"": {{
    ""karnellabs-unity-mcp"": {{
      ""url"": ""http://127.0.0.1:{_httpPort}/sse"",
      ""transport"": ""sse""
    }}
  }}
}}";
        }

        private void OnGUI()
        {
            _windowScrollPos = EditorGUILayout.BeginScrollView(_windowScrollPos);
            GUILayout.Space(10);

            // === 연결 상태 ===
            EditorGUILayout.LabelField("Connection Status", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                var activeSocket = McpBridge.HasActiveClient;
                var recentlyActive = McpBridge.IsRecentlyActive;
                var statusColor = activeSocket ? Color.green : recentlyActive ? new Color(0.9f, 0.75f, 0.2f) : McpBridge.IsRunning ? new Color(0.4f, 0.7f, 1f) : Color.red;
                var statusText = activeSocket ? "Connected" : recentlyActive ? "Recently Active" : McpBridge.IsRunning ? "Listening" : "Stopped";

                var style = new GUIStyle(EditorStyles.label);
                style.normal.textColor = statusColor;
                EditorGUILayout.LabelField(statusText, style);

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"Handlers: {McpBridge.HandlerCount}", GUILayout.Width(120));
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            DrawSeparator();

            // === 포트 설정 ===
            EditorGUILayout.LabelField("Server Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                _port = EditorGUILayout.IntField("Port", _port);
                if (GUILayout.Button("Apply", GUILayout.Width(60)))
                {
                    McpBridge.SetPort(_port);
                    UpdateMcpJsonConfig();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Start Server"))
                    McpBridge.Start();
                if (GUILayout.Button("Stop Server"))
                    McpBridge.Stop();
                if (GUILayout.Button("Restart"))
                {
                    McpBridge.Stop();
                    McpBridge.Start();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            DrawSeparator();

            DrawHttpMcpServerControls();

            GUILayout.Space(10);
            DrawSeparator();

            // === Claude Code 설정 ===
            EditorGUILayout.LabelField("Claude Code Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Claude Code plugin users usually need no local config. Use setup --mcp-config or setup --mcp-transport=http|sse to generate one automatically; this box is for copying/debugging.",
                MessageType.Info
            );

            _configScrollPos = EditorGUILayout.BeginScrollView(_configScrollPos, GUILayout.Height(220));
            EditorGUILayout.TextArea(_mcpJsonConfig, EditorStyles.textArea);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Copy to Clipboard"))
            {
                GUIUtility.systemCopyBuffer = _mcpJsonConfig;
                UnityEngine.Debug.Log("[KarnelLabs MCP] Configuration copied to clipboard!");
            }

            GUILayout.Space(10);
            DrawSeparator();

            // === 최근 요청 로그 ===
            DrawRequestLog();

            GUILayout.Space(10);
            DrawSeparator();

            // === 정보 ===
            EditorGUILayout.LabelField("Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"WebSocket: ws://127.0.0.1:{McpBridge.Port}");
            EditorGUILayout.LabelField($"Connected: {(McpBridge.IsConnected ? "Yes" : "No")}");
            EditorGUILayout.LabelField($"Active Socket: {(McpBridge.HasActiveClient ? "Yes" : "No")}");
            EditorGUILayout.LabelField($"Client: {McpBridge.ClientEndpoint}");
            var lastActivity = McpBridge.LastClientActivityUtc;
            EditorGUILayout.LabelField($"Last Activity: {(lastActivity.HasValue ? lastActivity.Value.ToLocalTime().ToString("HH:mm:ss") : "-")}");
            EditorGUILayout.LabelField($"Unity Version: {Application.unityVersion}");
            EditorGUILayout.LabelField($"Project: {Application.productName}");
            EditorGUILayout.EndScrollView();
        }

        private void DrawHttpMcpServerControls()
        {
            EditorGUILayout.LabelField("HTTP MCP Server", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "For AgentManager or URL-based MCP clients. Unity can launch the Node HTTP MCP server for you, so you do not have to run npx manually after reboot.",
                MessageType.Info
            );

            EditorGUILayout.BeginHorizontal();
            {
                var newPort = EditorGUILayout.IntField("HTTP Port", _httpPort);
                if (newPort != _httpPort && newPort > 0)
                {
                    _httpPort = newPort;
                    EditorPrefs.SetInt(HttpPortPrefsKey, _httpPort);
                    HttpMcpServerLauncher.Configure(_httpPort, _httpAutoStart);
                    UpdateMcpJsonConfig();
                }

                var statusStyle = new GUIStyle(EditorStyles.label);
                statusStyle.normal.textColor = HttpMcpServerLauncher.IsRunning ? Color.green : new Color(0.9f, 0.5f, 0.2f);
                EditorGUILayout.LabelField(HttpMcpServerLauncher.IsRunning ? "Running" : "Stopped", statusStyle, GUILayout.Width(80));
            }
            EditorGUILayout.EndHorizontal();

            var nextAutoStart = EditorGUILayout.ToggleLeft("Auto-start HTTP MCP server when Unity Editor opens", _httpAutoStart);
            if (nextAutoStart != _httpAutoStart)
            {
                _httpAutoStart = nextAutoStart;
                EditorPrefs.SetBool(HttpAutoStartPrefsKey, _httpAutoStart);
                HttpMcpServerLauncher.Configure(_httpPort, _httpAutoStart);
                if (_httpAutoStart) HttpMcpServerLauncher.Start();
            }

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Start HTTP MCP"))
                    HttpMcpServerLauncher.Start();
                if (GUILayout.Button("Stop HTTP MCP"))
                    HttpMcpServerLauncher.Stop();
                if (GUILayout.Button("Copy HTTP URL"))
                {
                    GUIUtility.systemCopyBuffer = $"http://127.0.0.1:{_httpPort}/mcp";
                    UnityEngine.Debug.Log("[KarnelLabs MCP] HTTP MCP URL copied to clipboard.");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"MCP URL: http://127.0.0.1:{_httpPort}/mcp");
        }

        private void DrawRequestLog()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Recent Requests (last 20)", EditorStyles.boldLabel);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
                RequestLog.Clear();
            EditorGUILayout.EndHorizontal();

            var entries = RequestLog.GetAll();
            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No requests yet.", MessageType.None);
                return;
            }

            // 로그 스크롤 뷰: 최대 5줄 높이
            _logScrollPos = EditorGUILayout.BeginScrollView(_logScrollPos, GUILayout.MinHeight(110), GUILayout.MaxHeight(220));

            // 최신 항목을 위쪽에 표시
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var entry = entries[i];
                EditorGUILayout.BeginHorizontal();

                // 타임스탬프
                EditorGUILayout.LabelField(entry.Timestamp, GUILayout.Width(60));

                // 성공/실패 아이콘
                var iconStyle = new GUIStyle(EditorStyles.label);
                iconStyle.normal.textColor = entry.Success ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.9f, 0.3f, 0.3f);
                EditorGUILayout.LabelField(entry.Success ? "OK" : "ERR", iconStyle, GUILayout.Width(32));

                // 메서드 이름
                EditorGUILayout.LabelField(entry.Method, GUILayout.ExpandWidth(true));

                EditorGUILayout.LabelField(entry.RiskLevel ?? "-", EditorStyles.miniLabel, GUILayout.Width(46));
                EditorGUILayout.LabelField($"{entry.DurationMs}ms", EditorStyles.miniLabel, GUILayout.Width(54));
                EditorGUILayout.LabelField($"req {FormatBytes(entry.RequestBytes)}", EditorStyles.miniLabel, GUILayout.Width(70));
                EditorGUILayout.LabelField($"res {FormatBytes(entry.ResponseBytes)}", EditorStyles.miniLabel, GUILayout.Width(70));

                if (!entry.Success && entry.ErrorCode.HasValue)
                {
                    var codeStyle = new GUIStyle(EditorStyles.miniLabel);
                    codeStyle.normal.textColor = new Color(0.9f, 0.5f, 0.2f);
                    var codeText = $"{entry.CodeName ?? SafetyPolicy.CodeName(entry.ErrorCode.Value)} ({entry.ErrorCode.Value})";
                    EditorGUILayout.LabelField(new GUIContent(codeText, codeText), codeStyle, GUILayout.MinWidth(220), GUILayout.MaxWidth(320));
                }

                // 에러 메시지 (있을 경우)
                if (!entry.Success && !string.IsNullOrEmpty(entry.ErrorMessage))
                {
                    var errStyle = new GUIStyle(EditorStyles.miniLabel);
                    errStyle.normal.textColor = new Color(0.9f, 0.5f, 0.2f);
                    EditorGUILayout.LabelField(new GUIContent(entry.ErrorMessage, entry.ErrorMessage), errStyle, GUILayout.MinWidth(180), GUILayout.MaxWidth(320));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            GUILayout.Space(5);
        }

        private static string FormatBytes(int bytes)
        {
            if (bytes <= 0) return "0B";
            if (bytes < 1024) return $"{bytes}B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024f:F1}KB";
            return $"{bytes / (1024f * 1024f):F1}MB";
        }

        // 실시간 업데이트
        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void OnEditorUpdate()
        {
            if (_httpPort != HttpMcpServerLauncher.Port)
            {
                _httpPort = HttpMcpServerLauncher.Port;
                UpdateMcpJsonConfig();
            }
            Repaint();
        }
    }

    [InitializeOnLoad]
    public static class HttpMcpServerLauncher
    {
        private const string HttpPortPrefsKey = "KarnelLabs.MCP.HttpPort";
        private const string HttpAutoStartPrefsKey = "KarnelLabs.MCP.HttpAutoStart";
        private static Process _process;
        private static int _port;
        private static bool _autoStart;

        public static int Port => _port;
        public static bool IsRunning => _process != null && !_process.HasExited;

        static HttpMcpServerLauncher()
        {
            _port = EditorPrefs.GetInt(HttpPortPrefsKey, 8765);
            _autoStart = EditorPrefs.GetBool(HttpAutoStartPrefsKey, false);
            EditorApplication.delayCall += () =>
            {
                if (_autoStart) Start();
            };
            EditorApplication.quitting += Stop;
        }

        public static void Configure(int port, bool autoStart)
        {
            _port = port > 0 ? port : 8765;
            _autoStart = autoStart;
        }

        public static void Start()
        {
            if (IsRunning)
            {
                UnityEngine.Debug.Log($"[KarnelLabs MCP] HTTP MCP server is already running at http://127.0.0.1:{_port}/mcp");
                return;
            }

            var executable = Application.platform == RuntimePlatform.WindowsEditor ? "npx.cmd" : "npx";
            var requestedPort = _port;
            int selectedPort;
            try
            {
                selectedPort = ResolveAvailablePort(requestedPort);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[KarnelLabs MCP] Failed to find an available HTTP MCP port: {ex.Message}");
                return;
            }
            if (selectedPort != requestedPort)
            {
                UnityEngine.Debug.LogWarning($"[KarnelLabs MCP] HTTP port {requestedPort} is busy; using {selectedPort} instead.");
                _port = selectedPort;
                EditorPrefs.SetInt(HttpPortPrefsKey, _port);
            }
            var args = $"-y github:karnelian/unity-mcp --transport=http --mcp-port={_port} --profile=core";

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = args,
                    WorkingDirectory = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
                _process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.Log($"[KarnelLabs MCP HTTP] {e.Data}");
                };
                _process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data)) UnityEngine.Debug.Log($"[KarnelLabs MCP HTTP] {e.Data}");
                };
                _process.Exited += (_, _) => UnityEngine.Debug.Log("[KarnelLabs MCP] HTTP MCP server process exited.");

                _process.Start();
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                UnityEngine.Debug.Log($"[KarnelLabs MCP] Started HTTP MCP server at http://127.0.0.1:{_port}/mcp");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[KarnelLabs MCP] Failed to start HTTP MCP server via '{executable} {args}': {ex.Message}");
                _process = null;
            }
        }

        private static int ResolveAvailablePort(int preferredPort)
        {
            var startPort = preferredPort > 0 ? preferredPort : 8765;
            const int maxAttempts = 100;

            for (var offset = 0; offset < maxAttempts; offset++)
            {
                var candidate = startPort + offset;
                if (candidate > 65535) break;
                if (IsTcpPortAvailable(candidate)) return candidate;
            }

            throw new InvalidOperationException($"No available HTTP MCP port found in range {startPort}-{Math.Min(65535, startPort + maxAttempts - 1)}.");
        }

        private static bool IsTcpPortAvailable(int port)
        {
            TcpListener listener = null;
            try
            {
                listener = new TcpListener(IPAddress.Loopback, port);
                listener.Start();
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
            finally
            {
                listener?.Stop();
            }
        }

        public static void Stop()
        {
            if (!IsRunning) return;

            try
            {
                _process.Kill();
                _process.Dispose();
                UnityEngine.Debug.Log("[KarnelLabs MCP] Stopped HTTP MCP server.");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"[KarnelLabs MCP] Failed to stop HTTP MCP server: {ex.Message}");
            }
            finally
            {
                _process = null;
            }
        }
    }

}

// ─── Request Log (McpServerWindow 전용 정적 저장소) ─────────────────────────

namespace KarnelLabs.MCP
{
    /// <summary>
    /// 최근 JSON-RPC 요청 로그 (최대 20개).
    /// McpBridge.ProcessMessageQueue에서 호출.
    /// </summary>
    public static class RequestLog
    {
        private const int MaxEntries = 20;

        public struct Entry
        {
            public string Timestamp;
            public string Method;
            public bool Success;
            public int? ErrorCode;
            public string CodeName;
            public string RiskLevel;
            public string ErrorMessage; // null = success
            public long DurationMs;
            public int RequestBytes;
            public int ResponseBytes;
        }

        private static readonly List<Entry> _entries = new();
        private static readonly object _lock = new();

        public static void Add(string method, bool success, string errorMessage = null, long durationMs = 0, int requestBytes = 0, int responseBytes = 0, int? errorCode = null, string codeName = null, string riskLevel = null)
        {
            lock (_lock)
            {
                var risk = SafetyPolicy.Describe(method);
                _entries.Add(new Entry
                {
                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    Method    = method ?? "(unknown)",
                    Success   = success,
                    ErrorCode = errorCode,
                    CodeName = codeName ?? (errorCode.HasValue ? SafetyPolicy.CodeName(errorCode.Value) : null),
                    RiskLevel = riskLevel ?? risk.RiskLevel,
                    ErrorMessage = errorMessage,
                    DurationMs = durationMs,
                    RequestBytes = requestBytes,
                    ResponseBytes = responseBytes,
                });

                // 오래된 항목 제거
                while (_entries.Count > MaxEntries)
                    _entries.RemoveAt(0);
            }
        }

        public static List<Entry> GetAll()
        {
            lock (_lock)
            {
                return new List<Entry>(_entries);
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
            }
        }
    }
}
