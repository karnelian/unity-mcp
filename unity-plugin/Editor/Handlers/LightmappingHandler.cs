using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class LightmappingHandler
    {
        public static void Register()
        {
            CommandRouter.Register("lightmapping.bake", Bake);
            CommandRouter.Register("lightmapping.cancel", Cancel);
            CommandRouter.Register("lightmapping.clear", Clear);
            CommandRouter.Register("lightmapping.getSettings", GetSettings);
            CommandRouter.Register("lightmapping.setSettings", SetSettings);
            CommandRouter.Register("lightmapping.getProgress", GetProgress);
        }

        private static object Bake(JToken p)
        {
            bool async_ = p?["async"]?.Value<bool>() ?? true;

            if (async_)
                Lightmapping.BakeAsync();
            else
                Lightmapping.Bake();

            return new { success = true, async_ = async_, message = async_ ? "Baking started (async)" : "Baking complete" };
        }

        private static object Cancel(JToken p)
        {
            Lightmapping.Cancel();
            return new { success = true, message = "Baking cancelled" };
        }

        private static object Clear(JToken p)
        {
            Lightmapping.Clear();
            Lightmapping.ClearDiskCache();
            return new { success = true, message = "Lightmap data cleared" };
        }

        private static object GetSettings(JToken p)
        {
            int bounces = -1;
            bool hasSettings = false;
            try
            {
                var ls = Lightmapping.lightingSettings;
                if (ls != null)
                {
                    bounces = ls.maxBounces;
                    hasSettings = true;
                }
            }
            catch { /* lightingSettings throws if no asset assigned */ }

            return new
            {
                lightmapper = LightmapEditorSettings.lightmapper.ToString(),
                bounces,
                lightmapResolution = LightmapEditorSettings.bakeResolution,
                lightmapPadding = LightmapEditorSettings.padding,
                isRunning = Lightmapping.isRunning,
                hasLightingSettings = hasSettings,
            };
        }

        private static object SetSettings(JToken p)
        {
            if (p["lightmapper"] != null)
            {
                LightmapEditorSettings.lightmapper = (string)p["lightmapper"] switch
                {
                    "ProgressiveGPU" => LightmapEditorSettings.Lightmapper.ProgressiveGPU,
                    "ProgressiveCPU" => LightmapEditorSettings.Lightmapper.ProgressiveCPU,
                    _ => LightmapEditorSettings.lightmapper
                };
            }
            if (p["directSampleCount"] != null) LightmapEditorSettings.directSampleCount = (int)p["directSampleCount"];
            if (p["indirectSampleCount"] != null) LightmapEditorSettings.indirectSampleCount = (int)p["indirectSampleCount"];
            if (p["environmentSampleCount"] != null) LightmapEditorSettings.environmentSampleCount = (int)p["environmentSampleCount"];
            if (p["bounces"] != null) { try { if (Lightmapping.lightingSettings != null) Lightmapping.lightingSettings.maxBounces = (int)p["bounces"]; } catch { } }
            if (p["lightmapResolution"] != null) LightmapEditorSettings.bakeResolution = (float)p["lightmapResolution"];
            if (p["lightmapPadding"] != null) LightmapEditorSettings.padding = (int)p["lightmapPadding"];
            if (p["compressLightmaps"] != null) LightmapEditorSettings.textureCompression = (bool)p["compressLightmaps"];
            if (p["ambientOcclusion"] != null) LightmapEditorSettings.enableAmbientOcclusion = (bool)p["ambientOcclusion"];
            if (p["aoMaxDistance"] != null) LightmapEditorSettings.aoMaxDistance = (float)p["aoMaxDistance"];

            return new { success = true, message = "Lightmapping settings updated" };
        }

        private static object GetProgress(JToken p)
        {
            return new
            {
                isRunning = Lightmapping.isRunning,
                progress = Lightmapping.buildProgress,
            };
        }
    }
}
