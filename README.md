# SmartDesk

SmartDesk is an AI-augmented IT service desk built as a Clean Architecture modular monolith. AI will automate ticket triage while agents retain review and control of the ticket lifecycle.

## Current status

Phase 1 is complete: the .NET 8 solution, Clean Architecture projects, domain model, EF Core SQL Server persistence boundary, API bootstrap, structured logging, OpenAPI, CORS, centralized error handling, and health endpoints are in place.

## Run the API

Set a SQL Server connection string outside source control before applying migrations in a later phase:

```powershell
$env:ConnectionStrings__SmartDesk = "Server=localhost,1433;Database=SmartDesk;User Id=sa;Password=<your-password>;TrustServerCertificate=True"
dotnet run --project src/SmartDesk.API
```

In Development, Swagger is available at `/swagger`; liveness and readiness endpoints are `/health` and `/health/ready`.
