import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerSceneViewTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_sceneView_setCamera", "Set the Scene View camera position and rotation.", {
    position: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    rotation: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional().describe("Euler angles"),
    size: z.number().optional().describe("Orthographic size / distance"),
    orthographic: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("sceneView.setCamera", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_frame", "Frame a GameObject in the Scene View.", {
    name: z.string().optional().describe("GameObject name"),
    path: z.string().optional().describe("GameObject path"),
    instanceId: z.number().optional().describe("Instance ID"),
    instant: z.boolean().optional().describe("Instant framing without animation"),
  }, async (p) => {
    const r = await bridge.request("sceneView.frame", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_toggle2D", "Toggle 2D mode in the Scene View.", {
    enable: z.boolean().describe("Enable 2D mode"),
  }, async (p) => {
    const r = await bridge.request("sceneView.toggle2D", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_setGizmos", "Toggle gizmo visibility in the Scene View.", {
    showGizmos: z.boolean().optional(),
    showGrid: z.boolean().optional(),
    showSelectionOutline: z.boolean().optional(),
    showSelectionWire: z.boolean().optional(),
  }, async (p) => {
    const r = await bridge.request("sceneView.setGizmos", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_align", "Align Scene View to the current selection or a specific view.", {
    alignTo: z.enum(["Selection", "Front", "Back", "Left", "Right", "Top", "Bottom"]).describe("What to align to"),
  }, async (p) => {
    const r = await bridge.request("sceneView.align", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_sceneView_getInfo", "Get current Scene View camera information.", {}, async (p) => {
    const r = await bridge.request("sceneView.getInfo", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
