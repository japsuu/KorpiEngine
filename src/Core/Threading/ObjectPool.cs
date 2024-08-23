using System.Collections.Concurrent;

namespace KorpiEngine.Core.Threading;

/// <summary>
/// Thread-safe object pool.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class ObjectPool<T> : IDisposable where T : class
{
    private readonly ConcurrentBag<T> _objects;
    private readonly SemaphoreSlim _semaphore;
    private readonly Func<T> _objectGenerator;

    public ObjectPool(int capacity, Func<T> objectGenerator)
    {
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be greater than zero.", nameof(capacity));
        }

        _objects = new ConcurrentBag<T>();
        _semaphore = new SemaphoreSlim(capacity, capacity);
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
    }

    public T Get()
    {
        _semaphore.Wait();

        return _objects.TryTake(out T? item) ? item : _objectGenerator();
    }

    public void Return(T item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        _objects.Add(item);
        _semaphore.Release();
    }

    public void Dispose()
    {
        _semaphore.Dispose();
        while (_objects.TryTake(out T? item))
        {
            (item as IDisposable)?.Dispose();
        }
    }
}