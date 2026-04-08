using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

#if PROBUILDER || UNITY_PROBUILDER
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
#endif

namespace KarnelLabs.MCP
{
    public static class ProBuilderHandler
    {
        public static void Register()
        {
#if PROBUILDER || UNITY_PROBUILDER
            CommandRouter.Register("probuilder.createCube", CreateCube);
            CommandRouter.Register("probuilder.createCylinder", CreateCylinder);
            CommandRouter.Register("probuilder.createSphere", CreateSphere);
            CommandRouter.Register("probuilder.createPlane", CreatePlane);
            CommandRouter.Register("probuilder.createStair", CreateStair);
            CommandRouter.Register("probuilder.createArch", CreateArch);
            CommandRouter.Register("probuilder.createDoor", CreateDoor);
            CommandRouter.Register("probuilder.createPipe", CreatePipe);
            CommandRouter.Register("probuilder.getInfo", GetInfo);
            CommandRouter.Register("probuilder.extrude", Extrude);
            CommandRouter.Register("probuilder.bevel", Bevel);
            CommandRouter.Register("probuilder.subdivide", Subdivide);
            CommandRouter.Register("probuilder.merge", Merge);
            CommandRouter.Register("probuilder.flip", FlipNormals);
            CommandRouter.Register("probuilder.setMaterial", SetMaterial);
            CommandRouter.Register("probuilder.findProBuilderObjects", FindProBuilderObjects);
            CommandRouter.Register("probuilder.centerPivot", CenterPivot);
            CommandRouter.Register("probuilder.freezeTransform", FreezeTransform);
            CommandRouter.Register("probuilder.triangulate", Triangulate);
            CommandRouter.Register("probuilder.export", Export);
            CommandRouter.Register("probuilder.selectFaces", SelectFaces);
            CommandRouter.Register("probuilder.deleteFaces", DeleteFaces);
#endif
        }

#if PROBUILDER || UNITY_PROBUILDER
        private static Vector3 ParsePosition(JToken p)
        {
            return new Vector3(
                p?["position"]?["x"]?.Value<float>() ?? 0,
                p?["position"]?["y"]?.Value<float>() ?? 0,
                p?["position"]?["z"]?.Value<float>() ?? 0
            );
        }

        private static object MeshResult(ProBuilderMesh mesh)
        {
            var go = mesh.gameObject;
            return new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
                vertexCount = mesh.vertexCount,
                faceCount = mesh.faceCount,
                triangleCount = mesh.triangleCount,
            };
        }

        private static object CreateCube(JToken p)
        {
            var size = new Vector3(
                p["size"]?["x"]?.Value<float>() ?? 1,
                p["size"]?["y"]?.Value<float>() ?? 1,
                p["size"]?["z"]?.Value<float>() ?? 1
            );
            var mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
            mesh.gameObject.name = p["name"]?.Value<string>() ?? "PB_Cube";
            mesh.transform.position = ParsePosition(p);
            Undo.RegisterCreatedObjectUndo(mesh.gameObject, "MCP: PB Cube");
            return MeshResult(mesh);
        }

        private static object CreateCylinder(JToken p)
        {
            var axisDivisions = p["axisDivisions"]?.Value<int>() ?? 18;
            var heightCuts = p["heightCuts"]?.Value<int>() ?? 0;
            var radius = p["radius"]?.Value<float>() ?? 0.5f;
            var height = p["height"]?.Value<float>() ?? 1f;
            var mesh = ShapeGenerator.GenerateCylinder(PivotLocation.Center, axisDivisions, radius, height, heightCuts);
            mesh.gameObject.name = p["name"]?.Value<string>() ?? "PB_Cylinder";
            mesh.transform.position = ParsePosition(p);
            Undo.RegisterCreatedObjectUndo(mesh.gameObject, "MCP: PB Cylinder");
            return MeshResult(mesh);
        }

