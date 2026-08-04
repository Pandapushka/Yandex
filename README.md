REST API для управления мероприятиями на ASP.NET Core Web API.

## Запуск

git clone <URL>
cd WebApiEvent
2. Запустить инфраструктуру (PostgreSQL, Kafka, ZooKeeper)
bash
docker compose up -d
3. Запустить приложение
bash
dotnet run
При первом запуске база данных и таблицы создаются автоматически через EnsureCreated(), а также заполняются начальными тестовыми данными.

Конфигурация
Строка подключения к PostgreSQL находится в appsettings.json:

json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=events;Username=postgres;Password=postgres"
  }
}
При необходимости измените параметры подключения под своё окружение.

## Swagger

https://localhost:7065/swagger/index.html

http://localhost:5171/swagger/index.html

Тесты
bash
cd WebApiEvent.Tests
dotnet test

## Новые эндпоинты (спринт 3)

- `POST /events/{id}/book` – создать бронь на мероприятие.  
  Возвращает `202 Accepted` + заголовок `Location: /bookings/{bookingId}` и тело брони.

- `GET /bookings/{id}` – получить статус брони.  
  Возвращает `200 OK` с информацией о брони.

## Модель Booking

Фоновый сервис использует IServiceScopeFactory для работы со Scoped-зависимостями.

Пример сценария
Создать событие: POST /events → получаем id.

Создать бронь: POST /events/{id}/book → получаем 202 Accepted, Location: /bookings/{bookingId}, статус Pending.

Сразу проверить: GET /bookings/{bookingId} → статус Pending.

Подождать 5–7 секунд, повторить GET → статус Confirmed, ProcessedAt заполнено.
