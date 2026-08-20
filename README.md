# REST API для управления мероприятиями (ASP.NET Core Web API)

Сервис бронирования мероприятий, реорганизованный по принципам **чистой архитектуры** (Clean Architecture).
Проект разбит на четыре независимые сборки со строго направленными зависимостями «внутрь».

## Структура решения

```text
Yandex.sln
├── WebApiEvent.Domain            — слой предметной области
├── WebApiEvent.Application       — слой бизнес-логики (use cases)
├── WebApiEvent.Infrastructure    — слой доступа к данным и внешним системам
├── WebApiEvent.Presentation      — веб-проект (точка входа, HTTP-обвязка)
└── WebApiEvent.Tests             — модульные тесты
```

### Назначение слоёв

| Слой | Содержимое | Зависимости |
| --- | --- | --- |
| **Domain** | Сущности (`Event`, `Booking`, `BaseEntity`), перечисления (`BookingStatus`), доменные исключения | Не зависит ни от чего внешнего |
| **Application** | Use cases и сервисы (`EventService`, `BookingService`, `BookingProcessingService`), интерфейсы портов (`IEventRepository`, `IBookingRepository`), DTO | Только `Domain` |
| **Infrastructure** | `AppDbContext`, конфигурации маппинга (`EventConfiguration`, `BookingConfiguration`), реализации репозиториев, `SeedData` | `Application` и `Domain` |
| **Presentation** | Контроллеры, глобальный обработчик исключений, composition root (`Program.cs`), CORS | `Application` и `Infrastructure` |

Направление зависимостей:

```text
Presentation ──► Application ──► Domain
      │              ▲
      └──► Infrastructure ──┘
```

Ключевое правило: **Application не ссылается на Infrastructure** — бизнес-логика работает
только с интерфейсами портов, а конкретные реализации подставляются через DI.

## Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- PostgreSQL (запускается через Docker Compose)

## Запуск

### 1. Клонировать репозиторий

```bash
git clone <URL>
cd WebApiEvent.Presentation
```

### 2. Запустить инфраструктуру (PostgreSQL, Kafka, ZooKeeper)

```bash
docker compose up -d
```

### 3. Запустить приложение

```bash
dotnet run
```

При первом запуске база данных и таблицы создаются автоматически через `EnsureCreated()`,
а также заполняются начальными тестовыми данными.

## Конфигурация

Строка подключения к PostgreSQL находится в `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=events;Username=postgres;Password=postgres"
  }
}
```

При необходимости измените параметры подключения под своё окружение.

## Swagger

- https://localhost:7065/swagger/index.html
- http://localhost:5171/swagger/index.html

## Тесты

```bash
dotnet test
```

Тесты используют InMemory-провайдер EF Core и не требуют запущенной базы данных.
Тестовый проект ссылается на слои `Application` и `Infrastructure`, а не на веб-проект.

## Эндпоинты

- `GET /events` — список активных мероприятий с фильтрами (`title`, `from`, `to`) и пагинацией (`page`, `pageSize`).
- `GET /events/{id}` — мероприятие по id.
- `POST /events` — создать мероприятие.
- `PUT /events/{id}` — обновить мероприятие.
- `DELETE /events/{id}` — удалить мероприятие.
- `PATCH /events/{id}/soft-delete` — деактивировать мероприятие.
- `POST /events/{id}/book` — создать бронь на мероприятие.
  Возвращает `202 Accepted` + заголовок `Location: /bookings/{bookingId}` и тело брони.
- `GET /bookings/{id}` — получить статус брони.

## Модель Booking

- `Id` (Guid)
- `EventId` (Guid)
- `Status` (`BookingStatus`: `Pending`, `Confirmed`, `Rejected`)
- `CreatedAt` (DateTime)
- `ProcessedAt` (DateTime?, заполняется после обработки)

## Фоновая обработка

`BookingProcessingService` (`BackgroundService`) запускается каждые 5 секунд, находит брони
со статусом `Pending`, имитирует внешний вызов (задержка 2 секунды), затем переводит их
в статус `Confirmed` и заполняет `ProcessedAt`.

## Хранение данных

Данные хранятся в PostgreSQL через Entity Framework Core.
Маппинг сущностей настроен через Fluent API (`IEntityTypeConfiguration<T>`).
Схема БД создаётся автоматически при запуске через `EnsureCreated()`.

## Пример сценария

1. Создать событие: `POST /events` → получаем `id`.
2. Создать бронь: `POST /events/{id}/book` → `202 Accepted`, `Location: /bookings/{bookingId}`, статус `Pending`.
3. Сразу проверить: `GET /bookings/{bookingId}` → статус `Pending`.
4. Подождать 5–7 секунд, повторить `GET` → статус `Confirmed`, `ProcessedAt` заполнено.

## Миграции

Контекст БД (`AppDbContext`) находится в проекте `WebApiEvent.Infrastructure`.
Для создания новой миграции выполните из корня репозитория:

```bash
dotnet ef migrations add <MigrationName> \
    --project WebApiEvent.Infrastructure \
    --startup-project WebApiEvent.Presentation
```

Для применения миграций к базе:

```bash
dotnet ef database update \
    --project WebApiEvent.Infrastructure \
    --startup-project WebApiEvent.Presentation
```

> Примечание: в текущей конфигурации схема создаётся автоматически через `EnsureCreated()`,
> поэтому миграции не обязательны для локального запуска.
