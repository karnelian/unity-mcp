import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerCharacterControllerTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_characterController_add", "Add CharacterController", {
    ...goRef,
    height: z.number().optional().describe("Controller height (default: 2)"),
    radius: z.number().optional().describe("Controller radius (default: 0.5)"),
    center: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    slopeLimit: z.number().optional().describe("Max slope angle (default: 45)"),
    stepOffset: z.number().optional().describe("Step offset (default: 0.3)"),
    skinWidth: z.number().optional().describe("Skin width (default: 0.08)"),
    minMoveDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("characterController.add", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_characterController_get", "Get CharacterController", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("characterController.get", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_characterController_set", "Set CharacterController", {
    ...goRef,
    height: z.number().optional(),
    radius: z.number().optional(),
    center: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    slopeLimit: z.number().optional(),
    stepOffset: z.number().optional(),
    skinWidth: z.number().optional(),
    minMoveDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("characterController.set", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });

  server.tool("unity_characterController_find", "Find CharacterControllers", {}, async (p) => {
    const r = await bridge.request("characterController.find", p);
    return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
  });
}
