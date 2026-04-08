using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KarnelLabs.MCP
{
    /// <summary>
    /// 다중 Unity 인스턴스 자동 발견 서비스.
    /// ~/.karnellabs-mcp/registry.json에 인스턴스 등록/해제.
    /// 파일 락 + .tmp 크래시 복구 + stale entry 정리.
    /// </summary>
    [InitializeOnLoad]
    public static class RegistryService
    {
        private const string RegistryDir = ".karnellabs-mcp";
        private const string RegistryFile = "registry.json";
        private const int StaleSeconds = 120;
        private const int MaxRetries = 5;

        private static string _instanceId;
        private static double _lastHeartbeat;

        static RegistryService()
        {
            _instanceId = ComputeInstanceId(Application.dataPath);
            EditorApplication.update += Heartbeat;
            EditorApplication.quitting += Unregister;
            RegisterInstance();
        }

        /// <summary>SHA256 해시 기반 안정적 인스턴스 ID (프로세스 간 일관성)</summary>
        private static string ComputeInstanceId(string projectPath)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(projectPath));
            return BitConverter.ToString(hash, 0, 4).Replace("-", "").ToLower();
        }

        private static string GetRegistryPath()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var dir = Path.Combine(home, RegistryDir);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, RegistryFile);
        }

        public static void RegisterInstance()
        {
            ModifyRegistry(entries =>
            {
                // 기존 엔트리 제거 후 갱신
                entries.RemoveAll(e => e.InstanceId == _instanceId);
                entries.Add(new RegistryEntry
                {
                    InstanceId = _instanceId,
                    Port = McpBridge.Port,
                    Pid = Process.GetCurrentProcess().Id,
                    ProjectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..")),
                    ProjectName = Application.productName,
                    UnityVersion = Application.unityVersion,
                    LastHeartbeat = DateTime.UtcNow,
                });
            });
            _lastHeartbeat = EditorApplication.timeSinceStartup;
            Debug.Log($"[KarnelLabs MCP] Instance registered: {_instanceId} (port {McpBridge.Port})");
        }

        public static void Unregister()
        {
            ModifyRegistry(entries =>
            {
                entries.RemoveAll(e => e.InstanceId == _instanceId);
            });
        }

        private static void Heartbeat()
        {
            // 30초마다 하트비트
            if (EditorApplication.timeSinceStartup - _lastHeartbeat < 30) return;
            _lastHeartbeat = EditorApplication.timeSinceStartup;

            ModifyRegistry(entries =>
            {
                var mine = entries.FirstOrDefault(e => e.InstanceId == _instanceId);
                if (mine != null)
                {
                    mine.LastHeartbeat = DateTime.UtcNow;
                    mine.Port = McpBridge.Port; // 포트 변경 반영
                }
                else
                {
                    RegisterInstance(); // 엔트리가 사라졌으면 재등록
                }
            });
        }

        /// <summary>등록된 모든 인스턴스 조회 (MCP 리소스용)</summary>
        public static List<RegistryEntry> GetInstances()
        {
            var path = GetRegistryPath();
            if (!File.Exists(path)) return new List<RegistryEntry>();

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<List<RegistryEntry>>(json) ?? new List<RegistryEntry>();
            }
            catch
            {
                return new List<RegistryEntry>();
            }
        }

        /// <summary>파일 락 + atomic write 기반 레지스트리 수정</summary>
        private static void ModifyRegistry(Action<List<RegistryEntry>> modifier)
        {
            var path = GetRegistryPath();
            var tmpPath = path + ".tmp";

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                try
                {
                    var entries = new List<RegistryEntry>();

                    // 읽기
                    if (File.Exists(path))
                    {
                        try
                        {
                            var json = File.ReadAllText(path);
                            entries = JsonConvert.DeserializeObject<List<RegistryEntry>>(json) ?? new List<RegistryEntry>();
                        }
                        catch { entries = new List<RegistryEntry>(); }
                    }
                    else if (File.Exists(tmpPath))
                    {
                        // 크래시 복구
                        try
                        {
                            var json = File.ReadAllText(tmpPath);
                            entries = JsonConvert.DeserializeObject<List<RegistryEntry>>(json) ?? new List<RegistryEntry>();
                        }
                        catch { entries = new List<RegistryEntry>(); }
                    }

                    // Stale 엔트리 정리
                    var now = DateTime.UtcNow;
                    entries.RemoveAll(e =>
                    {
                        if ((now - e.LastHeartbeat).TotalSeconds > StaleSeconds) return true;
                        try { Process.GetProcessById(e.Pid); return false; }
                        catch { return true; } // PID 죽은 프로세스
                    });

                    // 수정 적용
                    modifier(entries);

                    // Atomic write
                    File.WriteAllText(tmpPath, JsonConvert.SerializeObject(entries, Formatting.Indented));
                    if (File.Exists(path)) File.Delete(path);
                    File.Move(tmpPath, path);
                    return;
                }
                catch (IOException) when (attempt < MaxRetries - 1)
                {
                    // 파일 락 충돌 — 지수 백오프
                    System.Threading.Thread.Sleep((int)Math.Pow(2, attempt) * 50);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[MCP Registry] Error: {ex.Message}");
                    return;
                }
            }
        }

        [Serializable]
        public class RegistryEntry
        {
            [JsonProperty("instanceId")] public string InstanceId;
            [JsonProperty("port")] public int Port;
            [JsonProperty("pid")] public int Pid;
            [JsonProperty("projectPath")] public string ProjectPath;
            [JsonProperty("projectName")] public string ProjectName;
            [JsonProperty("unityVersion")] public string UnityVersion;
            [JsonProperty("lastHeartbeat")] public DateTime LastHeartbeat;
        }
    }
}
