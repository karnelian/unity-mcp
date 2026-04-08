using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public class McpException : Exception
    {
        public int Code { get; }

        public McpException(int code, string message) : base(message)
        {
            Code = code;
        }

        public static McpException NotConnected()
            => new(-32001, "Unity Editor에 연결되지 않았습니다. Unity에서 MCP Server Window를 확인하세요.");

        public static McpException Timeout(string method, int seconds)
            => new(-32002, $"타임아웃: {method} ({seconds}초)");

        public static McpException GameObjectNotFound(string path)
            => new(-32003, $"GameObject not found: {path}");
    }

    public static class CommandRouter
    {
        private static readonly Dictionary<string, Func<JToken, object>> Handlers = new();

        public static void Register(string method, Func<JToken, object> handler)
        {
            Handlers[method] = handler;
        }

        public static void RegisterAll()
        {
            Handlers.Clear(); // 중복 등록 방지
            SceneHandler.Register();
            ScriptHandler.Register();
            AssetHandler.Register();
            BuildHandler.Register();
            DebugHandler.Register();
            ResourceHandler.Register();
            WorkflowHandler.Register();
            MaterialHandler.Register();
            LightHandler.Register();
            CameraHandler.Register();
            PhysicsHandler.Register();
            UIHandler.Register();
            PrefabHandler.Register();
            ComponentHandler.Register();
            AudioHandler.Register();
            AnimatorHandler.Register();
            TerrainHandler.Register();
            NavMeshHandler.Register();
            ShaderHandler.Register();
            ProjectHandler.Register();
            ScriptableObjectHandler.Register();
            TextureHandler.Register();
            ModelHandler.Register();
            PackageHandler.Register();
            ValidationHandler.Register();
            OptimizationHandler.Register();
            PlacementHandler.Register();
            TimelineHandler.Register();
            ProfilerHandler.Register();
            CleanerHandler.Register();
            CinemachineHandler.Register();
            ProBuilderHandler.Register();
            XRHandler.Register();
            PerceptionHandler.Register();
            EventHandler.Register();
            SmartHandler.Register();
            ParticleHandler.Register();
            Tilemap2DHandler.Register();
#if UNITY_RENDER_PIPELINES_CORE
            RenderingHandler.Register();
#endif
#if UNITY_SPLINES
            SplineHandler.Register();
#endif
#if UNITY_VFX_GRAPH
            VFXHandler.Register();
#endif
#if ENABLE_INPUT_SYSTEM
            InputSystemHandler.Register();
#endif
#if UNITY_ADDRESSABLES
            AddressablesHandler.Register();
#endif
            UIToolkitHandler.Register();
            Animation2DHandler.Register();
            VersionControlHandler.Register();
#if UNITY_LOCALIZATION
            LocalizationHandler.Register();
#endif

            // Phase 2: New categories
            JointHandler.Register();
            Physics2DHandler.Register();
            LODHandler.Register();
            CharacterControllerHandler.Register();
#if UNITY_TEXTMESHPRO
            TextMeshProHandler.Register();
#endif
            LightmappingHandler.Register();
            VideoHandler.Register();
            LineRendererHandler.Register();
            ConstraintHandler.Register();
            ScrollRectHandler.Register();
            CanvasGroupHandler.Register();
            UIMaskHandler.Register();
            ClothHandler.Register();
            SortingLayerHandler.Register();
            OcclusionCullingHandler.Register();
            RenderTextureHandler.Register();
            SceneViewHandler.Register();
            AsmdefHandler.Register();
#if UNITY_2D_SPRITESHAPE
            SpriteShapeHandler.Register();
#endif
#if UNITY_2D_ANIMATION
            Skeletal2DHandler.Register();
#endif
#if UNITY_2D_TILEMAP_EXTRAS
            RuleTileHandler.Register();
#endif
            ComputeShaderHandler.Register();
#if UNITY_RENDER_PIPELINES_UNIVERSAL
            RenderFeatureHandler.Register();
#endif
            PresetHandler.Register();
            UnitySearchHandler.Register();
            GridLayoutHandler.Register();
        }

        /// <summary>
        /// 트랜잭션 기반 디스패치.
        /// 매 실행을 Undo 그룹으로 감싸고, 실패 시 자동 롤백.
        /// 요청 후 GameObjectFinder 캐시 무효화.
        /// </summary>
        public static (string id, string response) Dispatch(string rawJson)
        {
            string id = "null";
            string method = null;
            try
            {
                try
                {
                    var obj = JObject.Parse(rawJson);
                    var idToken = obj["id"];
                    if (idToken != null)
                        id = idToken.ToString(Newtonsoft.Json.Formatting.None);
                }
                catch { }

                var request = JsonRpc.ParseRequest(rawJson);
                id = request.GetIdRaw();
                method = request.Method;

                if (string.IsNullOrEmpty(method))
                    return (id, JsonRpc.Error(id, -32600, "Invalid request: missing method"));

                if (!Handlers.TryGetValue(method, out var handler))
                    return (id, JsonRpc.Error(id, -32601, $"Unknown method: {method}"));

                // ── 트랜잭션 시작 ──
                Undo.IncrementCurrentGroup();
                int undoGroup = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName($"MCP: {method}");

                object result;
                try
                {
                    result = handler(request.Params);
                }
                catch
                {
                    // 실패 시 Undo 그룹 롤백
                    Undo.RevertAllInCurrentGroup();
                    throw;
                }
                finally
                {
                    Undo.CollapseUndoOperations(undoGroup);
                    // 요청 후 캐시 무효화
                    GameObjectFinder.InvalidateCache();
                }

                return (id, JsonRpc.Success(id, result));
            }
            catch (McpException ex)
            {
                Debug.LogWarning($"[MCP] McpException ({ex.Code}): {ex.Message}");
                return (id, JsonRpc.Error(id, ex.Code, ex.Message));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP] Error handling '{method}': {ex}");
                return (id, JsonRpc.Error(id, -32000, ex.Message));
            }
        }

        public static int HandlerCount => Handlers.Count;
    }
}
