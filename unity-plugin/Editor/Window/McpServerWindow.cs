using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public class McpServerWindow : EditorWindow
    {
        private int _port;
        private Vector2 _scrollPos;
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
            UpdateMcpJsonConfig();
        }

        private void UpdateMcpJsonConfig()
        {
            _mcpJsonConfig = $@"{{
  ""mcpServers"": {{
    ""unity"": {{
      ""command"": ""node"",
      ""args"": [""{GetServerPath()}/dist/index.js""],
      ""env"": {{
        ""UNITY_WS_PORT"": ""{_port}""
      }}
    }}
  }}
}}";
        }

        private string GetServerPath()
        {
            // 프로젝트 상대 경로로 server 폴더 추정
            return "../server";
        }

        private void OnGUI()
        {
            GUILayout.Space(10);

            // === 연결 상태 ===
            EditorGUILayout.LabelField("Connection Status", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                var statusColor = McpBridge.IsConnected ? Color.green : Color.red;
                var statusText = McpBridge.IsConnected ? "Connected" : "Disconnected";

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

            // === Claude Code 설정 ===
            EditorGUILayout.LabelField("Claude Code Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Add this to your Claude Code mcp.json or claude_desktop_config.json to connect:",
                MessageType.Info
            );

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(120));
            EditorGUILayout.TextArea(_mcpJsonConfig, EditorStyles.textArea);
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Copy to Clipboard"))
            {
                GUIUtility.systemCopyBuffer = _mcpJsonConfig;
                Debug.Log("[KarnelLabs MCP] Configuration copied to clipboard!");
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
            EditorGUILayout.LabelField($"Unity Version: {Application.unityVersion}");
            EditorGUILayout.LabelField($"Project: {Application.productName}");
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
            _logScrollPos = EditorGUILayout.BeginScrollView(_logScrollPos, GUILayout.Height(110));

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

                // 에러 메시지 (있을 경우)
                if (!entry.Success && !string.IsNullOrEmpty(entry.ErrorMessage))
                {
                    var errStyle = new GUIStyle(EditorStyles.miniLabel);
                    errStyle.normal.textColor = new Color(0.9f, 0.5f, 0.2f);
                    EditorGUILayout.LabelField(entry.ErrorMessage, errStyle, GUILayout.MaxWidth(200));
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

        // 실시간 업데이트
        private void OnInspectorUpdate()
        {
            Repaint();
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
            public string ErrorMessage; // null = success
        }

        private static readonly List<Entry> _entries = new();
        private static readonly object _lock = new();

        public static void Add(string method, bool success, string errorMessage = null)
        {
            lock (_lock)
            {
                _entries.Add(new Entry
                {
                    Timestamp = DateTime.Now.ToString("HH:mm:ss"),
                    Method    = method ?? "(unknown)",
                    Success   = success,
                    ErrorMessage = errorMessage,
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
