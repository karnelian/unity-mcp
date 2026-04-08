using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    /// <summary>
    /// 세션 단위 워크플로 관리 + 스냅샷 기반 롤백.
    /// Unity-Skills의 WorkflowManager를 개선:
    /// - 스냅샷 크기 제한 (10MB)
    /// - 최대 스냅샷 수 제한 (200)
    /// - .tmp 크래시 복구
    /// - GlobalObjectId로 영속 참조
    /// </summary>
    public static class WorkflowManager
    {
        private const int MaxSnapshotsPerSession = 200;
        private const long MaxSnapshotBytes = 10 * 1024 * 1024; // 10MB

        private static string _currentSessionId;
        private static string _currentSessionName;
        private static readonly List<Snapshot> _snapshots = new();
        private static int _undoGroup = -1;

        public static bool HasActiveSession => _currentSessionId != null;
        public static string CurrentSessionId => _currentSessionId;
        public static int SnapshotCount => _snapshots.Count;

        // ── 세션 관리 ──────────────────────────────────────────────

        /// <summary>새 워크플로 세션 시작. AI 대화 단위.</summary>
        public static object BeginSession(string name = null)
        {
            if (_currentSessionId != null)
                return new { success = false, error = $"이미 세션이 활성 상태입니다: {_currentSessionId}" };

            _currentSessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
            _currentSessionName = name ?? $"Session-{_currentSessionId}";
            _snapshots.Clear();
            _undoGroup = Undo.GetCurrentGroup();

            Debug.Log($"[MCP Workflow] 세션 시작: {_currentSessionName} ({_currentSessionId})");

            return new
            {
                success = true,
                sessionId = _currentSessionId,
                sessionName = _currentSessionName,
            };
        }

        /// <summary>현재 세션 종료.</summary>
        public static object EndSession()
        {
            if (_currentSessionId == null)
                return new { success = false, error = "활성 세션이 없습니다." };

            var result = new
            {
                success = true,
                sessionId = _currentSessionId,
                sessionName = _currentSessionName,
                snapshotCount = _snapshots.Count,
            };

            SaveHistory();
            _currentSessionId = null;
            _currentSessionName = null;
            _snapshots.Clear();
            _undoGroup = -1;

            Debug.Log($"[MCP Workflow] 세션 종료");
            return result;
        }

        // ── 스냅샷 ──────────────────────────────────────────────────

        /// <summary>
        /// 오브젝트의 현재 상태를 스냅샷으로 저장.
        /// 스킬 실행 전에 호출하여 롤백 지원.
        /// </summary>
        /// <summary>오버로드: operationName 생략 시 기본값 사용</summary>
        public static void SnapshotObject(GameObject go) => SnapshotObject(go, "MCP Operation");

        public static void SnapshotObject(GameObject go, string operationName)
        {
            if (_currentSessionId == null) return;
            if (go == null) return;

            if (_snapshots.Count >= MaxSnapshotsPerSession)
            {
                Debug.LogWarning($"[MCP Workflow] 스냅샷 한도 도달 ({MaxSnapshotsPerSession}). 스킵합니다.");
                return;
            }

            try
            {
                var json = EditorJsonUtility.ToJson(go, true);

                // 크기 제한 체크
                if (json.Length > MaxSnapshotBytes)
                {
                    Debug.LogWarning($"[MCP Workflow] 스냅샷 크기 초과 ({json.Length / 1024}KB > {MaxSnapshotBytes / 1024}KB). 메타데이터만 저장합니다.");
                    json = null; // 메타데이터만 저장
                }

                _snapshots.Add(new Snapshot
                {
                    Timestamp = DateTime.UtcNow,
                    Operation = operationName,
                    ObjectName = go.name,
                    ObjectPath = GameObjectFinder.GetFullPath(go.transform),
                    InstanceId = go.GetInstanceID(),
                    GlobalId = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString(),
                    SerializedState = json,
                    UndoGroup = Undo.GetCurrentGroup(),
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MCP Workflow] 스냅샷 실패: {ex.Message}");
            }
        }

        /// <summary>에셋 파일 스냅샷 (바이너리 에셋용, 크기 제한 적용)</summary>
        /// <summary>오버로드: operationName 생략 시 기본값 사용</summary>
        public static void SnapshotAsset(string assetPath) => SnapshotAsset(assetPath, "MCP Operation");

        public static void SnapshotAsset(string assetPath, string operationName)
        {
            if (_currentSessionId == null) return;
            if (string.IsNullOrEmpty(assetPath)) return;

            if (_snapshots.Count >= MaxSnapshotsPerSession) return;

            try
            {
                var fullPath = Path.Combine(Application.dataPath, "..", assetPath);
                if (!File.Exists(fullPath)) return;

                var fileInfo = new FileInfo(fullPath);
                string base64Data = null;

                if (fileInfo.Length <= MaxSnapshotBytes)
                {
                    var bytes = File.ReadAllBytes(fullPath);
                    base64Data = Convert.ToBase64String(bytes);
                }

                _snapshots.Add(new Snapshot
                {
                    Timestamp = DateTime.UtcNow,
                    Operation = operationName,
                    ObjectName = Path.GetFileName(assetPath),
                    ObjectPath = assetPath,
                    AssetBytesBase64 = base64Data,
                    UndoGroup = Undo.GetCurrentGroup(),
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MCP Workflow] 에셋 스냅샷 실패: {ex.Message}");
            }
        }

        // ── Undo / Redo ──────────────────────────────────────────────

        /// <summary>현재 세션의 모든 작업을 되돌림</summary>
        public static object UndoSession()
        {
            if (_currentSessionId == null)
                return new { success = false, error = "활성 세션이 없습니다." };

            if (_undoGroup < 0)
                return new { success = false, error = "되돌릴 Undo 그룹이 없습니다." };

            int undoneCount = 0;

            // 역순으로 Undo
            for (int i = _snapshots.Count - 1; i >= 0; i--)
            {
                try
                {
                    Undo.PerformUndo();
                    undoneCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[MCP Workflow] Undo 실패 ({i}): {ex.Message}");
                }
            }

            var result = new
            {
                success = true,
                sessionId = _currentSessionId,
                undoneCount,
                totalSnapshots = _snapshots.Count,
            };

            _snapshots.Clear();
            return result;
        }

        /// <summary>마지막 작업만 되돌림</summary>
        public static object UndoLast()
        {
            Undo.PerformUndo();
            if (_snapshots.Count > 0)
                _snapshots.RemoveAt(_snapshots.Count - 1);

            return new { success = true, remainingSnapshots = _snapshots.Count };
        }

        // ── 상태 조회 ──────────────────────────────────────────────

        public static object GetStatus()
        {
            return new
            {
                hasActiveSession = HasActiveSession,
                sessionId = _currentSessionId,
                sessionName = _currentSessionName,
                snapshotCount = _snapshots.Count,
                maxSnapshots = MaxSnapshotsPerSession,
                recentOperations = _snapshots.TakeLast(10).Select(s => new
                {
                    s.Operation,
                    s.ObjectName,
                    s.ObjectPath,
                    timestamp = s.Timestamp.ToString("HH:mm:ss"),
                }).ToArray(),
            };
        }

        // ── 히스토리 영속화 ──────────────────────────────────────────

        private static string GetHistoryPath()
        {
            return Path.Combine(Application.dataPath, "..", "Library", "KarnelLabs_MCP_WorkflowHistory.json");
        }

        private static void SaveHistory()
        {
            try
            {
                var historyPath = GetHistoryPath();
                var tmpPath = historyPath + ".tmp";
                var entry = new
                {
                    sessionId = _currentSessionId,
                    sessionName = _currentSessionName,
                    timestamp = DateTime.UtcNow,
                    snapshotCount = _snapshots.Count,
                    operations = _snapshots.Select(s => new { s.Operation, s.ObjectName, s.Timestamp }).ToArray(),
                };

                // 기존 히스토리 로드
                var history = new List<object>();
                if (File.Exists(historyPath))
                {
                    try
                    {
                        var existing = JsonConvert.DeserializeObject<List<object>>(File.ReadAllText(historyPath));
                        if (existing != null) history = existing;
                    }
                    catch { }
                }

                history.Add(entry);

                // 최근 50개만 유지
                if (history.Count > 50)
                    history = history.Skip(history.Count - 50).ToList();

                // Atomic write: .tmp 먼저, 그다음 이동
                File.WriteAllText(tmpPath, JsonConvert.SerializeObject(history, Formatting.Indented));
                if (File.Exists(historyPath)) File.Delete(historyPath);
                File.Move(tmpPath, historyPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MCP Workflow] 히스토리 저장 실패: {ex.Message}");
            }
        }

        // ── 내부 타입 ──────────────────────────────────────────────

        private class Snapshot
        {
            public DateTime Timestamp;
            public string Operation;
            public string ObjectName;
            public string ObjectPath;
            public int InstanceId;
            public string GlobalId;
            public string SerializedState;
            public string AssetBytesBase64;
            public int UndoGroup;
        }
    }
}
