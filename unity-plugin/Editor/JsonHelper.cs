using Newtonsoft.Json.Linq;
using UnityEngine;

namespace KarnelLabs.MCP
{
    public static class JsonHelper
    {
        public static Vector3 ToVector3(JToken t)
        {
            if (t == null) return Vector3.zero;
            return new Vector3(
                t["x"]?.Value<float>() ?? 0f,
                t["y"]?.Value<float>() ?? 0f,
                t["z"]?.Value<float>() ?? 0f
            );
        }

        public static Color ToColor(JToken t)
        {
            if (t == null) return Color.white;
            return new Color(
                t["r"]?.Value<float>() ?? 1f,
                t["g"]?.Value<float>() ?? 1f,
                t["b"]?.Value<float>() ?? 1f,
                t["a"]?.Value<float>() ?? 1f
            );
        }
    }
}
