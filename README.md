# Event API

REST API для управления мероприятиями на ASP.NET Core Web API с JWT-аутентификацией.

## Структура

Проект разделён на четыре слоя по принципам чистой архитектуры:

- `WebApiEvent.Domain` — сущности, перечисления, доменные исключения (без внешних зависимостей).
- `WebApiEvent.Application` — use cases, сервисы, интерфейсы портов, DTO (зависит только от Domain).
- `WebApiEvent.Infrastructure` — DbContext, конфигурации, репозитории, JWT, хэширование паролей (зависит от Application и Domain).
- `WebApiEvent.Presentation` — контроллеры, middleware, composition root (зависит от Application и Infrastructure).

Application не зависит от Infrastructure — только через интерфейсы портов.

## Требования

.NET 8 SDK, Docker Desktop.

## Запуск

```
git clone <URL>
cd Yandex
docker compose -f WebApiEvent.Presentation/docker-compose.yml up -d events-db
dotnet run --project WebApiEvent.Presentation --launch-profile http
```

Swagger: http://localhost:5171/swagger (https: https://localhost:7065/swagger)

БД применяется автоматически через `db.Database.Migrate()`, при первом запуске сидируются
демо-события и администратор по умолчанию.

## Аутентификация и роли

- `POST /auth/register` — публичная регистрация обычного пользователя (роль `User`). Возвращает `204`.
- `POST /auth/register-admin` — регистрация администратора (роль `Admin`). Требует JWT с ролью `Admin`.
- `POST /auth/login` — вход, возвращает `{ "token": "..." }`.

Администратор по умолчанию (сидируется из `appsettings.json`, секция `SeedAdmin`):

```
login:    admin
password: Admin123!
```

Настройки JWT — секция `Jwt` в `appsettings.json` (`Key`, `Issuer`, `Audience`, `LifetimeMinutes`).
В Swagger токен вводится через кнопку **Authorize**: `Bearer <токен>`.

## Роли и доступ

| Эндпоинт | Доступ |
| --- | --- |
| `GET /Events`, `GET /Events/{id}` | публичный |
| `POST /Events`, `PUT /Events/{id}`, `DELETE /Events/{id}`, `PATCH /Events/{id}/soft-delete` | `Admin` |
| `POST /Events/{id}/book` | авторизованный (`User`/`Admin`) |
| `GET /bookings/{id}` | авторизованный |
| `DELETE /bookings/{id}` | владелец брони или `Admin` |

## Доменные правила бронирования

- Нельзя забронировать событие, которое уже началось → `400`.
- Максимум 10 активных броней (Pending + Confirmed) на пользователя → `409`.
- Отменять бронь может только её владелец; администратор может отменить любую → иначе `403`.
- Фоновая обработка броней: `Pending → Confirmed` (~5 сек).

## Миграции

Схема БД управляется миграциями EF Core. При запуске применяются автоматически через `Migrate()`.
Создание новой миграции:

```
dotnet ef migrations add <Name> --project WebApiEvent.Infrastructure --startup-project WebApiEvent.Presentation
```

## Тесты

Модульные (InMemory): `dotnet test Yandex.sln`.
Покрывают доменные правила (лимит броней, запрет прошедших событий, права на отмену),
сценарии регистрации/входа, хэширование паролей и уникальный индекс логина.

## Данные

PostgreSQL, маппинг через Fluent API, репозитории для доступа к данным.
