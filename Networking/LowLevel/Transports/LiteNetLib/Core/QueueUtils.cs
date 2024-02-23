using System.Collections.Concurrent;

namespace KorpiEngine.Networking.LowLevel.Transports.LiteNetLib.Core;

public static class QueueUtils
{
    /// <summary>
    /// Clears a ConcurrentQueue of any type.
    /// </summary>
    internal static void ClearGenericQueue<T>(ref ConcurrentQueue<T> queue)
    {
        while (queue.TryDequeue(out _))
        {
        }
    }


    /// <summary>
    /// Clears a queue using Packet type.
    /// </summary>
    /// <param name="queue"></param>
    internal static void ClearPacketQueue(ref ConcurrentQueue<Packet> queue)
    {
        while (queue.TryDequeue(out Packet p))
            p.Dispose();
    }


    /// <summary>
    /// Clears a queue using Packet type.
    /// </summary>
    /// <param name="queue"></param>
    internal static void ClearPacketQueue(ref Queue<Packet> queue)
    {
        int count = queue.Count;
        for (int i = 0; i < count; i++)
        {
            Packet p = queue.Dequeue();
            p.Dispose();
        }
    }
}