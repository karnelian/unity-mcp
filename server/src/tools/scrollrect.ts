import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerScrollRectTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_scrollRect_create", "Create ScrollRect", {
    name: z.string().optional(),
    parent: z.string().optional(),
    horizontal: z.boolean().optional(),
    vertical: z.boolean().optional(),
    movementType: z.enum(["Unrestricted", "Elastic", "Clamped"]).optional(),
    elasticity: z.number().optional(),
    inertia: z.boolean().optional(),
    decelerationRate: z.number().optional(),
    scrollSensitivity: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("scrollRect.create", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scrollRect_set", "Set ScrollRect", {
    ...goRef,
    horizontal: z.boolean().optional(),
    vertical: z.boolean().optional(),
    movementType: z.enum(["Unrestricted", "Elastic", "Clamped"]).optional(),
    elasticity: z.number().optional(),
    inertia: z.boolean().optional(),
    decelerationRate: z.number().optional(),
    scrollSensitivity: z.number().optional(),
    normalizedPosition: z.object({ x: z.number(), y: z.number() }).optional(),
  }, async (p) => {
    const r = await bridge.request("scrollRect.set", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scrollRect_getInfo", "Get ScrollRect info", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("scrollRect.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_scrollRect_find", "Find ScrollRects", {}, async (p) => {
    const r = await bridge.request("scrollRect.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
