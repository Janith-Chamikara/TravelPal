using System;
using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<KeyValuePair<double, T>> _elements = new List<KeyValuePair<double, T>>();

    public void Enqueue(T item, double priority)
    {
        _elements.Add(new KeyValuePair<double, T>(priority, item));
        _elements.Sort((a, b) => a.Key.CompareTo(b.Key)); // Sort by priority
    }

    public T Dequeue()
    {
        var element = _elements[0];
        _elements.RemoveAt(0);
        return element.Value;
    }

    public bool IsEmpty()
    {
        return _elements.Count == 0;
    }
}
