using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TravelPal.Utils;

namespace TravelPal.DataStructures
{
    public class CustomGraph
    {
        private Dictionary<long, Node> nodes;
        private const double MAX_SEARCH_RADIUS = 5.0;// 1 kilometer radius for nearest node search

        public CustomGraph()
        {
            nodes = new Dictionary<long, Node>();
        }

        public void AddNode(long id, double lat, double lon)
        {
            if (!nodes.ContainsKey(id))
            {
                nodes[id] = new Node(id, lat, lon);
            }
        }

        public void AddEdge(long fromId, long toId)
        {
            if (!nodes.ContainsKey(fromId) || !nodes.ContainsKey(toId))
            {
                Console.WriteLine($"Cannot add edge: Node {fromId} or {toId} not found");
                return;
            }

            Node fromNode = nodes[fromId];
            Node toNode = nodes[toId];

            if (!fromNode.Neighbors.ContainsKey(toNode))
            {
                double distance = DistanceCalculator.CalculateDistance(fromNode, toNode);
                fromNode.Neighbors[toNode] = distance;
                toNode.Neighbors[fromNode] = distance; // Assuming undirected graph
            }
        }

        public List<Node> FindShortestPath(double startLat, double startLon, double endLat, double endLon)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            if (nodes.Count == 0)
            {
                throw new InvalidOperationException("Graph is empty. No nodes loaded.");
            }

            Console.WriteLine($"Finding path from ({startLat}, {startLon}) to ({endLat}, {endLon})");

            Node startNode = FindNearestNode(startLat, startLon);
            Node endNode = FindNearestNode(endLat, endLon);

            if (startNode == null)
            {
                throw new Exception($"No nodes found near start point ({startLat}, {startLon})");
            }
            if (endNode == null)
            {
                throw new Exception($"No nodes found near end point ({endLat}, {endLon})");
            }

            Console.WriteLine($"Found start node: {startNode.Id} ({startNode.Latitude}, {startNode.Longitude})");
            Console.WriteLine($"Found end node: {endNode.Id} ({endNode.Latitude}, {endNode.Longitude})");
            stopwatch.Stop();
            Console.WriteLine($"Execution Time for FindShortestPath: {stopwatch.ElapsedMilliseconds} ms");
            return DijkstraAlgorithm(startNode, endNode);
        }

        private Node FindNearestNode(double lat, double lon)
        {
            if (nodes.Count == 0)
                return null;

            Node nearest = null;
            double minDistance = double.MaxValue;
            Node tempNode = new Node(0, lat, lon);

            // Only search nodes within reasonable distance
            var nearbyNodes = nodes.Values.Where(n =>
            {
                double distance = DistanceCalculator.CalculateDistance(tempNode, n);
                return distance <= MAX_SEARCH_RADIUS;
            });

            foreach (var node in nearbyNodes)
            {
                double distance = DistanceCalculator.CalculateDistance(tempNode, node);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = node;
                }
            }

            if (nearest == null)
            {
                // If no nodes found within MAX_SEARCH_RADIUS, try finding the closest node regardless of distance
                foreach (var node in nodes.Values)
                {
                    double distance = DistanceCalculator.CalculateDistance(tempNode, node);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = node;
                    }
                }
            }

            return nearest;
        }

        private List<Node> DijkstraAlgorithm(Node start, Node end)
        {
            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            var distances = new Dictionary<Node, double>();
            var previous = new Dictionary<Node, Node>();
            var unvisited = new HashSet<Node>();
            var visited = new HashSet<Node>();

            // Initialize only nodes within reasonable distance
            var relevantNodes = nodes.Values.Where(n =>
                CalculateDistance(n, start) <= MAX_SEARCH_RADIUS * 5 ||
                CalculateDistance(n, end) <= MAX_SEARCH_RADIUS * 5);

            foreach (var node in relevantNodes)
            {
                distances[node] = double.MaxValue;
                unvisited.Add(node);
            }

            distances[start] = 0;
            int iterations = 0;
            const int MAX_ITERATIONS = 10000; // Prevent infinite loops

            while (unvisited.Count > 0 && iterations < MAX_ITERATIONS)
            {
                iterations++;
                Node current = null;
                double minDistance = double.MaxValue;

                foreach (var node in unvisited)
                {
                    if (distances[node] < minDistance)
                    {
                        current = node;
                        minDistance = distances[node];
                    }
                }

                if (current == null || current == end)
                    break;

                unvisited.Remove(current);
                visited.Add(current);

                foreach (var neighbor in current.Neighbors)
                {
                    if (visited.Contains(neighbor.Key))
                        continue;

                    double alt = distances[current] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = current;
                    }
                }
            }

            if (iterations >= MAX_ITERATIONS)
            {
                Console.WriteLine("Warning: Maximum iterations reached in path finding");
            }

            // Reconstruct path
            var path = new List<Node>();
            Node currentNode = end;

            while (currentNode != null)
            {
                path.Insert(0, currentNode);
                previous.TryGetValue(currentNode, out currentNode);
            }

            if (path.Count <= 1)
            {
                Console.WriteLine("No valid path found between points");
                return null;
            }

            Console.WriteLine($"Path found with {path.Count} nodes");
            stopwatch.Stop();
            return path;
        }

        private double CalculateDistance(Node node1, Node node2)
        {
            return DistanceCalculator.CalculateDistance(node1, node2);
        }

        public int NodeCount()
        {
            return nodes.Count;
        }

        public bool HasNodes()
        {
            return nodes.Count > 0;
        }

        public IEnumerable<Node> GetAllNodes()
        {
            return nodes.Values;
        }
    }
}