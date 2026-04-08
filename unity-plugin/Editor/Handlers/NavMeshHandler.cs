using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace KarnelLabs.MCP
{
    public static class NavMeshHandler
    {
        public static void Register()
        {
            CommandRouter.Register("navmesh.bake", Bake);
            CommandRouter.Register("navmesh.clear", Clear);
            CommandRouter.Register("navmesh.addAgent", AddAgent);
            CommandRouter.Register("navmesh.setAgent", SetAgent);
            CommandRouter.Register("navmesh.addObstacle", AddObstacle);
            CommandRouter.Register("navmesh.setObstacle", SetObstacle);
            CommandRouter.Register("navmesh.findPath", FindPath);
            CommandRouter.Register("navmesh.setDestination", SetDestination);
            CommandRouter.Register("navmesh.getInfo", GetInfo);
            CommandRouter.Register("navmesh.addLink", AddLink);
        }

        private static object Bake(JToken p)
        {
            var settings = NavMesh.GetSettingsByID(0);
            if (p["agentRadius"] != null) settings.agentRadius = p["agentRadius"].Value<float>();
            if (p["agentHeight"] != null) settings.agentHeight = p["agentHeight"].Value<float>();
            if (p["agentSlope"] != null) settings.agentSlope = p["agentSlope"].Value<float>();
            if (p["agentClimb"] != null) settings.agentClimb = p["agentClimb"].Value<float>();

            #pragma warning disable CS0618
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            #pragma warning restore CS0618
            var triangulation = NavMesh.CalculateTriangulation();
            return new { baked = true, vertices = triangulation.vertices.Length, indices = triangulation.indices.Length };
        }

        private static object Clear(JToken p)
        {
            #pragma warning disable CS0618
            UnityEditor.AI.NavMeshBuilder.ClearAllNavMeshes();
            #pragma warning restore CS0618
            return new { cleared = true };
        }

        private static object AddAgent(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            if (go.GetComponent<NavMeshAgent>() != null)
                throw new McpException(-32602, $"'{go.name}' already has NavMeshAgent");

            WorkflowManager.SnapshotObject(go);
            var agent = Undo.AddComponent<NavMeshAgent>(go);
            if (p["speed"] != null) agent.speed = p["speed"].Value<float>();
            if (p["angularSpeed"] != null) agent.angularSpeed = p["angularSpeed"].Value<float>();
            if (p["acceleration"] != null) agent.acceleration = p["acceleration"].Value<float>();
            if (p["stoppingDistance"] != null) agent.stoppingDistance = p["stoppingDistance"].Value<float>();
            if (p["radius"] != null) agent.radius = p["radius"].Value<float>();
            if (p["height"] != null) agent.height = p["height"].Value<float>();

            return new { added = true, gameObject = go.name, speed = agent.speed };
        }

        private static object SetAgent(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent == null) throw new McpException(-32602, $"No NavMeshAgent on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(agent, "MCP: Set NavMeshAgent");
            if (p["speed"] != null) agent.speed = p["speed"].Value<float>();
            if (p["angularSpeed"] != null) agent.angularSpeed = p["angularSpeed"].Value<float>();
            if (p["acceleration"] != null) agent.acceleration = p["acceleration"].Value<float>();
            if (p["stoppingDistance"] != null) agent.stoppingDistance = p["stoppingDistance"].Value<float>();
            if (p["enabled"] != null) agent.enabled = p["enabled"].Value<bool>();

            return new { gameObject = go.name, speed = agent.speed, enabled = agent.enabled };
        }

        private static object AddObstacle(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);
            var obstacle = Undo.AddComponent<NavMeshObstacle>(go);

            if (p["carving"] != null) obstacle.carving = p["carving"].Value<bool>();
            if (p["shape"] != null)
            {
                var shape = p["shape"].Value<string>();
                obstacle.shape = shape.ToLower() == "capsule" ? NavMeshObstacleShape.Capsule : NavMeshObstacleShape.Box;
            }
            if (p["size"] != null)
                obstacle.size = new Vector3(p["size"]["x"]?.Value<float>() ?? 1, p["size"]["y"]?.Value<float>() ?? 1, p["size"]["z"]?.Value<float>() ?? 1);

            return new { added = true, gameObject = go.name, carving = obstacle.carving };
        }

        private static object SetObstacle(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var obstacle = go.GetComponent<NavMeshObstacle>();
            if (obstacle == null) throw new McpException(-32602, $"No NavMeshObstacle on '{go.name}'");

            WorkflowManager.SnapshotObject(go);
            Undo.RecordObject(obstacle, "MCP: Set NavMeshObstacle");
            if (p["carving"] != null) obstacle.carving = p["carving"].Value<bool>();
            if (p["enabled"] != null) obstacle.enabled = p["enabled"].Value<bool>();

            return new { gameObject = go.name, carving = obstacle.carving };
        }

        private static object FindPath(JToken p)
        {
            var from = new Vector3(p["from"]["x"]?.Value<float>() ?? 0, p["from"]["y"]?.Value<float>() ?? 0, p["from"]["z"]?.Value<float>() ?? 0);
            var to = new Vector3(p["to"]["x"]?.Value<float>() ?? 0, p["to"]["y"]?.Value<float>() ?? 0, p["to"]["z"]?.Value<float>() ?? 0);

            var path = new NavMeshPath();
            var found = NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
            return new
            {
                found,
                status = path.status.ToString(),
                corners = path.corners.Select(c => new { c.x, c.y, c.z }).ToArray(),
            };
        }

        private static object SetDestination(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent == null) throw new McpException(-32602, $"No NavMeshAgent on '{go.name}'");
            if (!agent.isOnNavMesh)
                throw new McpException(-32602, $"NavMeshAgent on '{go.name}' is not placed on a NavMesh. Bake NavMesh first or move the agent onto the NavMesh surface.");

            var dest = new Vector3(
                p["destination"]["x"]?.Value<float>() ?? 0,
                p["destination"]["y"]?.Value<float>() ?? 0,
                p["destination"]["z"]?.Value<float>() ?? 0);
            var success = agent.SetDestination(dest);
            return new { gameObject = go.name, destinationSet = success, destination = new { dest.x, dest.y, dest.z } };
        }

        private static object GetInfo(JToken p)
        {
            var triangulation = NavMesh.CalculateTriangulation();
            var agents = UnityEngine.Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
            var obstacles = UnityEngine.Object.FindObjectsByType<NavMeshObstacle>(FindObjectsSortMode.None);
            return new
            {
                hasNavMesh = triangulation.vertices.Length > 0,
                vertices = triangulation.vertices.Length,
                agentCount = agents.Length,
                obstacleCount = obstacles.Length,
            };
        }

        private static object AddLink(JToken p)
        {
            var go = GameObjectFinder.FindOrThrow(p);
            WorkflowManager.SnapshotObject(go);
            #pragma warning disable CS0618
            var link = Undo.AddComponent<OffMeshLink>(go);

            if (p["costOverride"] != null) link.costOverride = p["costOverride"].Value<float>();
            if (p["bidirectional"] != null) link.biDirectional = p["bidirectional"].Value<bool>();
            link.activated = true;
            #pragma warning restore CS0618

            return new { added = true, gameObject = go.name, type = "OffMeshLink" };
        }
    }
}
