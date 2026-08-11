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

Swagger
Документация API доступна после запуска:

https://localhost:7065/swagger/index.html

http://localhost:5171/swagger/index.html

Тесты
bash
cd WebApiEvent.Tests
dotnet test
Тесты используют InMemory-провайдер EF Core и не требуют запущенной базы данных.

Новые эндпоинты (спринт 3)
POST /events/{id}/book – создать бронь на мероприятие.
Возвращает 202 Accepted + заголовок Location: /bookings/{bookingId} и тело брони.

GET /bookings/{id} – получить статус брони.
Возвращает 200 OK с информацией о брони.

Модель Booking
Id (Guid)

EventId (Guid)

Status (BookingStatus: Pending, Confirmed, Rejected)

CreatedAt (DateTime)

ProcessedAt (DateTime?, заполняется после обработки)

Фоновая обработка
BookingProcessingService (BackgroundService) запускается каждые 5 секунд.

Находит брони со статусом Pending, имитирует внешний вызов (задержка 2 сек), затем переводит в статус Confirmed и заполняет ProcessedAt.

Хранение данных 
Данные хранятся в PostgreSQL через Entity Framework Core.

Маппинг сущностей настроен через Fluent API (IEntityTypeConfiguration<T>).

Схема БД создаётся автоматически при запуске через EnsureCreated().

Сервисы работают с AppDbContext напрямую и зарегистрированы как Scoped.

Фоновый сервис использует IServiceScopeFactory для работы со Scoped-зависимостями.

Пример сценария
Создать событие: POST /events → получаем id.

Создать бронь: POST /events/{id}/book → получаем 202 Accepted, Location: /bookings/{bookingId}, статус Pending.

Сразу проверить: GET /bookings/{bookingId} → статус Pending.

Подождать 5–7 секунд, повторить GET → статус Confirmed, ProcessedAt заполнено.
