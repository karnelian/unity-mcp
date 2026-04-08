using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class ScriptHandler
    {
        private static readonly Dictionary<string, string> Templates = new()
        {
            ["MonoBehaviour"] = "using UnityEngine;\n\npublic class {NAME} : MonoBehaviour\n{\n    void Start()\n    {\n    }\n\n    void Update()\n    {\n    }\n}\n",
            ["ScriptableObject"] = "using UnityEngine;\n\n[CreateAssetMenu]\npublic class {NAME} : ScriptableObject\n{\n}\n",
            ["Editor"] = "using UnityEditor;\nusing UnityEngine;\n\n[CustomEditor(typeof({NAME}))]\npublic class {NAME}Editor : Editor\n{\n    public override void OnInspectorGUI()\n    {\n    }\n}\n",
            ["Interface"] = "public interface I{NAME}\n{\n}\n",
            ["Struct"] = "using System;\n\n[Serializable]\npublic struct {NAME}\n{\n}\n",
            ["Enum"] = "public enum {NAME}\n{\n}\n",
            ["Static"] = "public static class {NAME}\n{\n}\n",
        };

        public static void Register()
        {
            CommandRouter.Register("script.create", CreateScript);
            CommandRouter.Register("script.read", ReadScript);
            CommandRouter.Register("script.edit", EditScript);
            CommandRouter.Register("script.compileCheck", CompileCheck);
            CommandRouter.Register("script.list", ListScripts);
            CommandRouter.Register("script.delete", DeleteScript);
            CommandRouter.Register("script.rename", RenameScript);
            CommandRouter.Register("script.search", SearchInScripts);
            CommandRouter.Register("script.getInfo", GetScriptInfo);
        }

        private static string ResolvePath(string assetPath)
        {
            return Validate.SafeFilePath(assetPath, "path");
        }

        private static object WithCompilationInfo(object result, bool isScriptMutation)
        {
            if (!isScriptMutation) return result;
            bool isCompiling = EditorApplication.isCompiling;
            bool hasErrors = EditorUtility.scriptCompilationFailed;
            string nextAction = isCompiling
                ? "스크립트가 컴파일 중입니다. 잠시 후 script.compileCheck로 결과를 확인하세요."
                : hasErrors
                    ? "컴파일 에러가 있습니다. editor.console로 에러를 확인하세요."
                    : "컴파일 성공.";
            return new
            {
                result,
                compilation = new { isCompiling, hasErrors, nextAction },
                warning = isCompiling ? "서버가 도메인 리로드 중 일시 중단될 수 있습니다." : null,
            };
        }

        private static object CreateScript(JToken p)
        {
            string path = Validate.Required((string)p?["path"], "path");
            string template = (string)p?["template"] ?? "MonoBehaviour";
            string code = (string)p?["code"];
            string fullPath = ResolvePath(path);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            WorkflowManager.SnapshotAsset(path, $"script.create({path})");
            if (!string.IsNullOrEmpty(code))
            {
                File.WriteAllText(fullPath, code);
            }
            else
            {
                string className = Path.GetFileNameWithoutExtension(path);
                if (!Templates.TryGetValue(template, out var tmpl))
                    throw new McpException(-32602, $"Unknown template: {template}. Available: {string.Join(", ", Templates.Keys)}");
                File.WriteAllText(fullPath, tmpl.Replace("{NAME}", className));
            }
            AssetDatabase.Refresh();
            return WithCompilationInfo(
                new { success = true, path, template = string.IsNullOrEmpty(code) ? template : "custom" },
                isScriptMutation: true);
        }

        private static object ReadScript(JToken p)
        {
            string path = Validate.Required((string)p?["path"], "path");
            string fullPath = ResolvePath(path);
            if (!File.Exists(fullPath)) throw new McpException(-32003, $"File not found: {path}");
            string content = File.ReadAllText(fullPath);
            return new { path, lineCount = content.Split('\n').Length, size = content.Length, content };
        }

        private static object EditScript(JToken p)
        {
            string path = Validate.Required((string)p?["path"], "path");
            string fullPath = ResolvePath(path);
            if (!File.Exists(fullPath)) throw new McpException(-32003, $"File not found: {path}");
            WorkflowManager.SnapshotAsset(path, $"script.edit({path})");
            var contentToken = p?["content"];
            if (contentToken != null)
            {
                File.WriteAllText(fullPath, (string)contentToken);
                AssetDatabase.Refresh();
                return WithCompilationInfo(
                    new { success = true, path, mode = "fullReplace" },
                    isScriptMutation: true);
            }
            var editsToken = p?["lineEdits"] as JArray;
            if (editsToken != null)
            {
                var lines = File.ReadAllLines(fullPath).ToList();
                var edits = new List<(int line, string action, string text)>();
                foreach (var edit in editsToken)
                {
                    int line = (int)edit["line"];
                    string action = Validate.Required((string)edit["action"], "action");
                    string text = (string)edit["text"] ?? "";
                    edits.Add((line, action, text));
                }
                foreach (var (line, action, text) in edits.OrderByDescending(e => e.line))
                {
                    int idx = line - 1;
                    switch (action)
                    {
                        case "replace": if (idx >= 0 && idx < lines.Count) lines[idx] = text; break;
                        case "insert": if (idx >= 0 && idx <= lines.Count) lines.Insert(idx, text); break;
                        case "delete": if (idx >= 0 && idx < lines.Count) lines.RemoveAt(idx); break;
                        default: throw new McpException(-32602, $"Unknown edit action: {action}. Valid: replace, insert, delete");
                    }
                }
                File.WriteAllLines(fullPath, lines);
                AssetDatabase.Refresh();
                return WithCompilationInfo(
                    new { success = true, path, mode = "lineEdits", editCount = edits.Count },
                    isScriptMutation: true);
            }
            throw new McpException(-32602, "Either 'content' or 'lineEdits' must be provided");
        }

        private static object CompileCheck(JToken p)
        {
            bool hasErrors = EditorUtility.scriptCompilationFailed;
            bool isCompiling = EditorApplication.isCompiling;
            var assemblies = CompilationPipeline.GetAssemblies();
            var assemblyInfo = assemblies.Select(a => new { name = a.name, sourceFiles = a.sourceFiles.Length }).ToArray();
            string nextAction = isCompiling
                ? "아직 컴파일 중입니다. 잠시 후 다시 확인하세요."
                : hasErrors
                    ? "컴파일 에러가 있습니다. editor.console로 에러 내용을 확인하세요."
                    : "컴파일 완료. 에러 없음.";
            return new { hasErrors, isCompiling, assemblyCount = assemblies.Length, assemblies = assemblyInfo, nextAction };
        }

        private static object ListScripts(JToken p)
        {
            string folder = (string)p?["folder"] ?? "Assets";
            string pattern = (string)p?["pattern"] ?? "*.cs";
            bool recursive = (bool?)p?["recursive"] ?? true;
            string fullFolder = ResolvePath(folder);
            if (!Directory.Exists(fullFolder)) throw new McpException(-32003, $"Folder not found: {folder}");
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(fullFolder, pattern, option);
            var projectRoot = Path.Combine(Application.dataPath, "..");
            var scripts = files.Take(200).Select(f =>
            {
                var relativePath = Path.GetRelativePath(projectRoot, f).Replace('\\', '/');
                return new { path = relativePath, name = Path.GetFileNameWithoutExtension(f), size = new FileInfo(f).Length };
            }).ToArray();
            return new { folder, pattern, count = scripts.Length, totalFound = files.Length, scripts };
        }

        private static object DeleteScript(JToken p)
        {
            string path = Validate.Required((string)p?["path"], "path");
            string fullPath = ResolvePath(path);
            if (!File.Exists(fullPath)) throw new McpException(-32003, $"File not found: {path}");
            WorkflowManager.SnapshotAsset(path, $"script.delete({path})");
            AssetDatabase.DeleteAsset(path);
            return WithCompilationInfo(new { success = true, deleted = path }, isScriptMutation: true);
        }

        private static object RenameScript(JToken p)
        {
            string path = Validate.Required((string)p?["path"], "path");
            string newName = Validate.Required((string)p?["newName"], "newName");
            string fullPath = ResolvePath(path);
            if (!File.Exists(fullPath)) throw new McpException(-32003, $"File not found: {path}");
            WorkflowManager.SnapshotAsset(path, $"script.rename({path})");
            string result = AssetDatabase.RenameAsset(path, newName);
            if (!string.IsNullOrEmpty(result))
                throw new McpException(-32000, $"Rename failed: {result}");
            string dir = Path.GetDirectoryName(path).Replace('\\', '/');
            string newPath = $"{dir}/{newName}.cs";
            return WithCompilationInfo(new { success = true, oldPath = path, newPath }, isScriptMutation: true);
        }

        private static object SearchInScripts(JToken p)
        {
            string pattern = Validate.Required((string)p?["pattern"], "pattern");
            string folder = (string)p?["folder"] ?? "Assets";
            bool caseSensitive = (bool?)p?["caseSensitive"] ?? false;
            int maxResults = (int?)p?["maxResults"] ?? 50;
            string fullFolder = ResolvePath(folder);
            if (!Directory.Exists(fullFolder)) throw new McpException(-32003, $"Folder not found: {folder}");

            var files = Directory.GetFiles(fullFolder, "*.cs", SearchOption.AllDirectories);
            var projectRoot = Path.Combine(Application.dataPath, "..");
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var results = new List<object>();

            foreach (var file in files)
            {
                if (results.Count >= maxResults) break;
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(pattern, comparison))
                    {
                        results.Add(new
                        {
                            path = Path.GetRelativePath(projectRoot, file).Replace('\\', '/'),
                            line = i + 1,
                            content = lines[i].Trim(),
                        });
                        if (results.Count >= maxResults) break;
                    }
                }
            }

            return new { pattern, count = results.Count, results = results.ToArray() };
        }

        private static object GetScriptInfo(JToken p)
        {
            string typeName = Validate.Required<string>(p, "typeName");
            Type targetType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    targetType = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
                    if (targetType != null) break;
                }
                catch { }
            }
            if (targetType == null)
                throw new McpException(-32003, $"Type not found: {typeName}");

            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var fields = targetType.GetFields(flags).Select(f => new { name = f.Name, type = f.FieldType.Name }).ToArray();
            var methods = targetType.GetMethods(flags)
                .Where(m => !m.IsSpecialName)
                .Select(m => new { name = m.Name, returnType = m.ReturnType.Name, paramCount = m.GetParameters().Length })
                .ToArray();

            return new
            {
                name = targetType.Name,
                fullName = targetType.FullName,
                baseType = targetType.BaseType?.Name,
                isMonoBehaviour = typeof(MonoBehaviour).IsAssignableFrom(targetType),
                fields,
                methods,
            };
        }
    }
}
