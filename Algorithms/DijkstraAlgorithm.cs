using System.Collections.Generic;
using TravelPal.DataStructures;


namespace TravelPal.Algorithms
{
    public class DijkstraAlgorithm
    {
        private readonly IEnumerable<Node> nodes;

        public DijkstraAlgorithm(IEnumerable<Node> nodes)
        {
            this.nodes = nodes;
        }

        public List<Node> FindPath(Node start, Node end)
        {
            var distances = new Dictionary<Node, double>();
            var previous = new Dictionary<Node, Node>();
            var unvisited = new HashSet<Node>();

            foreach (var node in nodes)
            {
                distances[node] = double.MaxValue;
                unvisited.Add(node);
            }

            distances[start] = 0;

            while (unvisited.Count > 0)
            {
                Node current = FindMinDistanceNode(unvisited, distances);

                if (current == null || current == end)
                    break;

                unvisited.Remove(current);

                foreach (var neighbor in current.Neighbors)
                {
                    double alt = distances[current] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = current;
                    }
                }
            }

            return ReconstructPath(previous, end);
        }

        private Node FindMinDistanceNode(HashSet<Node> unvisited, Dictionary<Node, double> distances)
        {
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

            return current;
        }

        private List<Node> ReconstructPath(Dictionary<Node, Node> previous, Node end)
        {
            var path = new List<Node>();
            Node currentNode = end;

            while (currentNode != null)
            {
                path.Insert(0, currentNode);
                previous.TryGetValue(currentNode, out currentNode);
            }

            return path.Count > 1 ? path : null;
        }
    }
}