// SharedQueue.cs
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace MayhemFamiliar.QueueManager
{
    // LogTailer -> JsonParser
    internal static class JsonQueue
    {
        public static ConcurrentQueue<JObject> Queue { get; } = new ConcurrentQueue<JObject>();
    }

    // JsonParser -> DialogGenerator
    internal static class EventQueue
    {
        public static ConcurrentQueue<Event> Queue { get; } = new ConcurrentQueue<Event>();
    }

    // DialogGenerator -> Speaker
    internal static class DialogueQueue
    {
        public static ConcurrentQueue<Dialogue> Queue { get; } = new ConcurrentQueue<Dialogue>();
    }

}