using TravelPal.Algorithms;

namespace TravelPal.DataStructures
{
    public class TravelLocationNode
    {
        public string LocationName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public TravelLocationNode Next { get; set; }
        public TravelLocationNode Previous { get; set; }

        public TravelLocationNode(string name, double lat, double lon, DateTime createdAt)
        {
            LocationName = name;
            Latitude = lat;
            Longitude = lon;
            CreatedAt = createdAt;
            Next = null;
            Previous = null;
        }
    }

    public class TravelLocationList
    {
        private TravelLocationNode head;
        private TravelLocationNode tail;
        public int Count { get; private set; }

        public TravelLocationList()
        {
            head = null;
            tail = null;
            Count = 0;
        }

        public void Clear()
        {
            head = null;
            tail = null;
            Count = 0;
        }

        public void AddLocation(string name, double lat, double lon, DateTime createdAt)
        {
            var newNode = new TravelLocationNode(name, lat, lon, createdAt);

            if (head == null)
            {
                head = newNode;
                tail = newNode;
            }
            else
            {
                tail.Next = newNode;
                newNode.Previous = tail;
                tail = newNode;
            }
            Count++;
        }

        public bool RemoveLocation(string name)
        {
            var current = head;
            while (current != null)
            {
                if (current.LocationName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (current.Previous != null)
                        current.Previous.Next = current.Next;
                    else
                        head = current.Next;

                    if (current.Next != null)
                        current.Next.Previous = current.Previous;
                    else
                        tail = current.Previous;

                    Count--;
                    return true;
                }
                current = current.Next;
            }
            return false;
        }

        public enum SearchAlgorithmType
        {
            BoyerMoore,
            KMP,
            Fuzzy
        }

        public TravelLocationNode SearchLocation(string name, SearchAlgorithmType algorithm = SearchAlgorithmType.BoyerMoore)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            switch (algorithm)
            {
                case SearchAlgorithmType.BoyerMoore:
                    return SearchAlgorithms.BoyerMooreSearch(head, name);

                case SearchAlgorithmType.KMP:
                    return SearchAlgorithms.KMPSearch(head, name);

                case SearchAlgorithmType.Fuzzy:
                    return SearchAlgorithms.FuzzySearch(head, name);

                default:
                    return SearchAlgorithms.BoyerMooreSearch(head, name);
            }
        }

        public List<TravelLocationNode> GetAllLocations()
        {
            var locations = new List<TravelLocationNode>();
            var current = head;
            while (current != null)
            {
                locations.Add(current);
                current = current.Next;
            }
            return locations;
        }
    }
}