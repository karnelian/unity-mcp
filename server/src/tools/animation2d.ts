import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAnimation2DTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_animation2d_createClip",
    "Create 2D AnimationClip",
    {
      savePath: z.string().optional().describe("Save path (e.g. 'Assets/Animations/Walk.anim')"),
      sampleRate: z.number().optional().describe("Frame rate (default: 12)"),
      loop: z.boolean().optional().describe("Loop the animation (default: true)"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.createClip", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_getClipInfo",
    "Get AnimationClip info",
    {
      path: z.string(),
    },
    async (p) => {
      const r = await bridge.request("animation2d.getClipInfo", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_findClips",
    "Find AnimationClips",
    {
      nameFilter: z.string().optional(),
    },
    async (p) => {
      const r = await bridge.request("animation2d.findClips", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_setSpriteKeyframes",
    "Set sprite keyframes",
    {
      clipPath: z.string(),
      spritePaths: z.array(z.string()),
      gameObjectPath: z.string().optional(),
    },
    async (p) => {
      const r = await bridge.request("animation2d.setSpriteKeyframes", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_setClipSettings",
    "Set AnimationClip settings",
    {
      path: z.string(),
      loop: z.boolean().optional(),
      sampleRate: z.number().optional(),
      speed: z.number().optional(),
    },
    async (p) => {
      const r = await bridge.request("animation2d.setClipSettings", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_createSpriteAtlas",
    "Create SpriteAtlas",
    {
      savePath: z.string().optional().describe("Save path (e.g. 'Assets/Atlas/UI.spriteatlas')"),
      includeInBuild: z.boolean().optional().describe("Include in build (default: true)"),
      enableRotation: z.boolean().optional().describe("Allow rotation in packing (default: false)"),
      enableTightPacking: z.boolean().optional().describe("Enable tight packing (default: false)"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.createSpriteAtlas", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_getSpriteAtlasInfo",
    "Get SpriteAtlas info",
    {
      path: z.string(),
    },
    async (p) => {
      const r = await bridge.request("animation2d.getSpriteAtlasInfo", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_addToSpriteAtlas",
    "Add to SpriteAtlas",
    {
      atlasPath: z.string(),
      assetPaths: z.array(z.string()),
    },
    async (p) => {
      const r = await bridge.request("animation2d.addToSpriteAtlas", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_removeFromSpriteAtlas",
    "Remove from SpriteAtlas",
    {
      atlasPath: z.string(),
      assetPaths: z.array(z.string()),
    },
    async (p) => {
      const r = await bridge.request("animation2d.removeFromSpriteAtlas", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_sliceSprite",
    "Slice sprite sheet",
    {
      texturePath: z.string(),
      mode: z.enum(["grid"]).optional().describe("Slice mode (currently: grid)"),
      cellWidth: z.number().optional().describe("Cell width in pixels (default: 32)"),
      cellHeight: z.number().optional().describe("Cell height in pixels (default: 32)"),
      padding: z.number().optional().describe("Padding between cells (default: 0)"),
      namePrefix: z.string().optional(),
    },
    async (p) => {
      const r = await bridge.request("animation2d.sliceSprite", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );
}
