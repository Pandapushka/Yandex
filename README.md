# Event Platform

Система управления мероприятиями, разделённая на три микросервиса с асинхронным обменом через Apache Kafka.

## Состав системы

| Сервис | Зона ответственности | База данных | Порт (локально) |
| --- | --- | --- | --- |
| Users (Users/Auth) | регистрация, вход, выдача JWT | `users` | 5101 |
| Events | CRUD событий и учёт доступных мест | `events` | 5102 |
| Bookings | создание и отмена броней | `bookings` | 5103 |

Каждый сервис построен по чистой архитектуре (слои `Domain`, `Application`, `Infrastructure`, `Presentation`) и имеет собственную базу PostgreSQL.

Разделяемый проект `BookingContracts` содержит контракт события `BookingConfirmed` и имя топика — его подключают сервисы-издатель и подписчик.

## Поток данных BookingConfirmed

1. Пользователь создаёт бронь в сервисе **Bookings** (`POST /bookings`).
2. Фоновый обработчик подтверждает бронь: сначала сохраняет статус `Confirmed` в свою базу, затем публикует в Kafka событие `BookingConfirmed` в топик `booking-confirmed` (ключ сообщения — `EventId`, чтобы все брони одного события обрабатывались по порядку).
3. Сервис **Events** подписан на топик `booking-confirmed`. При получении события он уменьшает количество доступных мест у соответствующего события.

Сервисы не вызывают друг друга напрямую по HTTP — обмен идёт только через Kafka (итоговая согласованность).

## JWT

Токен выдаёт только сервис **Users** (`POST /auth/login`). Сервисы **Events** и **Bookings** проверяют этот же токен — во всех трёх сервисах общие значения секрета, издателя и аудитории (секция `Jwt` в конфигурации).

Права доступа:

- управление событиями (`POST`/`PUT`/`DELETE /events`) — только роль `Admin`;
- эндпоинты броней — требуют аутентификации, `userId` читается из claims.

Администратор по умолчанию (сидируется в сервисе Users): `admin` / `Admin123!`.

## Запуск в Docker

### Предварительные требования

- Docker с поддержкой Docker Compose;
- .NET 8 SDK — только если сервисы запускаются локально вне Docker.

### Запуск всей системы (Kafka + базы + сервисы)

```
docker compose up --build -d
```

Флаг `-d` запускает в фоне; без него логи выводятся в текущую консоль. Поднимаются контейнеры:

- инфраструктура: `zookeeper`, `kafka`, `users-db`, `events-db`, `bookings-db`;
- сервисы: `users-service`, `events-service`, `bookings-service`.

Проверить статус контейнеров:

```
docker compose ps
```

Swagger:

- Users: http://localhost:5101/swagger
- Events: http://localhost:5102/swagger
- Bookings: http://localhost:5103/swagger

Логи конкретного сервиса, например Events:

```
docker compose logs -f events-service
```

Остановить систему (контейнеры удаляются, данные баз сохраняются в volumes):

```
docker compose down
```

Остановить и полностью удалить данные баз:

```
docker compose down -v
```

### Только базы данных и Kafka (сервисы — локально через dotnet)

Запустить только инфраструктуру:

```
docker compose up -d zookeeper kafka users-db events-db bookings-db
```

После этого сервисы можно запускать локально:

```
dotnet run --project src/Users/Users.Presentation --launch-profile http
dotnet run --project src/Events/Events.Presentation --launch-profile http
dotnet run --project src/Bookings/Bookings.Presentation --launch-profile http
```

Порты локальной разработки: Users — 5101, Events — 5102, Bookings — 5103 (настраиваются в `appsettings.json` и `launchSettings.json` каждого сервиса).

### Порты

| Компонент | Порт (хост) |
| --- | --- |
| Kafka (внешний слушатель) | 9092 |
| База данных `users` | 5432 |
| База данных `events` | 5433 |
| База данных `bookings` | 5434 |
| Сервис Users | 5101 |
| Сервис Events | 5102 |
| Сервис Bookings | 5103 |

Базы доступны с хоста по `localhost:5432/5433/5434` (логин `postgres`, пароль `postgres`). Внутри сети Docker сервисы обращаются к базам по именам `users-db:5432`, `events-db:5432`, `bookings-db:5432`, а к Kafka — по адресу `kafka:29092`.

## Проверка сценария

1. В Swagger сервиса Users зарегистрируйте пользователя и получите токен (`POST /auth/login` как `admin` / `Admin123!`).
2. В Swagger сервиса Events создайте событие администратором и запомните количество мест.
3. В Swagger сервиса Bookings создайте бронь (`POST /bookings` с `eventId`) и дождитесь подтверждения.
4. Проверьте в сервисе Events: количество доступных мест уменьшилось — событие прошло через Kafka.

## Сборка и тесты

```
dotnet build Yandex.sln
dotnet test Yandex.sln
```
