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

        // Lightmapping.lightingSettings throws InvalidOperationException when no LightingSettings
        // asset is assigned to the active scene. This helper returns null instead of throwing.
        private static LightingSettings TryGetLS()
        {
            try { return Lightmapping.lightingSettings; }
            catch { return null; }
        }

        private static object GetSettings(JToken p)
        {
            var ls = TryGetLS();
            if (ls == null)
            {
                return new
                {
                    isRunning = Lightmapping.isRunning,
                    hasLightingSettings = false,
                    message = "No LightingSettings asset assigned to active scene",
                };
            }

            return new
            {
                lightmapper = ls.lightmapper.ToString(),
                bounces = ls.maxBounces,
                lightmapResolution = ls.lightmapResolution,
                lightmapPadding = ls.lightmapPadding,
                isRunning = Lightmapping.isRunning,
                hasLightingSettings = true,
            };
        }

        private static object SetSettings(JToken p)
        {
            var ls = TryGetLS();
            if (ls == null)
                return new { success = false, message = "No LightingSettings asset assigned to active scene" };

            if (p["lightmapper"] != null)
            {
                ls.lightmapper = (string)p["lightmapper"] switch
                {
                    "ProgressiveGPU" => LightingSettings.Lightmapper.ProgressiveGPU,
                    "ProgressiveCPU" => LightingSettings.Lightmapper.ProgressiveCPU,
                    _ => ls.lightmapper
                };
            }
            if (p["directSampleCount"] != null) ls.directSampleCount = (int)p["directSampleCount"];
            if (p["indirectSampleCount"] != null) ls.indirectSampleCount = (int)p["indirectSampleCount"];
            if (p["environmentSampleCount"] != null) ls.environmentSampleCount = (int)p["environmentSampleCount"];
            if (p["bounces"] != null) ls.maxBounces = (int)p["bounces"];
            if (p["lightmapResolution"] != null) ls.lightmapResolution = (float)p["lightmapResolution"];
            if (p["lightmapPadding"] != null) ls.lightmapPadding = (int)p["lightmapPadding"];
            if (p["compressLightmaps"] != null)
                ls.lightmapCompression = (bool)p["compressLightmaps"]
                    ? LightmapCompression.NormalQuality
                    : LightmapCompression.None;
            if (p["ambientOcclusion"] != null) ls.ao = (bool)p["ambientOcclusion"];
            if (p["aoMaxDistance"] != null) ls.aoMaxDistance = (float)p["aoMaxDistance"];

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
