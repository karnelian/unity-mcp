using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_LOCALIZATION
using UnityEditor.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
#endif

namespace KarnelLabs.MCP
{
    public static class LocalizationHandler
    {
        public static void Register()
        {
#if UNITY_LOCALIZATION
            CommandRouter.Register("localization.getLocales", GetLocales);
            CommandRouter.Register("localization.addLocale", AddLocale);
            CommandRouter.Register("localization.createStringTable", CreateStringTable);
            CommandRouter.Register("localization.getStringTable", GetStringTable);
            CommandRouter.Register("localization.setEntry", SetEntry);
            CommandRouter.Register("localization.removeEntry", RemoveEntry);
            CommandRouter.Register("localization.findTables", FindTables);
            CommandRouter.Register("localization.getProjectLocale", GetProjectLocale);
            CommandRouter.Register("localization.setProjectLocale", SetProjectLocale);
            CommandRouter.Register("localization.exportCsv", ExportCsv);
#endif
        }

#if UNITY_LOCALIZATION
        private static object GetLocales(JToken p)
        {
            var locales = LocalizationEditorSettings.GetLocales();
            return new
            {
                count = locales.Count,
                locales = locales.Select(l => new
                {
                    name = l.LocaleName,
                    code = l.Identifier.Code,
                    cultureInfo = l.Identifier.CultureInfo?.DisplayName
                }).ToArray()
            };
        }

        private static object AddLocale(JToken p)
        {
            string code = (string)p["code"];
            if (string.IsNullOrEmpty(code))
                throw new McpException(-32602, "code is required (e.g., 'en', 'ko', 'ja')");

            var identifier = new LocaleIdentifier(code);
            var locale = Locale.CreateLocale(identifier);

            string savePath = $"Assets/Localization/Locales/{code}.asset";
            var dir = System.IO.Path.GetDirectoryName(savePath);
            if (!AssetDatabase.IsValidFolder(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            AssetDatabase.CreateAsset(locale, savePath);
            LocalizationEditorSettings.AddLocale(locale);
            AssetDatabase.SaveAssets();

            return new { code, name = locale.LocaleName, path = savePath };
        }

        private static object CreateStringTable(JToken p)
        {
            string tableName = (string)p["tableName"];
            if (string.IsNullOrEmpty(tableName))
                throw new McpException(-32602, "tableName is required");

            var locales = LocalizationEditorSettings.GetLocales();
            if (locales.Count == 0)
                throw new McpException(-32602, "No locales configured. Add locales first.");

            string savePath = (string)p?["savePath"] ?? "Assets/Localization/Tables";
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                System.IO.Directory.CreateDirectory(savePath);
                AssetDatabase.Refresh();
            }

            var collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection != null)
                return new { tableName, status = "already exists", tableCount = collection.StringTables.Count };

            collection = LocalizationEditorSettings.CreateStringTableCollection(tableName, savePath, locales);
            AssetDatabase.SaveAssets();

            return new { tableName, path = savePath, tableCount = collection.StringTables.Count };
        }

        private static object GetStringTable(JToken p)
        {
            string tableName = (string)p["tableName"];
            string localeCode = (string)p?["locale"];

            if (string.IsNullOrEmpty(tableName))
                throw new McpException(-32602, "tableName is required");

            var collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
                throw new McpException(-32602, $"StringTable not found: {tableName}");

            if (!string.IsNullOrEmpty(localeCode))
            {
                var table = collection.StringTables.FirstOrDefault(t =>
                    t.LocaleIdentifier.Code == localeCode);
                if (table == null)
                    throw new McpException(-32602, $"Locale '{localeCode}' not found in table '{tableName}'");

                var entries = table.Values.Select(e => new
                {
                    key = e.Key,
                    keyId = e.KeyId,
                    value = e.Value
                }).ToArray();

                return new { tableName, locale = localeCode, entryCount = entries.Length, entries };
            }

            var allEntries = collection.SharedData.Entries.Select(shared =>
            {
                var translations = collection.StringTables.ToDictionary(
                    t => t.LocaleIdentifier.Code,
                    t => t.GetEntry(shared.Id)?.Value ?? ""
                );
                return new { key = shared.Key, keyId = shared.Id, translations };
            }).ToArray();

            return new { tableName, entryCount = allEntries.Length, entries = allEntries };
        }

        private static object SetEntry(JToken p)
        {
            string tableName = (string)p["tableName"];
            string key = (string)p["key"];
            string locale = (string)p["locale"];
            string value = (string)p["value"];

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(key) ||
                string.IsNullOrEmpty(locale))
                throw new McpException(-32602, "tableName, key, and locale are required");

            var collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
                throw new McpException(-32602, $"StringTable not found: {tableName}");

