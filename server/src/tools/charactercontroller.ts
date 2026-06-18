import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

const goRef = {
  name: z.string().optional(),
  path: z.string().optional(),
  instanceId: z.number().optional(),
};

export function registerCharacterControllerTools(server: McpServer, bridge: UnityBridge) {

  server.tool("unity_characterController_add", "Add CharacterController", {
    ...goRef,
    height: z.number().optional(),
    radius: z.number().optional(),
    center: z.object({ x: z.number(), y: z.number(), z: z.number() }).optional(),
    slopeLimit: z.number().optional(),
    stepOffset: z.number().optional(),
    skinWidth: z.number().optional(),
    minMoveDistance: z.number().optional(),
  }, async (p) => {
    const r = await bridge.request("characterController.add", p);
    return textResult(r);
  });

  server.tool("unity_characterController_get", "Get CharacterController", {
    ...goRef,
  }, async (p) => {
    const r = await bridge.request("characterController.get", p);
    return textResult(r);
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
    return textResult(r);
  });

  server.tool("unity_characterController_find", "Find CharacterControllers", {}, async (p) => {
    const r = await bridge.request("characterController.find", p);
    return textResult(r);
  });
}
