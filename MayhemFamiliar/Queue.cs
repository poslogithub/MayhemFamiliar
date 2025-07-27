// SharedQueue.cs
using System.Collections.Concurrent;

namespace MayhemFamiliar.QueueManager
{
    // LogTailer -> JsonParser
    public static class JsonQueue
    {
        public static ConcurrentQueue<string> Queue { get; } = new ConcurrentQueue<string>();
    }

    // JsonParser -> DialogGenerator
    public static class EventQueue
    {
        public static ConcurrentQueue<string> Queue { get; } = new ConcurrentQueue<string>();
    }

    // DialogGenerator -> Speaker
    public static class DialogueQueue
    {
        public static ConcurrentQueue<string> Queue { get; } = new ConcurrentQueue<string>();
    }

}