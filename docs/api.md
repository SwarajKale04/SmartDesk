# API: Phase 3

All ticket endpoints require `Authorization: Bearer <access-token>`.

| Endpoint | Roles | Purpose |
| --- | --- | --- |
| `GET /api/tickets` | All | List tickets within the caller's access scope. |
| `GET /api/tickets/{id}` | All | Read ticket details, timeline, and visible comments. |
| `POST /api/tickets` | Customer | Create a new ticket. |
| `PUT /api/tickets/{id}` | Customer, Agent, Admin | Update details; customers can edit only a new ticket and agents only an assigned ticket. |
| `PUT /api/tickets/{id}/status` | Agent, Admin | Apply a valid lifecycle transition. |
| `PUT /api/tickets/{id}/assign` | Agent, Admin | Agent claims an unassigned ticket; Admin assigns any active agent. |
| `POST /api/tickets/{id}/comments` | All | Add a comment; internal comments are agent/admin-only. |

Supported list query parameters: `page`, `pageSize` (maximum 100), `search`, `status`, `priority`, `assignedAgentId`, `sortBy` (`createdAt`, `updatedAt`, `priority`), and `descending`.
