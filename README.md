REST API для управления мероприятиями на ASP.NET Core Web API.

## Запуск

git clone <URL>
cd WebApiEvent
dotnet run

## Swagger

Документация: https://localhost:7065/swagger/index.html

## Тесты

cd WebApiEvent.Tests
dotnet test

## Новые эндпоинты (спринт 3)

- `POST /events/{id}/book` – создать бронь на мероприятие.  
  Возвращает `202 Accepted` + заголовок `Location: /bookings/{bookingId}` и тело брони.

- `GET /bookings/{id}` – получить статус брони.  
  Возвращает `200 OK` с информацией о брони.

## Модель Booking

- `Id` (Guid)
- `EventId` (Guid)
- `Status` (BookingStatus: `Pending`, `Confirmed`, `Rejected`)
- `CreatedAt` (DateTime)
- `ProcessedAt` (DateTime?, заполняется после обработки)

## Фоновая обработка

- `BookingProcessingService` (BackgroundService) запускается каждые 5 секунд.
- Находит брони со статусом `Pending`, имитирует внешний вызов (задержка 2 сек), затем переводит в статус `Confirmed` и заполняет `ProcessedAt`.

## Пример сценария

1. Создать событие: `POST /events` → получаем `id`.
2. Создать бронь: `POST /events/{id}/book` → получаем `202 Accepted`, `Location: /bookings/{bookingId}`, статус `Pending`.
3. Сразу проверить: `GET /bookings/{bookingId}` → статус `Pending`.
4. Подождать 5–7 секунд, повторить `GET` → статус `Confirmed`, `ProcessedAt` заполнено.
