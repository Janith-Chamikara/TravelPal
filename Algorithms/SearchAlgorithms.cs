using TravelPal.DataStructures;

namespace TravelPal.Algorithms
{
    public static class SearchAlgorithms
    {
        // Linear Search with Boyer-Moore character matching
        public static TravelLocationNode BoyerMooreSearch(TravelLocationNode head, string pattern)
        {
            if (string.IsNullOrEmpty(pattern) || head == null)
                return null;

            pattern = pattern.ToLower();
            int[] badChar = new int[256];

            // Initialize bad character array
            for (int i = 0; i < 256; i++)
                badChar[i] = -1;

            // Fill bad character array
            for (int i = 0; i < pattern.Length; i++)
                badChar[pattern[i]] = i;

            var current = head;
            while (current != null)
            {
                string text = current.LocationName.ToLower();
                int n = text.Length;
                int m = pattern.Length;
                int s = 0;

                while (s <= (n - m))
                {
                    int j = m - 1;

                    while (j >= 0 && pattern[j] == text[s + j])
                        j--;

                    if (j < 0)
                        return current;

                    s += Math.Max(1, j - badChar[text[s + j]]);
                }

                current = current.Next;
            }

            return null;
        }

        // Knuth-Morris-Pratt (KMP) Search
        public static TravelLocationNode KMPSearch(TravelLocationNode head, string pattern)
        {
            if (string.IsNullOrEmpty(pattern) || head == null)
                return null;

            pattern = pattern.ToLower();
            int[] lps = ComputeLPSArray(pattern);
            var current = head;

            while (current != null)
            {
                string text = current.LocationName.ToLower();
                int i = 0; // index for text
                int j = 0; // index for pattern

                while (i < text.Length)
                {
                    if (pattern[j] == text[i])
                    {
                        j++;
                        i++;
                    }

                    if (j == pattern.Length)
                        return current;

                    else if (i < text.Length && pattern[j] != text[i])
                    {
                        if (j != 0)
                            j = lps[j - 1];
                        else
                            i++;
                    }
                }

                current = current.Next;
            }

            return null;
        }

        // Fuzzy Search using Levenshtein Distance
        public static TravelLocationNode FuzzySearch(TravelLocationNode head, string pattern, int maxDistance = 2)
        {
            if (string.IsNullOrEmpty(pattern) || head == null)
                return null;

            pattern = pattern.ToLower();
            var current = head;
            TravelLocationNode bestMatch = null;
            int minDistance = int.MaxValue;

            while (current != null)
            {
                string text = current.LocationName.ToLower();
                int distance = LevenshteinDistance(pattern, text);

                if (distance <= maxDistance && distance < minDistance)
                {
                    minDistance = distance;
                    bestMatch = current;
                }

                current = current.Next;
            }

            return bestMatch;
        }

        // Helper method for KMP
        private static int[] ComputeLPSArray(string pattern)
        {
            int[] lps = new int[pattern.Length];
            int len = 0;
            int i = 1;

            while (i < pattern.Length)
            {
                if (pattern[i] == pattern[len])
                {
                    len++;
                    lps[i] = len;
                    i++;
                }
                else
                {
                    if (len != 0)
                        len = lps[len - 1];
                    else
                    {
                        lps[i] = 0;
                        i++;
                    }
                }
            }

            return lps;
        }

        // Helper method for Fuzzy Search
        private static int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,     // deletion
                        d[i, j - 1] + 1),    // insertion
                        d[i - 1, j - 1] + cost); // substitution
                }
            }

            return d[s1.Length, s2.Length];
        }
    }
}