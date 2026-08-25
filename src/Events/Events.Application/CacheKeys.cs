namespace Events.Application
{
    public static class CacheKeys
    {
        public const string Top10 = "events:top10";

        public static string Event(Guid id) => $"event:{id}";
    }
}
