using Newtonsoft.Json.Linq;

namespace KarnelLabs.MCP
{
    public static class WorkflowHandler
    {
        public static void Register()
        {
            CommandRouter.Register("workflow.beginSession", BeginSession);
            CommandRouter.Register("workflow.endSession", EndSession);
            CommandRouter.Register("workflow.undoSession", UndoSession);
            CommandRouter.Register("workflow.undoLast", UndoLast);
            CommandRouter.Register("workflow.status", GetStatus);
            CommandRouter.Register("safety.describe", DescribeSafety);
            CommandRouter.Register("editor.diagnostics", Diagnostics);
        }

        private static object BeginSession(JToken p)
        {
            string name = (string)p?["name"];
            return WorkflowManager.BeginSession(name);
        }

        private static object EndSession(JToken p)
        {
            return WorkflowManager.EndSession();
        }

        private static object UndoSession(JToken p)
        {
            return WorkflowManager.UndoSession();
        }

        private static object UndoLast(JToken p)
        {
            return WorkflowManager.UndoLast();
        }

        private static object GetStatus(JToken p)
        {
            return WorkflowManager.GetStatus();
        }

        private static object DescribeSafety(JToken p)
        {
            string method = (string)p?["method"];
            if (string.IsNullOrEmpty(method))
                throw new McpException(-32602, "Missing required parameter: method");

            return new
            {
                risk = SafetyPolicy.Describe(method),
                dryRunPreview = SafetyPolicy.DryRunResult(SafetyPolicy.Describe(method), p?["params"]),
                confirmationRequiredWhen = "UNITY_MCP_REQUIRE_CONFIRMATION=1 or MCP_REQUIRE_CONFIRMATION=1"
            };
        }

        private static object Diagnostics(JToken p)
        {
            return McpBridge.GetDiagnostics();
        }
    }
}
