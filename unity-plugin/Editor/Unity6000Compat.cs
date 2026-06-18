using UnityEngine;

namespace KarnelLabs.MCP
{
    internal static class Unity6000Compat
    {
        public static int GetInstanceIdCompat(this Object obj)
        {
            if (obj == null) return 0;
#if UNITY_6000_5_OR_NEWER
            // Unity 6000.5 deprecates Object.GetInstanceID() in favor of EntityId.
            // Existing MCP schemas still expose instanceId as int, so use a stable session hash
            // until the public schema can migrate to a long/string entityId field.
            return obj.GetEntityId().GetHashCode();
#else
            return obj.GetInstanceID();
#endif
        }
    }
}
