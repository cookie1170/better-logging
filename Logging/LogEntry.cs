using Cookie.BetterLogging.TreeGeneration;

namespace Cookie.BetterLogging
{
    public readonly struct LogEntry
    {
        public readonly Node Content;
        public readonly LogInfo Info;

        public LogEntry(Node content, LogInfo info)
        {
            Content = content;
            Info = info;
        }
    }
}
