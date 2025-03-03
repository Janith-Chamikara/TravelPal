namespace TravelPal.Algorithms
{
    public static class SortAlgorithms
    {
        // BubbleSort implementation
        public static void BubbleSort<T>(List<T> items, Func<T, T, int> compareFunc)
        {
            int n = items.Count;
            bool swapped;

            for (int i = 0; i < n - 1; i++)
            {
                swapped = false;
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (compareFunc(items[j], items[j + 1]) > 0)
                    {
                        Swap(items, j, j + 1);
                        swapped = true;
                    }
                }

                if (!swapped)
                    break;
            }
        }


        // QuickSort implementation
        public static void QuickSort<T>(List<T> items, int left, int right,
            Func<T, T, int> compareFunc)
        {
            if (left < right)
            {
                int pivotIndex = Partition(items, left, right, compareFunc);
                QuickSort(items, left, pivotIndex - 1, compareFunc);
                QuickSort(items, pivotIndex + 1, right, compareFunc);
            }
        }

        private static int Partition<T>(List<T> items, int left, int right,
            Func<T, T, int> compareFunc)
        {
            var pivot = items[right];
            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                if (compareFunc(items[j], pivot) <= 0)
                {
                    i++;
                    Swap(items, i, j);
                }
            }

            Swap(items, i + 1, right);
            return i + 1;
        }

        // MergeSort implementation
        public static void MergeSort<T>(List<T> items, Func<T, T, int> compareFunc)
        {
            if (items.Count <= 1)
                return;

            int mid = items.Count / 2;
            List<T> left = items.GetRange(0, mid);
            List<T> right = items.GetRange(mid, items.Count - mid);

            MergeSort(left, compareFunc);
            MergeSort(right, compareFunc);
            Merge(items, left, right, compareFunc);
        }

        private static void Merge<T>(List<T> items, List<T> left, List<T> right,
            Func<T, T, int> compareFunc)
        {
            int leftIndex = 0, rightIndex = 0, targetIndex = 0;

            while (leftIndex < left.Count && rightIndex < right.Count)
            {
                if (compareFunc(left[leftIndex], right[rightIndex]) <= 0)
                {
                    items[targetIndex] = left[leftIndex];
                    leftIndex++;
                }
                else
                {
                    items[targetIndex] = right[rightIndex];
                    rightIndex++;
                }
                targetIndex++;
            }

            while (leftIndex < left.Count)
            {
                items[targetIndex] = left[leftIndex];
                leftIndex++;
                targetIndex++;
            }

            while (rightIndex < right.Count)
            {
                items[targetIndex] = right[rightIndex];
                rightIndex++;
                targetIndex++;
            }
        }

        private static void Swap<T>(List<T> items, int i, int j)
        {
            T temp = items[i];
            items[i] = items[j];
            items[j] = temp;
        }

        // Heap Sort implementation
        public static void HeapSort<T>(List<T> items, Func<T, T, int> compareFunc)
        {
            int n = items.Count;

            // Build heap
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(items, n, i, compareFunc);

            // Extract elements from heap
            for (int i = n - 1; i >= 0; i--)
            {
                Swap(items, 0, i);
                Heapify(items, i, 0, compareFunc);
            }
        }

        private static void Heapify<T>(List<T> items, int n, int i,
            Func<T, T, int> compareFunc)
        {
            int largest = i;
            int left = 2 * i + 1;
            int right = 2 * i + 2;

            if (left < n && compareFunc(items[left], items[largest]) > 0)
                largest = left;

            if (right < n && compareFunc(items[right], items[largest]) > 0)
                largest = right;

            if (largest != i)
            {
                Swap(items, i, largest);
                Heapify(items, n, largest, compareFunc);
            }
        }
    }
}