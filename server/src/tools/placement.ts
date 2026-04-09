import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerPlacementTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_placement_align",
    "Align objects",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      axis: z.string().describe("정렬 축 (x, y, z)"),
      mode: z.string().optional().describe("정렬 기준 (first, center, min, max; 기본: first)"),
    },
    async (params) => {
      const result = await bridge.request("placement.align", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_distribute",
    "Distribute objects",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      axis: z.string().describe("분배 축 (x, y, z)"),
    },
    async (params) => {
      const result = await bridge.request("placement.distribute", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_snap",
    "Snap to grid",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      gridSize: z.number().optional().describe("그리드 크기 (기본: 1)"),
    },
    async (params) => {
      const result = await bridge.request("placement.snap", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
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
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_circle",
    "Circle placement",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      radius: z.number().optional().describe("원 반지름 (기본: 5)"),
      center: vec3.optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.circle", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_grid",
    "Grid placement",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      columns: z.number().optional().describe("열 수 (기본: sqrt(count))"),
      spacing: z.number().optional().describe("간격 (기본: 2)"),
      origin: vec3.optional(),
    },
    async (params) => {
      const result = await bridge.request("placement.grid", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_stack",
    "Stack objects",
    {
      names: z.array(z.string()).optional(),
      paths: z.array(z.string()).optional(),
      axis: z.string().optional().describe("쌓기 축 (기본: y)"),
      gap: z.number().optional().describe("간격 (기본: 0)"),
    },
    async (params) => {
      const result = await bridge.request("placement.stack", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_groundSnap",
    "Ground snap",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      layerMask: z.number().optional().describe("레이어 마스크 (기본: 모든 레이어)"),
    },
    async (params) => {
      const result = await bridge.request("placement.groundSnap", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_mirror",
    "Mirror objects",
    {
      path: z.string().optional(),
      name: z.string().optional(),
      instanceId: z.number().optional(),
      axis: z.string().optional().describe("미러 축 (기본: x)"),
      pivot: z.number().optional().describe("피봇 좌표값 (기본: 0)"),
    },
    async (params) => {
      const result = await bridge.request("placement.mirror", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_scatter",
    "Scatter prefabs",
    {
      prefabPath: z.string(),
      count: z.number().optional().describe("배치 개수 (기본: 10)"),
      area: z.number().optional().describe("배치 영역 크기 (기본: 20)"),
      center: vec3.optional(),
      groundSnap: z.boolean().optional().describe("지면 스냅 여부 (기본: false)"),
      randomRotation: z.boolean().optional().describe("Y축 랜덤 회전 (기본: false)"),
      scaleMin: z.number().optional().describe("최소 스케일 (기본: 1)"),
      scaleMax: z.number().optional().describe("최대 스케일 (기본: 1)"),
    },
    async (params) => {
      const result = await bridge.request("placement.scatter", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );
}
