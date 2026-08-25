# Real-time notifications

SmartDesk persists every notification before attempting real-time delivery. This gives users an inbox through `GET /api/notifications` even if their browser was offline when the event occurred.

The authenticated SignalR hub is available at `/hubs/notifications`. A client connects using the access token returned from login; for browser WebSocket/SSE negotiation, SignalR passes this through the `access_token` query parameter. The API accepts that token only for the notifications hub path.

Each connection is added to `user:{userId}`. The server emits `NotificationReceived` with:

```ts
type Notification = {
  id: string; type: string; message: string;
  relatedTicketId?: string; isRead: boolean; createdAt: string;
};
```

Events currently include ticket creation, assignment, status changes, comments, and SLA risk/breach events. A failed real-time delivery is logged and does not undo the persisted notification.

```ts
const connection = new HubConnectionBuilder()
  .withUrl(`${apiUrl}/hubs/notifications`, { accessTokenFactory: () => accessToken })
  .withAutomaticReconnect()
  .build();
connection.on("NotificationReceived", notification => addToInbox(notification));
await connection.start();
```