            var table = collection.StringTables.FirstOrDefault(t =>
                t.LocaleIdentifier.Code == locale);
            if (table == null)
                throw new McpException(-32602, $"Locale '{locale}' not found in table '{tableName}'");

            // Ensure shared data entry exists
            var sharedEntry = collection.SharedData.GetEntry(key);
            if (sharedEntry == null)
                sharedEntry = collection.SharedData.AddKey(key);

            table.AddEntry(key, value ?? "");
            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(collection.SharedData);
            AssetDatabase.SaveAssets();

            return new { tableName, key, locale, value };
        }

        private static object RemoveEntry(JToken p)
        {
            string tableName = (string)p["tableName"];
            string key = (string)p["key"];

            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(key))
                throw new McpException(-32602, "tableName and key are required");

            var collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
                throw new McpException(-32602, $"StringTable not found: {tableName}");

            var sharedEntry = collection.SharedData.GetEntry(key);
            if (sharedEntry == null)
                throw new McpException(-32602, $"Key '{key}' not found in table '{tableName}'");

            collection.SharedData.RemoveKey(key);
            foreach (var table in collection.StringTables)
            {
                table.RemoveEntry(sharedEntry.Id);
                EditorUtility.SetDirty(table);
            }
            EditorUtility.SetDirty(collection.SharedData);
            AssetDatabase.SaveAssets();

            return new { tableName, removedKey = key };
        }

        private static object FindTables(JToken p)
        {
            string filter = (string)p?["filter"] ?? "";
            var guids = AssetDatabase.FindAssets($"t:StringTableCollection {filter}");
            var tables = guids.Select(guid =>
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var collection = AssetDatabase.LoadAssetAtPath<StringTableCollection>(path);
                return collection == null ? null : new
                {
                    name = collection.TableCollectionName,
                    path,
                    localeCount = collection.StringTables.Count,
                    entryCount = collection.SharedData?.Entries?.Count ?? 0
                };
            }).Where(t => t != null).ToArray();

            return new { count = tables.Length, tables };
        }

        private static object GetProjectLocale(JToken p)
        {
            var settings = LocalizationSettings.Instance;
            if (settings == null)
                return new { status = "Localization not initialized" };

            var selectedLocale = settings.GetSelectedLocale();
            var availableLocales = settings.GetAvailableLocales()?.Locales;

            return new
            {
                selectedLocale = selectedLocale != null ? new { name = selectedLocale.LocaleName, code = selectedLocale.Identifier.Code } : null,
                availableLocales = availableLocales?.Select(l => new { name = l.LocaleName, code = l.Identifier.Code }).ToArray()
            };
        }

        private static object SetProjectLocale(JToken p)
        {
            string code = (string)p["code"];
            if (string.IsNullOrEmpty(code))
                throw new McpException(-32602, "code is required");

            var settings = LocalizationSettings.Instance;
            if (settings == null)
                throw new McpException(-32602, "Localization not initialized");

            var locale = settings.GetAvailableLocales()?.Locales?
                .FirstOrDefault(l => l.Identifier.Code == code);

            if (locale == null)
                throw new McpException(-32602, $"Locale '{code}' not available in project");

            settings.SetSelectedLocale(locale);
            return new { code, name = locale.LocaleName };
        }

        private static object ExportCsv(JToken p)
        {
            string tableName = (string)p["tableName"];
            string outputPath = (string)p?["outputPath"] ?? $"Assets/Localization/Export/{tableName}.csv";

            if (string.IsNullOrEmpty(tableName))
                throw new McpException(-32602, "tableName is required");

            var collection = LocalizationEditorSettings.GetStringTableCollection(tableName);
            if (collection == null)
                throw new McpException(-32602, $"StringTable not found: {tableName}");

            var dir = System.IO.Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            var sb = new System.Text.StringBuilder();
            var localeCodes = collection.StringTables.Select(t => t.LocaleIdentifier.Code).ToArray();
            sb.AppendLine("Key," + string.Join(",", localeCodes));

            foreach (var shared in collection.SharedData.Entries)
            {
                sb.Append(EscapeCsv(shared.Key));
                foreach (var table in collection.StringTables)
                {
                    var entry = table.GetEntry(shared.Id);
                    sb.Append("," + EscapeCsv(entry?.Value ?? ""));
                }
                sb.AppendLine();
            }

            System.IO.File.WriteAllText(outputPath, sb.ToString(), System.Text.Encoding.UTF8);
            AssetDatabase.Refresh();

            return new { tableName, outputPath, entryCount = collection.SharedData.Entries.Count, locales = localeCodes };
        }

        private static string EscapeCsv(string s)
        {
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }
#endif
    }
}
