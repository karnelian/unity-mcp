import { z } from "zod";

export const safetyControls = {
  dryRun: z.boolean().optional().describe("Preview this mutating Unity operation without applying changes. The Unity plugin returns risk metadata and wouldExecute details when supported."),
  confirmationToken: z.string().optional().describe("Required only when the Unity plugin is configured with UNITY_MCP_REQUIRE_CONFIRMATION=1 or MCP_REQUIRE_CONFIRMATION=1 and this operation is high-risk. Get the expected value from unity_mcp_safety_describe or unity_mcp_safety_manifest."),
};

export type SafetyControlParams = {
  dryRun?: boolean;
  confirmationToken?: string;
};

export function withSafety<T extends z.ZodRawShape>(shape: T): T & typeof safetyControls {
  return { ...shape, ...safetyControls };
}

export type SafetyRiskLevel = "low" | "medium" | "high";
export type SafetyOperationKind = "read-only" | "mutating" | "destructive";

export interface SafetyManifestEntry {
  tool: string;
  method: string;
  riskLevel: SafetyRiskLevel;
  kind: SafetyOperationKind;
  supportsDryRun: boolean;
  confirmationToken?: string;
  notes?: string;
}

export const safetyManifest: SafetyManifestEntry[] = [
  { tool: "unity_scene_create", method: "scene.create", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_setTransform", method: "scene.setTransform", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_addComponent", method: "scene.addComponent", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_setComponent", method: "scene.setComponent", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_delete", method: "scene.delete", riskLevel: "high", kind: "destructive", supportsDryRun: true, confirmationToken: "confirm:scene:delete" },
  { tool: "unity_scene_duplicate", method: "scene.duplicate", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_manage", method: "scene.manage", riskLevel: "medium", kind: "mutating", supportsDryRun: true, notes: "action=info is read-only; save/open/create can mutate editor state." },
  { tool: "unity_scene_setActive", method: "scene.setActiveScene", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_moveToScene", method: "scene.moveToScene", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_openAdditive", method: "scene.openAdditive", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_close", method: "scene.close", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_saveAs", method: "scene.saveAs", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_undo", method: "scene.undo", riskLevel: "high", kind: "destructive", supportsDryRun: true, confirmationToken: "confirm:scene:undo" },
  { tool: "unity_scene_redo", method: "scene.redo", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_setSelection", method: "scene.setSelection", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_scene_setParent", method: "scene.setParent", riskLevel: "medium", kind: "mutating", supportsDryRun: true },

  { tool: "unity_script_create", method: "script.create", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_script_edit", method: "script.edit", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_script_writeAndCompile", method: "script.writeAndCompile", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_script_delete", method: "script.delete", riskLevel: "high", kind: "destructive", supportsDryRun: true, confirmationToken: "confirm:script:delete" },
  { tool: "unity_script_rename", method: "script.rename", riskLevel: "medium", kind: "mutating", supportsDryRun: true },

  { tool: "unity_asset_createMaterial", method: "asset.createMaterial", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_asset_createPrefab", method: "asset.createPrefab", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_asset_importSettings", method: "asset.importSettings", riskLevel: "medium", kind: "mutating", supportsDryRun: true, notes: "action=get is read-only; action=set mutates import settings." },
  { tool: "unity_asset_createFolder", method: "asset.createFolder", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_asset_delete", method: "asset.delete", riskLevel: "high", kind: "destructive", supportsDryRun: true, confirmationToken: "confirm:asset:delete" },
  { tool: "unity_asset_move", method: "asset.move", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_asset_copy", method: "asset.copy", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_asset_refresh", method: "asset.refresh", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_asset_reimport", method: "asset.reimport", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_asset_setLabels", method: "asset.setLabels", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_asset_importPackage", method: "asset.importPackage", riskLevel: "medium", kind: "mutating", supportsDryRun: true },

  { tool: "unity_editor_playMode", method: "editor.playMode", riskLevel: "medium", kind: "mutating", supportsDryRun: true, notes: "action=status is read-only; play/stop/pause/step mutate editor state." },
  { tool: "unity_editor_build", method: "editor.build", riskLevel: "high", kind: "mutating", supportsDryRun: true, confirmationToken: "confirm:editor:build" },
  { tool: "unity_editor_buildSettings", method: "editor.buildSettings", riskLevel: "high", kind: "mutating", supportsDryRun: true, confirmationToken: "confirm:editor:buildSettings", notes: "action=get is read-only; action=set mutates build settings." },
  { tool: "unity_editor_executeMenu", method: "editor.executeMenu", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_editor_runTests", method: "editor.runTests", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_editor_console", method: "editor.console", riskLevel: "medium", kind: "mutating", supportsDryRun: true, notes: "clear=true mutates the console; read without clear is read-only." },
  { tool: "unity_editor_autoRefresh", method: "editor.autoRefresh", riskLevel: "medium", kind: "mutating", supportsDryRun: true },

  { tool: "unity_project_set_quality_level", method: "project.setQualityLevel", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_project_set_player_settings", method: "project.setPlayerSettings", riskLevel: "high", kind: "mutating", supportsDryRun: true, confirmationToken: "confirm:project:setPlayerSettings" },
  { tool: "unity_project_add_tag", method: "project.addTag", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_project_add_layer", method: "project.addLayer", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_project_set_time", method: "project.setTimeSettings", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_project_setBuildTarget", method: "project.setBuildTarget", riskLevel: "high", kind: "mutating", supportsDryRun: true, confirmationToken: "confirm:project:setBuildTarget" },
  { tool: "unity_project_setAndroidSettings", method: "project.setAndroidSettings", riskLevel: "high", kind: "mutating", supportsDryRun: true, confirmationToken: "confirm:project:setAndroidSettings" },
  { tool: "unity_project_setIOSSettings", method: "project.setIOSSettings", riskLevel: "high", kind: "mutating", supportsDryRun: true, confirmationToken: "confirm:project:setIOSSettings" },

  { tool: "unity_workflow_begin", method: "workflow.beginSession", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_workflow_end", method: "workflow.endSession", riskLevel: "medium", kind: "mutating", supportsDryRun: true },
  { tool: "unity_workflow_undo_session", method: "workflow.undoSession", riskLevel: "high", kind: "destructive", supportsDryRun: true, confirmationToken: "confirm:workflow:undoSession" },
  { tool: "unity_workflow_undo_last", method: "workflow.undoLast", riskLevel: "high", kind: "destructive", supportsDryRun: true, confirmationToken: "confirm:workflow:undoLast" },
];

export function describeSafetyControls(): Record<string, unknown> {
  return {
    dryRunParameter: "dryRun",
    confirmationParameter: "confirmationToken",
    confirmationEnv: ["UNITY_MCP_REQUIRE_CONFIRMATION=1", "MCP_REQUIRE_CONFIRMATION=1"],
    confirmationTokenFormat: "confirm:<method-with-dots-replaced-by-colons>",
    entries: safetyManifest,
  };
}
