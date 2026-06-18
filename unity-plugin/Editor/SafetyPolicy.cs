using System;
using Newtonsoft.Json.Linq;

namespace KarnelLabs.MCP
{
    public class MethodRiskInfo
    {
        public string Method { get; set; }
        public string RiskLevel { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IsDestructive { get; set; }
        public bool SupportsDryRun { get; set; }
        public bool RequiresConfirmation { get; set; }
        public string ConfirmationToken { get; set; }
        public string Reason { get; set; }
    }

    public static class SafetyPolicy
    {
        private static readonly string[] ReadOnlyVerbs =
        {
            "get", "list", "find", "search", "info", "status", "diagnostics", "capture", "validate", "check", "analyze", "profile", "preview"
        };

        private static readonly string[] MutatingVerbs =
        {
            "create", "add", "set", "edit", "write", "rename", "move", "copy", "import", "refresh", "begin", "end", "bake", "generate", "paint", "place", "apply", "run"
        };

        private static readonly string[] DestructiveVerbs =
        {
            "delete", "remove", "destroy", "clear", "reset", "clean", "undo", "revert"
        };

        public static bool RequireHighRiskConfirmation =>
            string.Equals(Environment.GetEnvironmentVariable("UNITY_MCP_REQUIRE_CONFIRMATION"), "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Environment.GetEnvironmentVariable("MCP_REQUIRE_CONFIRMATION"), "1", StringComparison.OrdinalIgnoreCase);

        public static MethodRiskInfo Describe(string method)
        {
            method ??= string.Empty;
            string lower = method.ToLowerInvariant();
            string action = ExtractAction(lower);

            bool destructive = ContainsAny(action, lower, DestructiveVerbs);
            bool mutating = destructive || ContainsAny(action, lower, MutatingVerbs);
            bool readOnly = !mutating || ContainsAny(action, lower, ReadOnlyVerbs);

            string level;
            string reason;
            if (destructive || lower.Contains("build") || lower.Contains("playersettings") || lower.Contains("androidsettings") || lower.Contains("iossettings") || lower.Contains("setbuildtarget"))
            {
                level = "high";
                reason = "May delete, reset, undo, build, or change project/platform settings.";
                readOnly = false;
            }
            else if (mutating)
            {
                level = "medium";
                reason = "May modify scene, asset, script, package, or editor state.";
                readOnly = false;
            }
            else
            {
                level = "low";
                reason = "Read-only inspection or diagnostics.";
                readOnly = true;
            }

            string token = ConfirmationTokenFor(method);
            return new MethodRiskInfo
            {
                Method = method,
                RiskLevel = level,
                IsReadOnly = readOnly,
                IsDestructive = destructive,
                SupportsDryRun = !readOnly,
                RequiresConfirmation = RequireHighRiskConfirmation && level == "high",
                ConfirmationToken = level == "high" ? token : null,
                Reason = reason,
            };
        }

        public static bool IsDryRun(JToken p)
        {
            return p != null && p.Type == JTokenType.Object && (bool?)p["dryRun"] == true;
        }

        public static bool HasValidConfirmation(JToken p, MethodRiskInfo risk)
        {
            if (!risk.RequiresConfirmation) return true;
            if (p == null || p.Type != JTokenType.Object) return false;
            return string.Equals((string)p["confirmationToken"], risk.ConfirmationToken, StringComparison.Ordinal);
        }

        public static object DryRunResult(MethodRiskInfo risk, JToken p)
        {
            return new
            {
                ok = true,
                dryRun = true,
                wouldExecute = risk.Method,
                risk,
                parameters = p,
                message = "Dry-run only: no Unity Editor changes were applied. Re-run without dryRun=true to execute."
            };
        }

        public static object ConfirmationRequired(MethodRiskInfo risk)
        {
            return new
            {
                codeName = "CONFIRMATION_REQUIRED",
                retryable = true,
                risk,
                confirmationToken = risk.ConfirmationToken,
                suggestedNextTool = "Re-run the same tool with confirmationToken and only after explaining the risky change to the user."
            };
        }

        public static object ErrorData(int code, string method = null, string codeName = null)
        {
            string resolved = codeName ?? CodeName(code);
            return new
            {
                codeName = resolved,
                retryable = IsRetryable(code),
                method,
                suggestedNextTool = SuggestedNextTool(code, method),
                risk = string.IsNullOrEmpty(method) ? null : Describe(method),
            };
        }

        public static string CodeName(int code)
        {
            return code switch
            {
                -32001 => "UNITY_NOT_CONNECTED",
                -32002 => "UNITY_REQUEST_TIMEOUT",
                -32003 => "GAMEOBJECT_NOT_FOUND",
                -32600 => "INVALID_REQUEST",
                -32601 => "UNKNOWN_METHOD",
                -32602 => "INVALID_PARAMS",
                _ => "UNITY_API_ERROR",
            };
        }

        private static bool IsRetryable(int code)
        {
            return code == -32001 || code == -32002 || code == -32003;
        }

        private static string SuggestedNextTool(int code, string method)
        {
            return code switch
            {
                -32001 => "Open Tools > KarnelLabs MCP > Server Window, confirm it is Listening, then call editor.diagnostics or unity_project_health.",
                -32002 => "Wait for Unity to finish compiling/importing, then call editor.diagnostics or retry with a narrower request.",
                -32003 => "Call scene hierarchy or search tools to locate the correct GameObject path before retrying.",
                -32601 => "Call editor.diagnostics and confirm the Unity plugin version/optional package handlers.",
                -32602 => "Check the tool schema and retry with valid parameters.",
                _ => string.IsNullOrEmpty(method) ? "Call editor.diagnostics and inspect the Unity Console." : $"Call editor.diagnostics and inspect recent request details for {method}.",
            };
        }

        private static string ConfirmationTokenFor(string method)
        {
            return "confirm:" + (method ?? string.Empty).Replace('.', ':');
        }

        private static string ExtractAction(string lowerMethod)
        {
            int dot = lowerMethod.LastIndexOf('.');
            string tail = dot >= 0 ? lowerMethod[(dot + 1)..] : lowerMethod;
            return tail.Replace("_", string.Empty).Replace("-", string.Empty);
        }

        private static bool ContainsAny(string action, string lowerMethod, string[] words)
        {
            foreach (var word in words)
            {
                if (action.Contains(word) || lowerMethod.Contains("." + word) || lowerMethod.Contains("_" + word))
                    return true;
            }
            return false;
        }
    }
}
