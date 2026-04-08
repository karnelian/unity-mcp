using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace KarnelLabs.MCP
{
    public static class VideoHandler
    {
        public static void Register()
        {
            CommandRouter.Register("video.addPlayer", AddPlayer);
            CommandRouter.Register("video.setPlayer", SetPlayer);
            CommandRouter.Register("video.getInfo", GetInfo);
            CommandRouter.Register("video.find", Find);
            CommandRouter.Register("video.findClips", FindClips);
        }

        private static object AddPlayer(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            Undo.RecordObject(go, "Add VideoPlayer");
            var vp = Undo.AddComponent<VideoPlayer>(go);

            if (p["source"] != null)
            {
                vp.source = (string)p["source"] switch
                {
                    "Url" => VideoSource.Url,
                    _ => VideoSource.VideoClip
                };
            }
            if (p["url"] != null) vp.url = (string)p["url"];
            if (p["clipPath"] != null)
            {
                var clip = AssetDatabase.LoadAssetAtPath<VideoClip>((string)p["clipPath"]);
                if (clip != null) vp.clip = clip;
            }
            if (p["playOnAwake"] != null) vp.playOnAwake = (bool)p["playOnAwake"];
            if (p["isLooping"] != null) vp.isLooping = (bool)p["isLooping"];
            if (p["renderMode"] != null)
            {
                vp.renderMode = (string)p["renderMode"] switch
                {
                    "CameraFarPlane" => VideoRenderMode.CameraFarPlane,
                    "CameraNearPlane" => VideoRenderMode.CameraNearPlane,
                    "RenderTexture" => VideoRenderMode.RenderTexture,
                    "MaterialOverride" => VideoRenderMode.MaterialOverride,
                    _ => vp.renderMode
                };
            }

            return new { success = true, gameObject = go.name, component = "VideoPlayer" };
        }

        private static object SetPlayer(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var vp = go.GetComponent<VideoPlayer>();
            if (vp == null) throw new McpException(-32000, $"No VideoPlayer on '{go.name}'");

            Undo.RecordObject(vp, "Set VideoPlayer");
            if (p["url"] != null) vp.url = (string)p["url"];
            if (p["clipPath"] != null)
            {
                var clip = AssetDatabase.LoadAssetAtPath<VideoClip>((string)p["clipPath"]);
                if (clip != null) vp.clip = clip;
            }
            if (p["playOnAwake"] != null) vp.playOnAwake = (bool)p["playOnAwake"];
            if (p["isLooping"] != null) vp.isLooping = (bool)p["isLooping"];
            if (p["playbackSpeed"] != null) vp.playbackSpeed = (float)p["playbackSpeed"];

            EditorUtility.SetDirty(vp);
            return new { success = true, gameObject = go.name };
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.Find(p);
            var vp = go.GetComponent<VideoPlayer>();
            if (vp == null) throw new McpException(-32000, $"No VideoPlayer on '{go.name}'");

            return new
            {
                gameObject = go.name,
                source = vp.source.ToString(),
                url = vp.url,
                clip = vp.clip != null ? vp.clip.name : null,
                isPlaying = vp.isPlaying,
                isPaused = vp.isPaused,
                isLooping = vp.isLooping,
                playOnAwake = vp.playOnAwake,
                playbackSpeed = vp.playbackSpeed,
                renderMode = vp.renderMode.ToString(),
                length = vp.clip != null ? vp.clip.length : 0,
                frameCount = vp.clip != null ? (long)vp.clip.frameCount : 0,
            };
        }

        private static object Find(JToken p)
        {
            var players = Object.FindObjectsByType<VideoPlayer>(FindObjectsSortMode.None);
            var result = players.Select(vp => new
            {
                gameObject = vp.gameObject.name,
                path = GameObjectFinder.GetPath(vp.gameObject),
                source = vp.source.ToString(),
                clip = vp.clip != null ? vp.clip.name : null,
                isPlaying = vp.isPlaying,
            }).ToArray();

            return new { count = result.Length, players = result };
        }

        private static object FindClips(JToken p)
        {
            string nameFilter = (string)p?["nameFilter"];
            string folder = (string)p?["folder"];

            string searchFilter = "t:VideoClip";
            string[] searchFolders = !string.IsNullOrEmpty(folder) ? new[] { folder } : null;

            var guids = searchFolders != null
                ? AssetDatabase.FindAssets(searchFilter, searchFolders)
                : AssetDatabase.FindAssets(searchFilter);

            var clips = guids.Select(g =>
            {
                string clipPath = AssetDatabase.GUIDToAssetPath(g);
                string clipName = System.IO.Path.GetFileNameWithoutExtension(clipPath);
                return new { name = clipName, path = clipPath };
            });

            if (!string.IsNullOrEmpty(nameFilter))
                clips = clips.Where(c => c.name.Contains(nameFilter, System.StringComparison.OrdinalIgnoreCase));

            var result = clips.ToArray();
            return new { count = result.Length, clips = result };
        }
    }
}
