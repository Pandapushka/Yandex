namespace Events.Application.Options
{
    public class CacheOptions
    {
        public int EventTtlMinutes { get; set; } = 5;

        public int TopEventsTtlMinutes { get; set; } = 10;
    }
}
