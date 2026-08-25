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

## SLA automation

Development data seeds active SLA policies: Critical (15 min response / 4 hr resolution), High (30 min / 8 hr), Medium (2 hr / 24 hr), and Low (8 hr / 72 hr). New tickets receive deadlines from their priority policy. A configurable hosted worker checks active tickets every 10 minutes, flags them at risk within 60 minutes of the earliest outstanding deadline, and records breach notifications for assigned agents and administrators. Configure the interval and warning threshold under `SlaMonitoring`.

## AI classification

New tickets are classified through an ML.NET service into category and priority, with a stored confidence score. Predictions are auto-applied at 60% confidence or higher and marked for human review below 80%. Classification is resilient: a model failure records an audit event but does not prevent ticket creation. See [AI classification documentation](docs/ai-classification.md).

## Real-time notifications

SignalR sends persisted ticket and SLA notifications to each authenticated user's private connection group. The notification inbox remains available through `GET /api/notifications` when a user is offline. See [the real-time client contract](docs/realtime-notifications.md).

## Frontend

The React/Vite client lives in `frontend/smartdesk-web`. It provides customer, agent, and admin-aware dashboards, ticket lists and detail conversations, AI transparency, and a live notification inbox.

```powershell
cd frontend/smartdesk-web
Copy-Item .env.example .env
npm install
npm run dev
```

Set `VITE_API_URL` when the API is not running at `http://localhost:5160/api`.