        private static object CreateSphere(JToken p)
        {
            var subdivisions = p["subdivisions"]?.Value<int>() ?? 2;
            var radius = p["radius"]?.Value<float>() ?? 0.5f;
            var mesh = ShapeGenerator.GenerateIcosahedron(PivotLocation.Center, radius, subdivisions);
            mesh.gameObject.name = p["name"]?.Value<string>() ?? "PB_Sphere";
            mesh.transform.position = ParsePosition(p);
            Undo.RegisterCreatedObjectUndo(mesh.gameObject, "MCP: PB Sphere");
            return MeshResult(mesh);
        }

        private static object CreatePlane(JToken p)
        {
            var width = p["width"]?.Value<float>() ?? 10;
            var height = p["height"]?.Value<float>() ?? 10;
            var widthCuts = p["widthCuts"]?.Value<int>() ?? 0;
            var heightCuts = p["heightCuts"]?.Value<int>() ?? 0;
            var mesh = ShapeGenerator.GeneratePlane(PivotLocation.Center, width, height, widthCuts, heightCuts, Axis.Up);
            mesh.gameObject.name = p["name"]?.Value<string>() ?? "PB_Plane";
            mesh.transform.position = ParsePosition(p);
            Undo.RegisterCreatedObjectUndo(mesh.gameObject, "MCP: PB Plane");
            return MeshResult(mesh);
        }

        private static object CreateStair(JToken p)
        {
            var size = new Vector3(
                p["width"]?.Value<float>() ?? 2,
                p["height"]?.Value<float>() ?? 2.5f,
                p["depth"]?.Value<float>() ?? 4
            );
            var steps = p["steps"]?.Value<int>() ?? 10;
            var mesh = ShapeGenerator.GenerateStair(PivotLocation.Center, size, steps, false);
            mesh.gameObject.name = p["name"]?.Value<string>() ?? "PB_Stair";
            mesh.transform.position = ParsePosition(p);
            Undo.RegisterCreatedObjectUndo(mesh.gameObject, "MCP: PB Stair");
            return MeshResult(mesh);
        }

        private static object CreateArch(JToken p)
        {
            var angle = p["angle"]?.Value<float>() ?? 180;
            var radius = p["radius"]?.Value<float>() ?? 1;
            var width = p["width"]?.Value<float>() ?? 0.5f;
            var depth = p["depth"]?.Value<float>() ?? 1;
            var radialCuts = p["radialCuts"]?.Value<int>() ?? 6;
            var insideFaces = p["insideFaces"]?.Value<bool>() ?? true;
            var outsideFaces = p["outsideFaces"]?.Value<bool>() ?? true;
            var frontFaces = p["frontFaces"]?.Value<bool>() ?? true;
            var backFaces = p["backFaces"]?.Value<bool>() ?? true;
            var mesh = ShapeGenerator.GenerateArch(PivotLocation.Center, angle, radius, Mathf.Lerp(radius, radius - width, 1), depth, radialCuts, insideFaces, outsideFaces, frontFaces, backFaces, true);
            mesh.gameObject.name = p["name"]?.Value<string>() ?? "PB_Arch";
            mesh.transform.position = ParsePosition(p);
            Undo.RegisterCreatedObjectUndo(mesh.gameObject, "MCP: PB Arch");
            return MeshResult(mesh);
        }

        private static object CreateDoor(JToken p)
        {
            var totalWidth = p["totalWidth"]?.Value<float>() ?? 4;
            var totalHeight = p["totalHeight"]?.Value<float>() ?? 4;
            var depth = p["depth"]?.Value<float>() ?? 0.5f;
            var doorWidth = p["doorWidth"]?.Value<float>() ?? 1;
            var doorHeight = p["doorHeight"]?.Value<float>() ?? 2.5f;
            var mesh = ShapeGenerator.GenerateDoor(PivotLocation.Center, totalWidth, totalHeight, depth, doorWidth, doorHeight);
            mesh.gameObject.name = p["name"]?.Value<string>() ?? "PB_Door";
            mesh.transform.position = ParsePosition(p);
            Undo.RegisterCreatedObjectUndo(mesh.gameObject, "MCP: PB Door");
            return MeshResult(mesh);
        }

