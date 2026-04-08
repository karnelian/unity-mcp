using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class PackageHandler
    {
        public static void Register()
        {
            CommandRouter.Register("package.list", ListPackages);
            CommandRouter.Register("package.info", GetInfo);
            CommandRouter.Register("package.add", AddPackage);
            CommandRouter.Register("package.remove", RemovePackage);
            CommandRouter.Register("package.search", SearchPackages);
            CommandRouter.Register("package.getVersion", GetVersion);
            CommandRouter.Register("package.listBuiltIn", ListBuiltIn);
            CommandRouter.Register("package.resolve", Resolve);
        }

        private static T WaitForRequest<T>(T request) where T : Request
        {
            while (!request.IsCompleted)
                System.Threading.Thread.Sleep(10);
            if (request.Status == StatusCode.Failure)
                throw new McpException(-32000, $"Package Manager error: {request.Error?.message}");
            return request;
        }

        private static object ListPackages(JToken p)
        {
            var includeIndirect = p["includeIndirect"]?.Value<bool>() ?? false;
            var request = WaitForRequest(Client.List(includeIndirect));
            var packages = request.Result.Select(pkg => new
            {
                name = pkg.name,
                displayName = pkg.displayName,
                version = pkg.version,
                source = pkg.source.ToString(),
            }).ToArray();
            return new { count = packages.Length, packages };
        }

        private static object GetInfo(JToken p)
        {
            var packageName = Validate.Required<string>(p, "packageName");
            var request = WaitForRequest(Client.List());
            var pkg = request.Result.FirstOrDefault(x => x.name == packageName);
            if (pkg == null) throw new McpException(-32003, $"Package not found: {packageName}");

            return new
            {
                name = pkg.name,
                displayName = pkg.displayName,
                version = pkg.version,
                description = pkg.description,
                source = pkg.source.ToString(),
                dependencies = pkg.dependencies.Select(d => new { d.name, d.version }).ToArray(),
                documentationUrl = pkg.documentationUrl,
            };
        }

        private static object AddPackage(JToken p)
        {
            var identifier = Validate.Required<string>(p, "identifier");
            var request = WaitForRequest(Client.Add(identifier));
            var pkg = request.Result;
            return new { added = true, name = pkg.name, version = pkg.version };
        }

        private static object RemovePackage(JToken p)
        {
            var packageName = Validate.Required<string>(p, "packageName");
            var request = WaitForRequest(Client.Remove(packageName));
            return new { removed = true, packageName };
        }

        private static object SearchPackages(JToken p)
        {
            var query = p["query"]?.Value<string>();
            var request = WaitForRequest(Client.SearchAll());
            IEnumerable<UnityEditor.PackageManager.PackageInfo> results = request.Result;

            if (!string.IsNullOrEmpty(query))
                results = results.Where(pkg =>
                    pkg.name.ToLower().Contains(query.ToLower()) ||
                    (pkg.displayName?.ToLower().Contains(query.ToLower()) ?? false));

            var packages = results.Take(50).Select(pkg => new
            {
                name = pkg.name,
                displayName = pkg.displayName,
                version = pkg.version,
                description = pkg.description?.Substring(0, Math.Min(pkg.description.Length, 100)),
            }).ToArray();

            return new { count = packages.Length, packages };
        }

        private static object GetVersion(JToken p)
        {
            var packageName = Validate.Required<string>(p, "packageName");
            var request = WaitForRequest(Client.List());
            var pkg = request.Result.FirstOrDefault(x => x.name == packageName);
            if (pkg == null) throw new McpException(-32003, $"Package not installed: {packageName}");

            return new
            {
                name = pkg.name,
                current = pkg.version,
                latest = pkg.versions.latest,
                compatible = pkg.versions.compatible,
                verified = pkg.versions.recommended,
            };
        }

        private static object ListBuiltIn(JToken p)
        {
            var request = WaitForRequest(Client.List());
            var builtIn = request.Result
                .Where(pkg => pkg.source == PackageSource.BuiltIn)
                .Select(pkg => new { name = pkg.name, displayName = pkg.displayName, version = pkg.version })
                .ToArray();
            return new { count = builtIn.Length, packages = builtIn };
        }

        private static object Resolve(JToken p)
        {
            Client.Resolve();
            return new { resolved = true };
        }
    }
}
