import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";
import { textResult } from "../utils/format.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerPlacementTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_placement_align",
    "Align objects",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      axis: z.string(),
      mode: z.string().optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.align", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_distribute",
    "Distribute objects",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      axis: z.string(),
    },
    async (params) => {
      const result = await bridge.request("placement.distribute", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_snap",
    "Snap to grid",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      gridSize: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.snap", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_randomize",
    "Randomize transforms",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      positionRange: z.number().optional(),
      rotationRange: z.number().optional(),
      scaleRange: z.object({ min: z.number(), max: z.number() }).optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.randomize", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_circle",
    "Circle placement",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      radius: z.number().optional(),
      center: vec3.optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.circle", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_grid",
    "Grid placement",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      columns: z.number().optional(),
      spacing: z.number().optional(),
      origin: vec3.optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.grid", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_stack",
    "Stack objects",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      axis: z.string().optional(),
      gap: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.stack", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_groundSnap",
    "Ground snap",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      layerMask: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.groundSnap", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_mirror",
    "Mirror objects",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      axis: z.string().optional(),
      pivot: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.mirror", params);
      return textResult(result);
    }
  );

  server.tool(
    "unity_placement_scatter",
    "Scatter prefabs",
    {
      prefabPath: z.string(),
      count: z.number().optional(),
      area: z.number().optional(),
      center: vec3.optional(),
      groundSnap: z.boolean().optional(),
      randomRotation: z.boolean().optional(),
      scaleMin: z.number().optional(),
      scaleMax: z.number().optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.scatter", params);
      return textResult(result);
    }
  );
}
