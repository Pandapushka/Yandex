# Event API

REST API для управления мероприятиями на ASP.NET Core Web API.

## Структура

Проект разделён на четыре слоя по принципам чистой архитектуры:

- WebApiEvent.Domain — сущности, перечисления, доменные исключения (без внешних зависимостей).
- WebApiEvent.Application — use cases, сервисы, интерфейсы портов, DTO (зависит только от Domain).
- WebApiEvent.Infrastructure — DbContext, конфигурации, репозитории (зависит от Application и Domain).
- WebApiEvent.Presentation — контроллеры, middleware, composition root (зависит от Application и Infrastructure).

Application не зависит от Infrastructure — только через интерфейсы портов.

## Требования

.NET 8 SDK, Docker Desktop.

## Запуск

git clone <URL>
cd WebApiEvent.Presentation
docker compose up -d
dotnet run

Swagger: https://localhost:7065/swagger

## Миграции

Схема БД управляется миграциями EF Core. При запуске применяются автоматически через Migrate(). Создание новой миграции: dotnet ef migrations add <Name> --project WebApiEvent.Infrastructure --startup-project WebApiEvent.Presentation.

## Тесты

Модульные (InMemory): cd WebApiEvent.Tests && dotnet test.

## Эндпоинты

GET /Events — список событий с пагинацией и фильтрами (title, from, to, page, pageSize). POST /Events — создать событие. GET /Events/{id} — получить событие. PUT /Events/{id} — обновить событие. DELETE /Events/{id} — удалить событие. PATCH /Events/{id}/soft-delete — деактивировать событие. POST /Events/{id}/book — создать бронь. GET /Bookings/{id} — статус брони.

## Данные

PostgreSQL, маппинг через Fluent API, репозитории для доступа к данным, фоновая обработка броней (Pending → Confirmed, ~5 сек).
