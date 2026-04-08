import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

const vec3 = z.object({ x: z.number(), y: z.number(), z: z.number() });

export function registerPlacementTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_placement_align",
    "여러 오브젝트를 특정 축으로 정렬합니다.",
    {
      names: z.array(z.string()).optional().describe("오브젝트 이름 배열"),
      paths: z.array(z.string()).optional().describe("오브젝트 경로 배열"),
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
    "3개 이상의 오브젝트를 균등 분배합니다.",
    {
      names: z.array(z.string()).optional().describe("오브젝트 이름 배열"),
      paths: z.array(z.string()).optional().describe("오브젝트 경로 배열"),
      axis: z.string().describe("분배 축 (x, y, z)"),
    },
    async (params) => {
      const result = await bridge.request("placement.distribute", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_snap",
    "오브젝트를 그리드에 스냅합니다.",
    {
      path: z.string().optional().describe("오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      gridSize: z.number().optional().describe("그리드 크기 (기본: 1)"),
    },
    async (params) => {
      const result = await bridge.request("placement.snap", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_randomize",
    "오브젝트의 위치/회전/스케일을 랜덤화합니다.",
    {
      path: z.string().optional().describe("오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      positionRange: z.number().optional().describe("위치 랜덤 범위"),
      rotationRange: z.number().optional().describe("회전 랜덤 범위 (도)"),
      scaleRange: z.object({ min: z.number(), max: z.number() }).optional().describe("스케일 범위"),
    },
    async (params) => {
      const result = await bridge.request("placement.randomize", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_circle",
    "오브젝트들을 원형으로 배치합니다.",
    {
      names: z.array(z.string()).optional().describe("오브젝트 이름 배열"),
      paths: z.array(z.string()).optional().describe("오브젝트 경로 배열"),
      radius: z.number().optional().describe("원 반지름 (기본: 5)"),
      center: vec3.optional().describe("원 중심점"),
    },
    async (params) => {
      const result = await bridge.request("placement.circle", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_grid",
    "오브젝트들을 그리드 형태로 배치합니다.",
    {
      names: z.array(z.string()).optional().describe("오브젝트 이름 배열"),
      paths: z.array(z.string()).optional().describe("오브젝트 경로 배열"),
      columns: z.number().optional().describe("열 수 (기본: sqrt(count))"),
      spacing: z.number().optional().describe("간격 (기본: 2)"),
      origin: vec3.optional().describe("시작점"),
    },
    async (params) => {
      const result = await bridge.request("placement.grid", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_stack",
    "오브젝트들을 특정 축으로 쌓습니다.",
    {
      names: z.array(z.string()).optional().describe("오브젝트 이름 배열"),
      paths: z.array(z.string()).optional().describe("오브젝트 경로 배열"),
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
    "오브젝트를 지면에 스냅합니다 (레이캐스트 사용).",
    {
      path: z.string().optional().describe("오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
      layerMask: z.number().optional().describe("레이어 마스크 (기본: 모든 레이어)"),
    },
    async (params) => {
      const result = await bridge.request("placement.groundSnap", params);
      return { content: [{ type: "text", text: JSON.stringify(result, null, 2) }] };
    }
  );

  server.tool(
    "unity_placement_mirror",
    "오브젝트를 축 기준으로 미러링합니다.",
    {
      path: z.string().optional().describe("오브젝트 경로"),
      name: z.string().optional().describe("오브젝트 이름"),
      instanceId: z.number().optional().describe("인스턴스 ID"),
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
    "프리팹을 영역에 랜덤 배치(Scatter)합니다.",
    {
      prefabPath: z.string().describe("프리팹 에셋 경로"),
      count: z.number().optional().describe("배치 개수 (기본: 10)"),
      area: z.number().optional().describe("배치 영역 크기 (기본: 20)"),
      center: vec3.optional().describe("중심점"),
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
