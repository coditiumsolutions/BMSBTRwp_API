# BMSBTRwp API

.NET 8 Web API that serves paginated SQL Server data for React Native / SQLite sync.

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/data/tables` | Lists all tables from `db.txt` |
| `GET` | `/api/data/{tableName}?page=1&pageSize=1000` | Fetches paginated rows |

## Setup

1. Update the connection string in `appsettings.json`
2. Ensure `db.txt` contains your table definitions
3. Run: `dotnet run`
4. Swagger UI: `https://localhost:7093/swagger`
