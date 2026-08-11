# Event API

REST API для управления мероприятиями на ASP.NET Core Web API.

## Требования

.NET 8 SDK, Docker Desktop.

## Запуск

git clone <URL>
cd WebApiEvent
docker compose up -d
dotnet run

Swagger: https://localhost:7065/swagger

## Миграции

Схема БД управляется миграциями EF Core. При запуске применяются автоматически через Migrate(). Создание новой миграции: dotnet ef migrations add <Name>.

## Тесты

Модульные (InMemory): cd WebApiEvent.Tests && dotnet test. Интеграционные (Testcontainers PostgreSQL, требуется Docker): cd EventApi.IntegrationTests && dotnet test.

## Эндпоинты

GET /Events — список событий с пагинацией и фильтрами (title, from, to, page, pageSize). POST /Events — создать событие. GET /Events/{id} — получить событие. PUT /Events/{id} — обновить событие. DELETE /Events/{id} — удалить событие. PATCH /Events/{id}/soft-delete — деактивировать событие. POST /Events/{id}/book — создать бронь. GET /Bookings/{id} — статус брони.

## Данные

PostgreSQL, маппинг через Fluent API, репозитории для доступа к данным, фоновая обработка броней (Pending → Confirmed, ~5 сек).
