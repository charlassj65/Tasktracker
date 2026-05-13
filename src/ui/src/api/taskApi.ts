import axios from 'axios';
import type { Task, CreateTaskRequest, UpdateTaskRequest, TaskSummary } from '../types/task';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  headers: { 'Content-Type': 'application/json' },
});

export const taskApi = {
  getAll: (): Promise<Task[]> =>
    api.get<Task[]>('/tasks').then(r => r.data),

  getById: (id: number): Promise<Task> =>
    api.get<Task>(`/tasks/${id}`).then(r => r.data),

  create: (request: CreateTaskRequest): Promise<Task> =>
    api.post<Task>('/tasks', request).then(r => r.data),

  update: (id: number, request: UpdateTaskRequest): Promise<void> =>
    api.put(`/tasks/${id}`, request).then(() => undefined),

  delete: (id: number): Promise<void> =>
    api.delete(`/tasks/${id}`).then(() => undefined),

  getTodaySummary: (): Promise<TaskSummary> =>
    api.get<TaskSummary>('/tasks/summary/today').then(r => r.data),
};
