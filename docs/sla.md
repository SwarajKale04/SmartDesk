# SLA automation

`ISlaCalculationService` selects the active policy matching ticket priority and returns first-response and resolution deadlines. Ticket creation records the applied SLA in ticket history. A public agent reply records the first response. Resolution completes the SLA; reopening recalculates its deadlines from the active policy.

`SlaMonitoringService` is a hosted background service. It runs at the configured interval, checks the earliest unmet deadline for active tickets, updates `OnTrack`, `AtRisk`, or `Breached` state, creates audit history, and persists notifications. SignalR delivery of these notifications is introduced in Phase 6.
