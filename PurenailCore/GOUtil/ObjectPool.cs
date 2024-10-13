using System.Collections.Generic;

namespace PurenailCore.GOUtil;

public class ObjectPool<T>
{
    public delegate T Producer();

    private Queue<T> queue = new();
    private Producer producer;

    public ObjectPool(Producer producer, int prewarm = 0)
    {
        this.producer = producer;
        for (int i = 0; i < prewarm; i++) queue.Enqueue(producer());
    }

    public T Lease() => queue.Count > 0 ? queue.Dequeue() : producer();

    public void Return(T item) => queue.Enqueue(item);
}
