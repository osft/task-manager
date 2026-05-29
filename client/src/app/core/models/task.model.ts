export type TaskPriority = 'High' | 'Standard' | 'Low';

export interface TaskItem {
  id: string;
  title: string;
  description: string;
  statusId: number;
  statusName?: string | null;
  priority: TaskPriority;
  dueDate: string;
  createdBy: string;
  createdOnUtc: string;
  updatedBy: string;
  updatedOnUtc: string;
}

export interface TaskStatus {
  id: number;
  name: string;
  description?: string | null;
  sortOrder: number;
}

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  priority: TaskPriority;
  dueDate: string;
}

export interface UpdateTaskRequest extends CreateTaskRequest {
  statusId: number;
}

export const TASK_PRIORITIES: TaskPriority[] = ['High', 'Standard', 'Low'];
