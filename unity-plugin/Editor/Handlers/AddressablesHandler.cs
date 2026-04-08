#if UNITY_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class AddressablesHandler
    {
        public static void Register()
        {
            CommandRouter.Register("addressables.getSettings", GetSettings);
            CommandRouter.Register("addressables.listGroups", ListGroups);
            CommandRouter.Register("addressables.createGroup", CreateGroup);
            CommandRouter.Register("addressables.removeGroup", RemoveGroup);
            CommandRouter.Register("addressables.markAddressable", MarkAddressable);
            CommandRouter.Register("addressables.removeAddressable", RemoveAddressable);
            CommandRouter.Register("addressables.setAddress", SetAddress);
            CommandRouter.Register("addressables.setLabel", SetLabel);
            CommandRouter.Register("addressables.getEntry", GetEntry);
            CommandRouter.Register("addressables.findEntries", FindEntries);
        }

        private static AddressableAssetSettings Settings
        {
            get
            {
                var s = AddressableAssetSettingsDefaultObject.Settings;
                if (s == null)
                {
                    s = AddressableAssetSettings.Create(
                        AddressableAssetSettingsDefaultObject.kDefaultConfigFolder,
                        AddressableAssetSettingsDefaultObject.kDefaultConfigAssetName,
                        true, true);
                    AddressableAssetSettingsDefaultObject.Settings = s;
                }
                return s;
            }
        }

        private static object GetSettings(JToken p)
        {
            var s = Settings;
            return new
            {
                profileName = s.activeProfileId,
                buildPath = s.profileSettings.GetValueByName(s.activeProfileId, "LocalBuildPath"),
                loadPath = s.profileSettings.GetValueByName(s.activeProfileId, "LocalLoadPath"),
                groupCount = s.groups.Count,
                labelCount = s.GetLabels().Count,
                labels = s.GetLabels().ToArray(),
            };
        }

        private static object ListGroups(JToken p)
        {
            var s = Settings;
            var groups = s.groups.Select(g => new
            {
                name = g.Name,
                guid = g.Guid,
                entryCount = g.entries.Count,
                readOnly = g.ReadOnly,
                schemas = g.Schemas.Select(sc => sc.GetType().Name).ToArray(),
            }).ToArray();
            return new { count = groups.Length, groups };
        }

        private static object CreateGroup(JToken p)
        {
            var s = Settings;
            var name = Validate.Required<string>(p, "groupName");
            var packed = p["packed"]?.Value<bool>() ?? true;

            var group = s.CreateGroup(name, false, false, true,
                packed
                    ? new List<AddressableAssetGroupSchema> { s.DefaultGroup.Schemas.OfType<BundledAssetGroupSchema>().FirstOrDefault() }
                    : null);

            if (packed && group.GetSchema<BundledAssetGroupSchema>() == null)
                group.AddSchema<BundledAssetGroupSchema>();

            EditorUtility.SetDirty(s);
            return new { success = true, groupName = group.Name, guid = group.Guid };
        }

        private static object RemoveGroup(JToken p)
        {
            var s = Settings;
            var name = Validate.Required<string>(p, "groupName");
            var group = s.groups.FirstOrDefault(g => g.Name == name);
            if (group == null) throw new McpException(-32602, $"Group not found: {name}");

            s.RemoveGroup(group);
            EditorUtility.SetDirty(s);
            return new { success = true, removed = name };
        }

        private static object MarkAddressable(JToken p)
        {
            var s = Settings;
            var assetPath = Validate.Required<string>(p, "assetPath");
            var groupName = p["groupName"]?.Value<string>();
            var address = p["address"]?.Value<string>();

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid)) throw new McpException(-32003, $"Asset not found: {assetPath}");

            AddressableAssetGroup group = null;
            if (!string.IsNullOrEmpty(groupName))
                group = s.groups.FirstOrDefault(g => g.Name == groupName);
            group ??= s.DefaultGroup;

            var entry = s.CreateOrMoveEntry(guid, group);
            if (!string.IsNullOrEmpty(address)) entry.address = address;

            EditorUtility.SetDirty(s);
            return new { success = true, assetPath, address = entry.address, group = group.Name };
        }

        private static object RemoveAddressable(JToken p)
        {
            var s = Settings;
            var assetPath = Validate.Required<string>(p, "assetPath");
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrEmpty(guid)) throw new McpException(-32003, $"Asset not found: {assetPath}");

            var removed = s.RemoveAssetEntry(guid);
            EditorUtility.SetDirty(s);
            return new { success = removed, assetPath };
        }

        private static object SetAddress(JToken p)
        {
            var s = Settings;
            var assetPath = Validate.Required<string>(p, "assetPath");
            var address = Validate.Required<string>(p, "address");
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = s.FindAssetEntry(guid);
            if (entry == null) throw new McpException(-32602, $"Asset not addressable: {assetPath}");

            entry.address = address;
            EditorUtility.SetDirty(s);
            return new { success = true, assetPath, address };
        }

        private static object SetLabel(JToken p)
        {
            var s = Settings;
            var assetPath = Validate.Required<string>(p, "assetPath");
            var label = Validate.Required<string>(p, "label");
            var enabled = p["enabled"]?.Value<bool>() ?? true;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = s.FindAssetEntry(guid);
            if (entry == null) throw new McpException(-32602, $"Asset not addressable: {assetPath}");

            // Add label to settings if it doesn't exist
            s.AddLabel(label);
            entry.SetLabel(label, enabled);
            EditorUtility.SetDirty(s);
            return new { success = true, assetPath, label, enabled };
        }

        private static object GetEntry(JToken p)
        {
            var s = Settings;
            var assetPath = Validate.Required<string>(p, "assetPath");
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = s.FindAssetEntry(guid);
            if (entry == null) return new { isAddressable = false, assetPath };

            return new
            {
                isAddressable = true,
                assetPath, address = entry.address,
                group = entry.parentGroup.Name,
                labels = entry.labels.ToArray(),
                guid = entry.guid,
            };
        }

        private static object FindEntries(JToken p)
        {
            var s = Settings;
            var labelFilter = p["label"]?.Value<string>();
            var groupFilter = p["groupName"]?.Value<string>();

            var entries = new List<object>();
            foreach (var group in s.groups)
            {
                if (!string.IsNullOrEmpty(groupFilter) && group.Name != groupFilter) continue;
                foreach (var entry in group.entries)
                {
                    if (!string.IsNullOrEmpty(labelFilter) && !entry.labels.Contains(labelFilter)) continue;
                    entries.Add(new
                    {
                        address = entry.address,
                        assetPath = entry.AssetPath,
                        group = group.Name,
                        labels = entry.labels.ToArray(),
                    });
                }
            }
            return new { count = entries.Count, entries = entries.ToArray() };
        }
    }
}
#endif
