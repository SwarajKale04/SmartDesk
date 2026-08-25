import axios from 'axios'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

export type Role = 'Customer' | 'Agent' | 'Admin'
export type Ticket = { id: string; ticketNumber: string; title: string; description: string; priority: string | number; status: string | number; customerId: string; assignedAgentId?: string; createdAt: string; updatedAt: string; aiPredictedCategory?: string; aiPredictedPriority?: string | number; aiConfidence?: number; aiReviewRequired: boolean; aiClassificationStatus?: string; comments?: Comment[]; history?: History[] }
export type Comment = { id: string; userId: string; content: string; isInternal: boolean; createdAt: string }
export type History = { id: string; userId?: string; action: string; oldValue?: string; newValue?: string; timestamp: string }
export type Notification = { id: string; type: string; message: string; relatedTicketId?: string; isRead: boolean; createdAt: string }
export type Session = { userId: string; name: string; email: string; role: Role; accessToken: string; expiresAt: string }

export const api = axios.create({ baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5160/api' })
export const hubUrl = (import.meta.env.VITE_API_URL ?? 'http://localhost:5160/api').replace(/\/api$/, '') + '/hubs/notifications'
export const setToken = (token?: string) => { api.defaults.headers.common.Authorization = token ? `Bearer ${token}` : undefined }
export const connectNotifications = (token: string, onNotification: (notification: Notification) => void) => {
  const connection = new HubConnectionBuilder().withUrl(hubUrl, { accessTokenFactory: () => token }).withAutomaticReconnect().configureLogging(LogLevel.Warning).build()
  connection.on('NotificationReceived', onNotification)
  void connection.start().catch(() => undefined)
  return connection
}
