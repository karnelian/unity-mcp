import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerSceneViewTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_sceneView_setCamera", "Set SceneView camera", {
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    rotation: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    size: z.number().optional(),
    orthographic: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("sceneView.setCamera", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_frame", "Frame object", {
    name: z.string().optional(),
    path: z.string().optional(),
    instanceId: z.number().optional(),
    instant: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("sceneView.frame", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_toggle2D", "Toggle 2D mode", {
    enable: z.boolean(),
  }, async (p) => {
    const r = await bridge.request("sceneView.toggle2D", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_setGizmos", "Set gizmo visibility", {
    showGizmos: z.boolean().optional(),
    showGrid: z.boolean().optional(),
    showSelectionOutline: z.boolean().optional(),
    showSelectionWire: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("sceneView.setGizmos", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_align", "Align to selection", {
    alignTo: z.enum(["Selection", "Front", "Back", "Left", "Right", "Top", "Bottom"]).describe("What to align to"),
  }, async (p) => {
    const r = await bridge.request("sceneView.align", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_getInfo", "Get SceneView info", {}, async (p) => {
    const r = await bridge.request("sceneView.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
