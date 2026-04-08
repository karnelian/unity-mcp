using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KarnelLabs.MCP
{
    public static class VersionControlHandler
    {
        public static void Register()
        {
            CommandRouter.Register("vcs.getStatus", GetStatus);
            CommandRouter.Register("vcs.getChanges", GetChanges);
            CommandRouter.Register("vcs.getHistory", GetHistory);
            CommandRouter.Register("vcs.getBranches", GetBranches);
            CommandRouter.Register("vcs.getCurrentBranch", GetCurrentBranch);
            CommandRouter.Register("vcs.getRemotes", GetRemotes);
            CommandRouter.Register("vcs.getDiff", GetDiff);
            CommandRouter.Register("vcs.getStash", GetStash);
        }

        private static string RunGit(string args, string workingDir = null)
        {
            workingDir ??= Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = workingDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            try
            {
                using var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit(10000);

                if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                    throw new McpException(-32000, $"Git error: {error.Trim()}");

                return output.Trim();
            }
            catch (Exception ex) when (ex is not McpException)
            {
                throw new McpException(-32000, $"Git not available or not a git repository: {ex.Message}");
            }
        }

        private static object GetStatus(JToken p)
        {
            var output = RunGit("status --porcelain");
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var files = lines.Select(line =>
            {
                string status = line.Length >= 2 ? line.Substring(0, 2).Trim() : "";
                string file = line.Length >= 3 ? line.Substring(3).Trim() : line.Trim();
                string statusName = status switch
                {
                    "M" => "modified",
                    "A" => "added",
                    "D" => "deleted",
                    "R" => "renamed",
                    "C" => "copied",
                    "U" => "unmerged",
                    "??" => "untracked",
                    "!!" => "ignored",
                    _ => status
                };
                return new { status = statusName, file };
            }).ToArray();

            var branch = RunGit("branch --show-current");

            return new { branch, fileCount = files.Length, files };
        }

        private static object GetChanges(JToken p)
        {
            bool staged = p?["staged"]?.Value<bool>() ?? false;
            string args = staged ? "diff --cached --name-status" : "diff --name-status";
            var output = RunGit(args);
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var changes = lines.Select(line =>
            {
                var parts = line.Split('\t');
                return new
                {
                    status = parts[0] switch { "M" => "modified", "A" => "added", "D" => "deleted", _ => parts[0] },
                    file = parts.Length > 1 ? parts[1] : ""
                };
            }).ToArray();

            return new { staged, changeCount = changes.Length, changes };
        }

        private static object GetHistory(JToken p)
        {
            int count = p?["count"]?.Value<int>() ?? 20;
            string filePath = (string)p?["filePath"];

            string args = $"log --oneline --format=\"%H|%h|%an|%ae|%s|%ci\" -n {count}";
            if (!string.IsNullOrEmpty(filePath)) args += $" -- \"{filePath}\"";

            var output = RunGit(args);
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var commits = lines.Select(line =>
            {
                var parts = line.Split('|');
                if (parts.Length < 6) return new { hash = line, shortHash = "", author = "", email = "", message = "", date = "" };
                return new { hash = parts[0], shortHash = parts[1], author = parts[2], email = parts[3], message = parts[4], date = parts[5] };
            }).ToArray();

            return new { count = commits.Length, commits };
        }

        private static object GetBranches(JToken p)
        {
            bool all = p?["all"]?.Value<bool>() ?? false;
            string args = all ? "branch -a --format=%(refname:short)|%(objectname:short)|%(upstream:short)"
                              : "branch --format=%(refname:short)|%(objectname:short)|%(upstream:short)";
            var output = RunGit(args);
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var current = RunGit("branch --show-current");

            var branches = lines.Select(line =>
            {
                var parts = line.Split('|');
                return new
                {
                    name = parts[0],
                    shortHash = parts.Length > 1 ? parts[1] : "",
                    upstream = parts.Length > 2 ? parts[2] : "",
                    isCurrent = parts[0] == current
                };
            }).ToArray();

            return new { current, branchCount = branches.Length, branches };
        }

        private static object GetCurrentBranch(JToken p)
        {
            var branch = RunGit("branch --show-current");
            var hash = RunGit("rev-parse HEAD");
            var message = RunGit("log -1 --format=%s");

            return new { branch, hash, lastCommitMessage = message };
        }

        private static object GetRemotes(JToken p)
        {
            var output = RunGit("remote -v");
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var remotes = lines.Where(l => l.Contains("(fetch)")).Select(line =>
            {
                var parts = line.Split('\t');
                var name = parts[0];
                var url = parts.Length > 1 ? parts[1].Replace(" (fetch)", "").Trim() : "";
                return new { name, url };
            }).ToArray();

            return new { count = remotes.Length, remotes };
        }

        private static object GetDiff(JToken p)
        {
            string filePath = (string)p?["filePath"];
            bool staged = p?["staged"]?.Value<bool>() ?? false;
            int contextLines = p?["contextLines"]?.Value<int>() ?? 3;

            string args = staged ? $"diff --cached -U{contextLines}" : $"diff -U{contextLines}";
            if (!string.IsNullOrEmpty(filePath)) args += $" -- \"{filePath}\"";

            var output = RunGit(args);

            // Truncate if too large
            if (output.Length > 50000)
                output = output.Substring(0, 50000) + "\n... (truncated)";

            return new { staged, diff = output };
        }

        private static object GetStash(JToken p)
        {
            var output = RunGit("stash list --format=%gd|%gs");
            var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var stashes = lines.Select(line =>
            {
                var parts = line.Split('|', 2);
                return new { index = parts[0], message = parts.Length > 1 ? parts[1] : "" };
            }).ToArray();

            return new { count = stashes.Length, stashes };
        }
    }
}