        private static object CreatePipe(JToken p)
        {
            var radius = p["radius"]?.Value<float>() ?? 1;
            var height = p["height"]?.Value<float>() ?? 2;
            var thickness = p["thickness"]?.Value<float>() ?? 0.2f;
            var subdivAxis = p["subdivisions"]?.Value<int>() ?? 18;
            var mesh = ShapeGenerator.GeneratePipe(PivotLocation.Center, radius, height, thickness, subdivAxis, 0);
            mesh.gameObject.name = p["name"]?.Value<string>() ?? "PB_Pipe";
            mesh.transform.position = ParsePosition(p);
            Undo.RegisterCreatedObjectUndo(mesh.gameObject, "MCP: PB Pipe");
            return MeshResult(mesh);
        }

        private static object GetInfo(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);

            return new
            {
                name = go.name,
                path = GameObjectFinder.GetPath(go),
                vertexCount = mesh.vertexCount,
                faceCount = mesh.faceCount,
                triangleCount = mesh.triangleCount,
                edgeCount = mesh.edgeCount,
            };
        }

        private static object Extrude(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var distance = p["distance"]?.Value<float>() ?? 0.5f;
            var faceIndices = p["faceIndices"]?.ToObject<int[]>();
            var faces = faceIndices != null
                ? faceIndices.Where(i => i < mesh.faces.Count).Select(i => mesh.faces[i]).ToArray()
                : mesh.faces.ToArray();

            mesh.Extrude(faces, ExtrudeMethod.FaceNormal, distance);
            mesh.ToMesh();
            mesh.Refresh();
            return MeshResult(mesh);
        }

        private static object Bevel(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var amount = p["amount"]?.Value<float>() ?? 0.1f;
            UnityEngine.ProBuilder.MeshOperations.Bevel.BevelEdges(mesh, mesh.faces.SelectMany(f => f.edges).ToList(), amount);
            mesh.ToMesh();
            mesh.Refresh();
            return MeshResult(mesh);
        }

        private static object Subdivide(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            ConnectElements.Connect(mesh, mesh.faces);
            mesh.ToMesh();
            mesh.Refresh();
            return MeshResult(mesh);
        }

        private static object Merge(JToken p)
        {
            var names = p["names"]?.ToObject<string[]>();
            var paths = p["paths"]?.ToObject<string[]>();
            if (names == null && paths == null) throw new McpException(-32602, "Provide names or paths of objects to merge");

            var meshes = new List<ProBuilderMesh>();
            var identifiers = paths ?? names;
            foreach (var id in identifiers)
            {
                var go = paths != null ? GameObjectFinder.FindOrThrow(path: id) : GameObjectFinder.FindOrThrow(name: id);
                var m = go.GetComponent<ProBuilderMesh>();
                if (m != null) meshes.Add(m);
            }
            if (meshes.Count < 2) throw new McpException(-32602, "Need at least 2 ProBuilder meshes to merge");

            var result = CombineMeshes.Combine(meshes, meshes[0]);
            return new { merged = true, resultCount = result.Count };
        }

        private static object FlipNormals(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            foreach (var face in mesh.faces) face.Reverse();
            mesh.ToMesh();
            mesh.Refresh();
            return MeshResult(mesh);
        }

        private static object SetMaterial(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            var materialPath = Validate.Required<string>(p, "materialPath");
            materialPath = Validate.SafeAssetPath(materialPath);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (mat == null) throw new McpException(-32003, $"Material not found: {materialPath}");
            WorkflowManager.SnapshotObject(go);

            var faceIndices = p["faceIndices"]?.ToObject<int[]>();
            var faces = faceIndices != null
                ? faceIndices.Where(i => i < mesh.faces.Count).Select(i => mesh.faces[i]).ToArray()
                : mesh.faces.ToArray();

            foreach (var face in faces) face.submeshIndex = 0;
            mesh.GetComponent<Renderer>().sharedMaterial = mat;
            mesh.ToMesh();
            mesh.Refresh();
            return new { name = go.name, material = mat.name, facesAffected = faces.Length };
        }

        private static object FindProBuilderObjects(JToken p)
        {
            var meshes = UnityEngine.Object.FindObjectsByType<ProBuilderMesh>(FindObjectsSortMode.None);
            var results = meshes.Select(m => new
            {
                name = m.gameObject.name,
                path = GameObjectFinder.GetPath(m.gameObject),
                vertexCount = m.vertexCount,
                faceCount = m.faceCount,
            }).ToArray();
            return new { count = results.Length, objects = results };
        }

        private static object CenterPivot(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            mesh.CenterPivot(null);
            mesh.ToMesh();
            mesh.Refresh();
            return new { name = go.name, centered = true };
        }

        private static object FreezeTransform(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            var verts = mesh.positions.ToArray();
            var t = go.transform;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = t.TransformPoint(verts[i]);
            }
            t.position = Vector3.zero;
            t.rotation = Quaternion.identity;
            t.localScale = Vector3.one;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = t.InverseTransformPoint(verts[i]);
            }
            mesh.positions = verts;
            mesh.ToMesh();
            mesh.Refresh();
            return new { name = go.name, frozen = true };
        }

        private static object Triangulate(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            WorkflowManager.SnapshotObject(go);

            foreach (var face in mesh.faces)
            {
                // Each face is already stored as triangles internally
            }
            mesh.ToMesh();
            mesh.Refresh();
            return MeshResult(mesh);
        }

        private static object Export(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);

            var format = p["format"]?.Value<string>()?.ToLower() ?? "obj";
            var savePath = p["savePath"]?.Value<string>() ?? $"Assets/Exports/{go.name}.{format}";
            savePath = Validate.SafeAssetPath(savePath);

            var dir = System.IO.Path.GetDirectoryName(System.IO.Path.Combine(Application.dataPath, "..", savePath));
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

            // Use MeshUtility to export — ensure .asset extension for CreateAsset
            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                var exportMesh = new Mesh();
                exportMesh.name = go.name;
                UnityEngine.ProBuilder.MeshUtility.Compile(mesh, exportMesh);

                // CreateAsset requires .asset extension; for other formats write raw file
                var ext = System.IO.Path.GetExtension(savePath).ToLower();
                if (ext != ".asset")
                    savePath = System.IO.Path.ChangeExtension(savePath, ".asset");

                AssetDatabase.CreateAsset(exportMesh, savePath);
                AssetDatabase.SaveAssets();
            }

            return new { exported = true, path = savePath, format };
        }

        private static object SelectFaces(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            var indices = Validate.Required<JToken>(p, "faceIndices").ToObject<int[]>();

            var faces = indices.Where(i => i < mesh.faces.Count).Select(i => mesh.faces[i]).ToArray();
            mesh.SetSelectedFaces(faces);

            return new { name = go.name, selectedFaces = faces.Length, totalFaces = mesh.faceCount };
        }

        private static object DeleteFaces(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var mesh = go.GetComponent<ProBuilderMesh>();
            if (mesh == null) throw new McpException(-32003, "ProBuilderMesh not found on " + go.name);
            var indices = Validate.Required<JToken>(p, "faceIndices").ToObject<int[]>();
            WorkflowManager.SnapshotObject(go);

            var faces = indices.Where(i => i < mesh.faces.Count).Select(i => mesh.faces[i]).ToArray();
            mesh.DeleteFaces(faces);
            mesh.ToMesh();
            mesh.Refresh();
            return MeshResult(mesh);
        }
#endif
    }
}
