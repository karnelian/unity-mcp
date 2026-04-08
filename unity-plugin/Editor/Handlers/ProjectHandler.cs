using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace KarnelLabs.MCP
{
    public static class ProjectHandler
    {
        public static void Register()
        {
            CommandRouter.Register("project.info", GetProjectInfo);
            CommandRouter.Register("project.getRenderPipeline", GetRenderPipeline);
            CommandRouter.Register("project.getQualitySettings", GetQualitySettings);
            CommandRouter.Register("project.setQualityLevel", SetQualityLevel);
            CommandRouter.Register("project.getPlayerSettings", GetPlayerSettings);
            CommandRouter.Register("project.setPlayerSettings", SetPlayerSettings);
            CommandRouter.Register("project.getTags", GetTags);
            CommandRouter.Register("project.getLayers", GetLayers);
            CommandRouter.Register("project.addTag", AddTag);
            CommandRouter.Register("project.addLayer", AddLayer);
            CommandRouter.Register("project.getTimeSettings", GetTimeSettings);
            CommandRouter.Register("project.setTimeSettings", SetTimeSettings);
            CommandRouter.Register("project.getBuildTarget", GetBuildTarget);
            CommandRouter.Register("project.setBuildTarget", SetBuildTarget);
            CommandRouter.Register("project.getAndroidSettings", GetAndroidSettings);
            CommandRouter.Register("project.setAndroidSettings", SetAndroidSettings);
            CommandRouter.Register("project.getIOSSettings", GetIOSSettings);
            CommandRouter.Register("project.setIOSSettings", SetIOSSettings);
        }

        private static object GetProjectInfo(JToken p)
        {
            return new
            {
                productName = Application.productName,
                companyName = Application.companyName,
                version = Application.version,
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
                dataPath = Application.dataPath,
                persistentDataPath = Application.persistentDataPath,
                isPlaying = Application.isPlaying,
                isBatchMode = Application.isBatchMode,
                systemLanguage = Application.systemLanguage.ToString(),
                targetFrameRate = Application.targetFrameRate,
                runInBackground = Application.runInBackground,
                renderPipeline = GraphicsSettings.currentRenderPipeline?.name ?? "Built-in",
            };
        }

        private static object GetRenderPipeline(JToken p)
        {
            var current = GraphicsSettings.currentRenderPipeline;
            return new
            {
                name = current?.name ?? "Built-in Render Pipeline",
                type = current?.GetType().Name ?? "BuiltIn",
            };
        }

        private static object GetQualitySettings(JToken p)
        {
            return new
            {
                currentLevel = QualitySettings.GetQualityLevel(),
                names = QualitySettings.names,
                pixelLightCount = QualitySettings.pixelLightCount,
                shadows = QualitySettings.shadows.ToString(),
                shadowResolution = QualitySettings.shadowResolution.ToString(),
                antiAliasing = QualitySettings.antiAliasing,
                vSyncCount = QualitySettings.vSyncCount,
                lodBias = QualitySettings.lodBias,
                particleRaycastBudget = QualitySettings.particleRaycastBudget,
            };
        }

        private static object SetQualityLevel(JToken p)
        {
            var level = Validate.Required<int>(p, "level");
            var applyExpensiveChanges = p["applyExpensiveChanges"]?.Value<bool>() ?? true;
            QualitySettings.SetQualityLevel(level, applyExpensiveChanges);
            return new { level = QualitySettings.GetQualityLevel(), name = QualitySettings.names[QualitySettings.GetQualityLevel()] };
        }

        private static object GetPlayerSettings(JToken p)
        {
            return new
            {
                productName = PlayerSettings.productName,
                companyName = PlayerSettings.companyName,
                bundleVersion = PlayerSettings.bundleVersion,
                defaultScreenWidth = PlayerSettings.defaultScreenWidth,
                defaultScreenHeight = PlayerSettings.defaultScreenHeight,
                fullscreen = PlayerSettings.fullScreenMode.ToString(),
                runInBackground = PlayerSettings.runInBackground,
                colorSpace = PlayerSettings.colorSpace.ToString(),
                apiCompatibilityLevel = PlayerSettings.GetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)).ToString(),
                scriptingBackend = PlayerSettings.GetScriptingBackend(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup)).ToString(),
            };
        }

        private static object SetPlayerSettings(JToken p)
        {
            if (p["productName"] != null) PlayerSettings.productName = p["productName"].Value<string>();
            if (p["companyName"] != null) PlayerSettings.companyName = p["companyName"].Value<string>();
            if (p["bundleVersion"] != null) PlayerSettings.bundleVersion = p["bundleVersion"].Value<string>();
            if (p["defaultScreenWidth"] != null) PlayerSettings.defaultScreenWidth = p["defaultScreenWidth"].Value<int>();
            if (p["defaultScreenHeight"] != null) PlayerSettings.defaultScreenHeight = p["defaultScreenHeight"].Value<int>();
            if (p["runInBackground"] != null) PlayerSettings.runInBackground = p["runInBackground"].Value<bool>();

            return GetPlayerSettings(p);
        }

        private static object GetTags(JToken p)
        {
            return new { tags = UnityEditorInternal.InternalEditorUtility.tags };
        }

        private static object GetLayers(JToken p)
        {
            var layers = new List<object>();
            for (int i = 0; i < 32; i++)
            {
                var name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name))
                    layers.Add(new { index = i, name });
            }
            return new { layers };
        }

        private static object AddTag(JToken p)
        {
            var tagName = Validate.Required<string>(p, "tag");
            var tags = UnityEditorInternal.InternalEditorUtility.tags;
            if (tags.Contains(tagName))
                return new { added = false, message = $"Tag '{tagName}' already exists" };

            UnityEditorInternal.InternalEditorUtility.AddTag(tagName);
            return new { added = true, tag = tagName };
        }

        private static object AddLayer(JToken p)
        {
            var layerName = Validate.Required<string>(p, "layer");

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");

            // Find empty slot (8-31 are user layers)
            for (int i = 8; i < 32; i++)
            {
                var layerProp = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(layerProp.stringValue))
                {
                    layerProp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return new { added = true, layer = layerName, index = i };
                }
            }
            throw new McpException(-32602, "No empty layer slots available (max 32 layers)");
        }

        private static object GetTimeSettings(JToken p)
        {
            return new
            {
                fixedDeltaTime = Time.fixedDeltaTime,
                maximumDeltaTime = Time.maximumDeltaTime,
                timeScale = Time.timeScale,
                maximumParticleDeltaTime = Time.maximumParticleDeltaTime,
            };
        }

        private static object SetTimeSettings(JToken p)
        {
            if (p["fixedDeltaTime"] != null) Time.fixedDeltaTime = p["fixedDeltaTime"].Value<float>();
            if (p["maximumDeltaTime"] != null) Time.maximumDeltaTime = p["maximumDeltaTime"].Value<float>();
            if (p["timeScale"] != null) Time.timeScale = p["timeScale"].Value<float>();

            return GetTimeSettings(p);
        }

        private static object GetBuildTarget(JToken p)
        {
            return new
            {
                activeBuildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(),
                buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget).ToString(),
                selectedStandaloneTarget = EditorUserBuildSettings.selectedStandaloneTarget.ToString(),
            };
        }

        private static object SetBuildTarget(JToken p)
        {
            var targetStr = Validate.Required<string>(p, "target");
            if (!Enum.TryParse<BuildTarget>(targetStr, true, out var target))
                throw new McpException(-32602, $"Invalid build target: {targetStr}");
            var group = BuildPipeline.GetBuildTargetGroup(target);
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
            return new { success = true, activeBuildTarget = target.ToString(), group = group.ToString() };
        }

        private static object GetAndroidSettings(JToken p)
        {
            var nbt = NamedBuildTarget.Android;
            return new
            {
                packageName = PlayerSettings.GetApplicationIdentifier(nbt),
                minSdkVersion = PlayerSettings.Android.minSdkVersion.ToString(),
                targetSdkVersion = PlayerSettings.Android.targetSdkVersion.ToString(),
                targetArchitectures = PlayerSettings.Android.targetArchitectures.ToString(),
                scriptingBackend = PlayerSettings.GetScriptingBackend(nbt).ToString(),
                installLocation = PlayerSettings.Android.preferredInstallLocation.ToString(),
                splitApplicationBinary = PlayerSettings.Android.splitApplicationBinary,
                keystoreName = PlayerSettings.Android.keystoreName,
            };
        }

        private static object SetAndroidSettings(JToken p)
        {
            var nbt = NamedBuildTarget.Android;
            if (p["packageName"] != null)
                PlayerSettings.SetApplicationIdentifier(nbt, p["packageName"].Value<string>());
            if (p["minSdkVersion"] != null && Enum.TryParse<AndroidSdkVersions>(p["minSdkVersion"].Value<string>(), true, out var minSdk))
                PlayerSettings.Android.minSdkVersion = minSdk;
            if (p["targetSdkVersion"] != null && Enum.TryParse<AndroidSdkVersions>(p["targetSdkVersion"].Value<string>(), true, out var targetSdk))
                PlayerSettings.Android.targetSdkVersion = targetSdk;
            if (p["targetArchitectures"] != null && Enum.TryParse<AndroidArchitecture>(p["targetArchitectures"].Value<string>(), true, out var arch))
                PlayerSettings.Android.targetArchitectures = arch;
            if (p["scriptingBackend"] != null && Enum.TryParse<ScriptingImplementation>(p["scriptingBackend"].Value<string>(), true, out var backend))
                PlayerSettings.SetScriptingBackend(nbt, backend);
            return GetAndroidSettings(p);
        }

        private static object GetIOSSettings(JToken p)
        {
            var nbt = NamedBuildTarget.iOS;
            return new
            {
                bundleIdentifier = PlayerSettings.GetApplicationIdentifier(nbt),
                targetOSVersionString = PlayerSettings.iOS.targetOSVersionString,
                sdkVersion = PlayerSettings.iOS.sdkVersion.ToString(),
                targetDevice = PlayerSettings.iOS.targetDevice.ToString(),
                scriptingBackend = PlayerSettings.GetScriptingBackend(nbt).ToString(),
                automaticallySign = PlayerSettings.iOS.appleEnableAutomaticSigning,
                teamId = PlayerSettings.iOS.appleDeveloperTeamID,
                cameraUsageDescription = PlayerSettings.iOS.cameraUsageDescription,
            };
        }

        private static object SetIOSSettings(JToken p)
        {
            var nbt = NamedBuildTarget.iOS;
            if (p["bundleIdentifier"] != null)
                PlayerSettings.SetApplicationIdentifier(nbt, p["bundleIdentifier"].Value<string>());
            if (p["targetOSVersionString"] != null)
                PlayerSettings.iOS.targetOSVersionString = p["targetOSVersionString"].Value<string>();
            if (p["sdkVersion"] != null && Enum.TryParse<iOSSdkVersion>(p["sdkVersion"].Value<string>(), true, out var sdk))
                PlayerSettings.iOS.sdkVersion = sdk;
            if (p["targetDevice"] != null && Enum.TryParse<iOSTargetDevice>(p["targetDevice"].Value<string>(), true, out var device))
                PlayerSettings.iOS.targetDevice = device;
            if (p["scriptingBackend"] != null && Enum.TryParse<ScriptingImplementation>(p["scriptingBackend"].Value<string>(), true, out var backend))
                PlayerSettings.SetScriptingBackend(nbt, backend);
            if (p["automaticallySign"] != null)
                PlayerSettings.iOS.appleEnableAutomaticSigning = p["automaticallySign"].Value<bool>();
            if (p["teamId"] != null)
                PlayerSettings.iOS.appleDeveloperTeamID = p["teamId"].Value<string>();
            if (p["cameraUsageDescription"] != null)
                PlayerSettings.iOS.cameraUsageDescription = p["cameraUsageDescription"].Value<string>();
            return GetIOSSettings(p);
        }
    }
}
