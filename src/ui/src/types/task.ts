export type TaskStatus = 'Todo' | 'InProgress' | 'Done';

export interface Task {
  id: number;
  title: string;
  description?: string;
  status: TaskStatus;
  dueDate?: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  dueDate?: string;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  dueDate?: string;
}

export interface TaskSummary {
  date: string;
  totalTasks: number;
  summary: string;
}
