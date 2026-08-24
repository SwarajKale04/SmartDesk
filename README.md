# SmartDesk

SmartDesk is an AI-augmented IT service desk built as a Clean Architecture modular monolith. AI will automate ticket triage while agents retain review and control of the ticket lifecycle.

## Current status

Phase 2 adds customer registration and login with BCrypt password hashing, short-lived JWT access tokens, and role claims for Customer, Agent, and Admin authorization.

## Run the API

Set secrets outside source control before starting the API:

```powershell
$env:ConnectionStrings__SmartDesk = "Server=localhost,1433;Database=SmartDesk;User Id=sa;Password=<your-password>;TrustServerCertificate=True"
$env:Jwt__SigningKey = "<at-least-32-random-characters>"
$env:Seed__DefaultPassword = "<development-only-password>"
dotnet run --project src/SmartDesk.API
```

In Development, Swagger is available at `/swagger`; liveness and readiness endpoints are `/health` and `/health/ready`.

When a database connection and `Seed__DefaultPassword` are configured in Development, the API applies migrations and creates the development users `admin@smartdesk.local`, `agent@smartdesk.local`, and `customer@smartdesk.local`. Registration is always Customer-only; agent/admin creation will be added to the admin workflow.

## Ticket API

Authenticated customers can create tickets and see only their own tickets. Agents can view their assigned work and the unassigned queue, claim an unassigned ticket, manage status, and add internal comments. Admins can assign agents and access all tickets. The API provides pagination, text search, status/priority/assignee filters, and created/updated/priority sorting.

Use Swagger's **Authorize** button with the JWT returned by `/api/auth/login`, then use `/api/tickets` to work with the ticket lifecycle.
