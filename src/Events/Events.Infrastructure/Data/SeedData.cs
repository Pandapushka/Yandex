using Events.Domain.Entities;

namespace Events.Infrastructure.Data
{
    public static class SeedData
    {
        public static List<Event> GetEvents()
        {
            return new List<Event>
            {
                Event.Create(
                    "Конференция разработчиков",
                    "Ежегодная конференция по ASP.NET Core",
                    DateTime.SpecifyKind(new DateTime(2026, 6, 1, 9, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 6, 1, 18, 0, 0), DateTimeKind.Utc),
                    100
                ),
                Event.Create(
                    "Митап по C#",
                    "Встреча разработчиков для обсуждения лучших практик",
                    DateTime.SpecifyKind(new DateTime(2026, 6, 15, 18, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 6, 15, 21, 0, 0), DateTimeKind.Utc),
                    50
                ),
                Event.Create(
                    "Воркшоп по Entity Framework",
                    "Практическое занятие по работе с EF Core",
                    DateTime.SpecifyKind(new DateTime(2026, 7, 5, 10, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 7, 5, 17, 0, 0), DateTimeKind.Utc),
                    30
                ),
                Event.Create(
                    "Хакатон: Инновации 2026",
                    "Командная разработка прототипов за 48 часов",
                    DateTime.SpecifyKind(new DateTime(2026, 7, 20, 9, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 7, 22, 18, 0, 0), DateTimeKind.Utc),
                    150
                ),
                Event.Create(
                    "Круглый стол по архитектуре",
                    "Обсуждение микросервисов и монолитов",
                    DateTime.SpecifyKind(new DateTime(2026, 8, 10, 14, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 8, 10, 17, 0, 0), DateTimeKind.Utc),
                    20
                ),
                Event.Create(
                    "Вебинар: Blazor в действии",
                    "Онлайн-встреча по созданию SPA на C#",
                    DateTime.SpecifyKind(new DateTime(2026, 8, 25, 15, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 8, 25, 16, 30, 0), DateTimeKind.Utc),
                    200
                ),
                Event.Create(
                    "Конференция по облачным технологиям",
                    "Azure, AWS, Google Cloud для .NET разработчиков",
                    DateTime.SpecifyKind(new DateTime(2026, 9, 10, 10, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 9, 10, 19, 0, 0), DateTimeKind.Utc),
                    100
                ),
                Event.Create(
                    "Maui-митап",
                    "Кроссплатформенная разработка с .NET MAUI",
                    DateTime.SpecifyKind(new DateTime(2026, 9, 22, 18, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 9, 22, 21, 0, 0), DateTimeKind.Utc),
                    40
                ),
                Event.Create(
                    "Семинар по безопасности",
                    "Защита веб-приложений и API",
                    DateTime.SpecifyKind(new DateTime(2026, 10, 5, 11, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 10, 5, 15, 0, 0), DateTimeKind.Utc),
                    60
                ),
                Event.Create(
                    "Годовая конференция DotNetFest",
                    "Главное событие года для .NET сообщества",
                    DateTime.SpecifyKind(new DateTime(2026, 11, 15, 9, 0, 0), DateTimeKind.Utc),
                    DateTime.SpecifyKind(new DateTime(2026, 11, 17, 18, 0, 0), DateTimeKind.Utc),
                    500
                )
            };
        }
    }
}
