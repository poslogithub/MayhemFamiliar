// SharedQueue.cs
using System.Collections.Concurrent;

namespace MayhemFamiliar.QueueManager
{
    // LogTailer -> JsonParser
    internal static class JsonQueue
    {
        public static ConcurrentQueue<string> Queue { get; } = new ConcurrentQueue<string>();
    }

    // JsonParser -> DialogGenerator
    internal static class EventQueue
    {
        public static ConcurrentQueue<string> Queue { get; } = new ConcurrentQueue<string>();
    }

    // DialogGenerator -> Speaker
    internal static class DialogueQueue
    {
        public static ConcurrentQueue<string> Queue { get; } = new ConcurrentQueue<string>();
    }

}