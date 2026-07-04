# TableTop — виртуальный стол для D&D

Реалтайм-платформа для настольных сессий: комнаты по кодам,
общий чат, броски кубиков, которые видят все игроки.

## Стек
ASP.NET Core (.NET 10) · SignalR · EF Core + PostgreSQL · React + TypeScript · Docker · xUnit

## Запуск
docker compose up -d
cd backend && dotnet ef database update --project TableTop.Infrastructure --startup-project TableTop.Api && dotnet run --project TableTop.Api
cd frontend && npm install && npm run dev
→ http://localhost:5173

## Архитектура
Core (домен, без зависимостей) ← Infrastructure (EF Core) ← Api (контроллеры, хабы)
Броски кубиков выполняются на сервере — подделать результат с клиента невозможно.