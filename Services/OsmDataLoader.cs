using System;
using System.IO;
using OsmSharp.Streams;
using TravelPal.DataStructures;

namespace TravelPal.Services
{
    public class OsmDataLoader
    {
        private readonly CustomGraph _graph;

        public OsmDataLoader(CustomGraph graph)
        {
            _graph = graph;
        }

        public void LoadOsmData(string pbfFilePath)
        {
            using (var fileStream = File.OpenRead(pbfFilePath))
            {
                var source = new PBFOsmStreamSource(fileStream);

                foreach (var element in source)
                {
                    if (element.Type == OsmSharp.OsmGeoType.Node)
                    {
                        var osmNode = element as OsmSharp.Node;
                        if (osmNode?.Latitude != null && osmNode?.Longitude != null)
                        {
                            _graph.AddNode(osmNode.Id.Value,
                                        osmNode.Latitude.Value,
                                        osmNode.Longitude.Value);
                        }
                    }
                }

                fileStream.Position = 0;
                source = new PBFOsmStreamSource(fileStream);

                foreach (var element in source)
                {
                    if (element.Type == OsmSharp.OsmGeoType.Way)
                    {
                        var way = element as OsmSharp.Way;
                        if (way?.Nodes != null && IsWalkable(way))
                        {
                            ProcessWay(way);
                        }
                    }
                }
            }
        }

        private bool IsWalkable(OsmSharp.Way way)
        {
            if (way.Tags == null) return false;

            var highway = way.Tags.GetValue("highway");
            if (string.IsNullOrEmpty(highway))
            {
                // Check for alternative path types
                var route = way.Tags.GetValue("route");
                var footway = way.Tags.GetValue("footway");
                if (!string.IsNullOrEmpty(route) && route == "hiking") return true;
                if (!string.IsNullOrEmpty(footway)) return true;
                return false;
            }

            var walkableHighways = new[]
            {
        // Existing types
        "footway", "path", "pedestrian", "steps", "residential",
        "service", "track", "unclassified", "living_street",
        "primary", "secondary", "tertiary",
        // Additional types common in Sri Lanka
        "trunk", "trunk_link", "primary_link", "secondary_link",
        "tertiary_link", "road", "trail", "pathway",
        "cycleway", "bridleway", "corridor"
    };

            return walkableHighways.Contains(highway.ToLowerInvariant());
        }

        private void ProcessWay(OsmSharp.Way way)
        {
            for (int i = 0; i < way.Nodes.Length - 1; i++)
            {
                _graph.AddEdge(way.Nodes[i], way.Nodes[i + 1]);
            }
        }
    }
}