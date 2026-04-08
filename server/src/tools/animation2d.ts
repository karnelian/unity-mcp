import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { z } from "zod";
import { UnityBridge } from "../bridge/unity-bridge.js";

export function registerAnimation2DTools(server: McpServer, bridge: UnityBridge) {

  server.tool(
    "unity_animation2d_createClip",
    "Create a new AnimationClip asset for 2D sprite animation. Returns the created clip path.",
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
    "Get detailed information about an AnimationClip including bindings, length, and settings.",
    {
      path: z.string().describe("Asset path of the AnimationClip"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.getClipInfo", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_findClips",
    "Search for AnimationClip assets in the project by name.",
    {
      nameFilter: z.string().optional().describe("Name filter for search"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.findClips", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_setSpriteKeyframes",
    "Set sprite keyframes on an AnimationClip for frame-by-frame 2D animation. Provide an array of sprite asset paths.",
    {
      clipPath: z.string().describe("Path to the AnimationClip asset"),
      spritePaths: z.array(z.string()).describe("Array of sprite asset paths in frame order"),
      gameObjectPath: z.string().optional().describe("Relative path of the target GameObject in hierarchy"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.setSpriteKeyframes", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_setClipSettings",
    "Modify AnimationClip settings like loop, sample rate, and speed.",
    {
      path: z.string().describe("Asset path of the AnimationClip"),
      loop: z.boolean().optional().describe("Enable looping"),
      sampleRate: z.number().optional().describe("Frame rate"),
      speed: z.number().optional().describe("Playback speed"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.setClipSettings", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_createSpriteAtlas",
    "Create a new SpriteAtlas asset for optimizing sprite draw calls.",
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
    "Get information about a SpriteAtlas including packing and texture settings.",
    {
      path: z.string().describe("Asset path of the SpriteAtlas"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.getSpriteAtlasInfo", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_addToSpriteAtlas",
    "Add textures, sprites, or folders to an existing SpriteAtlas.",
    {
      atlasPath: z.string().describe("Path to the SpriteAtlas asset"),
      assetPaths: z.array(z.string()).describe("Array of asset/folder paths to add"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.addToSpriteAtlas", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_removeFromSpriteAtlas",
    "Remove assets from a SpriteAtlas.",
    {
      atlasPath: z.string().describe("Path to the SpriteAtlas asset"),
      assetPaths: z.array(z.string()).describe("Array of asset paths to remove"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.removeFromSpriteAtlas", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );

  server.tool(
    "unity_animation2d_sliceSprite",
    "Slice a sprite sheet texture into individual sprites using grid-based slicing.",
    {
      texturePath: z.string().describe("Path to the texture asset"),
      mode: z.enum(["grid"]).optional().describe("Slice mode (currently: grid)"),
      cellWidth: z.number().optional().describe("Cell width in pixels (default: 32)"),
      cellHeight: z.number().optional().describe("Cell height in pixels (default: 32)"),
      padding: z.number().optional().describe("Padding between cells (default: 0)"),
      namePrefix: z.string().optional().describe("Prefix for sprite names"),
    },
    async (p) => {
      const r = await bridge.request("animation2d.sliceSprite", p);
      return { content: [{ type: "text", text: JSON.stringify(r, null, 2) }] };
    }
  );
}
