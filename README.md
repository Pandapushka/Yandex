# REST API для управления мероприятиями на ASP.NET Core Web API.

## Запуск
```bash
git clone <repo-url>
cd WebApiEvent
dotnet run
Swagger
Документация: https://localhost:7065/swagger/index.html

Тесты
bash
cd WebApiEvent.Tests
dotnet test
Модели
Event (спринт 4)
Добавлены поля:

TotalSeats (int) – общее количество мест на мероприятии

AvailableSeats (int) – текущее количество свободных мест (при создании равно TotalSeats)

Booking
Id (Guid)

EventId (Guid)

Status (BookingStatus: Pending, Confirmed, Rejected)

CreatedAt (DateTime)

ProcessedAt (DateTime?, заполняется после обработки)

Эндпоинты
Новые эндпоинты (спринт 3)
POST /events/{id}/book – создать бронь на мероприятие.
Возвращает 202 Accepted + заголовок Location: /bookings/{bookingId} и тело брони.
Дополнение спринта 4: при отсутствии свободных мест возвращает 409 Conflict с сообщением "Нет свободных мест".

GET /bookings/{id} – получить статус брони.
Возвращает 200 OK с информацией о брони.

Создание события (спринт 4)
При создании события (POST /events) обязательно передавать totalSeats (целое число > 0).
Пример тела запроса:

json
{
  "title": "Конференция",
  "description": "...",
  "startAt": "2026-06-01T09:00:00",
  "endAt": "2026-06-01T18:00:00",
  "totalSeats": 100
}
Фоновая обработка
Спринт 3
BookingProcessingService (BackgroundService) запускается каждые 5 секунд.
Находит брони со статусом Pending, имитирует внешний вызов (задержка 2 сек), затем переводит в статус Confirmed и заполняет ProcessedAt.

Спринт 4 (параллельная обработка)
Обработка броней выполняется параллельно с помощью Task.WhenAll.

Для защиты записи в хранилище используется SemaphoreSlim(1,1) (асинхронный аналог lock).

Если событие было удалено или деактивировано к моменту обработки, бронь переводится в статус Rejected, а место возвращается через ReleaseSeats().

При любой ошибке бронь отклоняется, место освобождается.

Защита от овербукинга (спринт 4)
В BookingService.CreateBookingAsync критическая секция (проверка мест + создание брони) обёрнута в lock.
Используется метод Event.TryReserveSeats() – атомарно проверяет и уменьшает количество доступных мест.
Если места нет, выбрасывается NoAvailableSeatsException → middleware возвращает 409 Conflict.

Пример сценария (спринт 4 – овербукинг)
Создать событие с totalSeats: 3:
POST /events → получаем id.

Отправить 4 параллельных запроса на бронирование:
POST /events/{id}/book.

Результат:

3 запроса получат 202 Accepted, брони со статусом Pending.

1 запрос получит 409 Conflict (нет мест).

Через 5–7 секунд все успешные брони станут Confirmed, AvailableSeats события станет 0.

Тесты на конкурентность (спринт 4)
Добавлены юнит-тесты:

Успешное бронирование уменьшает AvailableSeats.

При недостатке мест – NoAvailableSeatsException.

20 конкурентных запросов на 5 мест: ровно 5 успешных, 15 ошибок.

Уникальность ID броней при одновременных запросах.

Переходы статусов Confirm / Reject и освобождение мест.

Запуск тестов: dotnet test
