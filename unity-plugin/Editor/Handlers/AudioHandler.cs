using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace KarnelLabs.MCP
{
    public static class AudioHandler
    {
        public static void Register()
        {
            CommandRouter.Register("audio.addSource", AddSource);
            CommandRouter.Register("audio.setSource", SetSource);
            CommandRouter.Register("audio.getSource", GetSource);
            CommandRouter.Register("audio.play", Play);
            CommandRouter.Register("audio.stop", Stop);
            CommandRouter.Register("audio.addListener", AddListener);
            CommandRouter.Register("audio.setClip", SetClip);
            CommandRouter.Register("audio.findSources", FindSources);
            CommandRouter.Register("audio.getClipInfo", GetClipInfo);
            CommandRouter.Register("audio.setGlobalVolume", SetGlobalVolume);
            CommandRouter.Register("audio.pause", Pause);
            CommandRouter.Register("audio.unpause", Unpause);
            CommandRouter.Register("audio.playOneShot", PlayOneShot);
            CommandRouter.Register("audio.setMixerGroup", SetMixerGroup);
            CommandRouter.Register("audio.getMixerFloat", GetMixerFloat);
            CommandRouter.Register("audio.setMixerFloat", SetMixerFloat);
        }

        private static object SourceInfo(AudioSource src)
        {
            var go = src.gameObject;
            return new
            {
                name = go.name,
                instanceId = go.GetInstanceID(),
                path = GameObjectFinder.GetPath(go),
                clip = src.clip?.name,
                volume = src.volume,
                pitch = src.pitch,
                loop = src.loop,
                playOnAwake = src.playOnAwake,
                spatialBlend = src.spatialBlend,
                minDistance = src.minDistance,
                maxDistance = src.maxDistance,
                mute = src.mute,
                isPlaying = src.isPlaying,
            };
        }

        private static object AddSource(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);
            var src = Undo.AddComponent<AudioSource>(go);

            if (p["clipPath"] != null)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p["clipPath"].Value<string>());
                if (clip != null) src.clip = clip;
            }
            if (p["volume"] != null) src.volume = p["volume"].Value<float>();
            if (p["pitch"] != null) src.pitch = p["pitch"].Value<float>();
            if (p["loop"] != null) src.loop = p["loop"].Value<bool>();
            if (p["playOnAwake"] != null) src.playOnAwake = p["playOnAwake"].Value<bool>();
            if (p["spatialBlend"] != null) src.spatialBlend = p["spatialBlend"].Value<float>();

            return SourceInfo(src);
        }

        private static object SetSource(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(src, "MCP: Set AudioSource");

            if (p["clipPath"] != null)
            {
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p["clipPath"].Value<string>());
                if (clip != null) src.clip = clip;
            }
            if (p["volume"] != null) src.volume = p["volume"].Value<float>();
            if (p["pitch"] != null) src.pitch = p["pitch"].Value<float>();
            if (p["loop"] != null) src.loop = p["loop"].Value<bool>();
            if (p["playOnAwake"] != null) src.playOnAwake = p["playOnAwake"].Value<bool>();
            if (p["spatialBlend"] != null) src.spatialBlend = p["spatialBlend"].Value<float>();
            if (p["mute"] != null) src.mute = p["mute"].Value<bool>();
            if (p["minDistance"] != null) src.minDistance = p["minDistance"].Value<float>();
            if (p["maxDistance"] != null) src.maxDistance = p["maxDistance"].Value<float>();

            return SourceInfo(src);
        }

        private static object GetSource(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");
            return SourceInfo(src);
        }

        private static object Play(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");
            src.Play();
            return new { playing = true, gameObject = go.name, clip = src.clip?.name };
        }

        private static object Stop(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");
            src.Stop();
            return new { stopped = true, gameObject = go.name };
        }

        private static object AddListener(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (go.GetComponent<AudioListener>() != null)
                throw new McpException(-32602, $"'{go.name}' already has an AudioListener");

            WorkflowManager.SnapshotObject(go);
            Undo.AddComponent<AudioListener>(go);
            return new { added = true, gameObject = go.name };
        }

        private static object SetClip(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");

            var clipPath = Validate.Required<string>(p, "clipPath");
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip == null) throw new McpException(-32003, $"AudioClip not found: {clipPath}");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(src, "MCP: Set AudioClip");
            src.clip = clip;
            return new { gameObject = go.name, clip = clip.name, length = clip.length, channels = clip.channels, frequency = clip.frequency };
        }

        private static object FindSources(JToken p)
        {
            var sources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            return new { count = sources.Length, sources = sources.Select(SourceInfo).ToArray() };
        }

        private static object GetClipInfo(JToken p)
        {
            var clipPath = Validate.Required<string>(p, "clipPath");
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip == null) throw new McpException(-32003, $"AudioClip not found: {clipPath}");
            return new
            {
                name = clip.name,
                path = clipPath,
                length = clip.length,
                channels = clip.channels,
                frequency = clip.frequency,
                samples = clip.samples,
                loadType = clip.loadType.ToString(),
                ambisonic = clip.ambisonic,
            };
        }

        private static object SetGlobalVolume(JToken p)
        {
            var volume = Validate.Required<float>(p, "volume");
            AudioListener.volume = volume;
            return new { globalVolume = AudioListener.volume };
        }

        private static object Pause(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");
            src.Pause();
            return new { gameObject = go.name, paused = true };
        }

        private static object Unpause(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");
            src.UnPause();
            return new { gameObject = go.name, unpaused = true };
        }

        private static object PlayOneShot(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");
            var clipPath = Validate.Required<string>(p, "clipPath");
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip == null) throw new McpException(-32003, $"AudioClip not found: {clipPath}");
            var volume = p["volume"]?.Value<float>() ?? 1f;
            src.PlayOneShot(clip, volume);
            return new { gameObject = go.name, clip = clip.name, volume };
        }

        private static object SetMixerGroup(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var src = go.GetComponent<AudioSource>();
            if (src == null) throw new McpException(-32602, $"No AudioSource on '{go.name}'");
            var mixerPath = Validate.Required<string>(p, "mixerPath");
            var groupName = p["groupName"]?.Value<string>();

            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(mixerPath);
            if (mixer == null) throw new McpException(-32003, $"AudioMixer not found: {mixerPath}");

            AudioMixerGroup[] groups;
            if (!string.IsNullOrEmpty(groupName))
                groups = mixer.FindMatchingGroups(groupName);
            else
                groups = mixer.FindMatchingGroups("Master");

            if (groups.Length == 0) throw new McpException(-32003, $"Group not found: {groupName}");
            Undo.RecordObject(src, "Set Mixer Group");
            src.outputAudioMixerGroup = groups[0];
            return new { gameObject = go.name, mixer = mixer.name, group = groups[0].name };
        }

        private static object GetMixerFloat(JToken p)
        {
            var mixerPath = Validate.Required<string>(p, "mixerPath");
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(mixerPath);
            if (mixer == null) throw new McpException(-32003, $"AudioMixer not found: {mixerPath}");
            var paramName = Validate.Required<string>(p, "parameterName");
            if (mixer.GetFloat(paramName, out float value))
                return new { mixer = mixer.name, parameter = paramName, value };
            throw new McpException(-32602, $"Parameter '{paramName}' not exposed in mixer");
        }

        private static object SetMixerFloat(JToken p)
        {
            var mixerPath = Validate.Required<string>(p, "mixerPath");
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(mixerPath);
            if (mixer == null) throw new McpException(-32003, $"AudioMixer not found: {mixerPath}");
            var paramName = Validate.Required<string>(p, "parameterName");
            var value = Validate.Required<float>(p, "value");
            if (!mixer.SetFloat(paramName, value))
                throw new McpException(-32602, $"Parameter '{paramName}' not exposed in mixer");
            return new { mixer = mixer.name, parameter = paramName, value };
        }
    }
}
