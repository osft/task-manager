import { TaskPriority } from '../models/task.model';

const HOURS_48_MS = 48 * 60 * 60 * 1000;
const YEAR_MS = 365 * 24 * 60 * 60 * 1000;

export function maxDueDateForPriority(priority: TaskPriority, anchor: Date = new Date()): Date {
  if (priority === 'High') {
    return new Date(anchor.getTime() + HOURS_48_MS);
  }

  return new Date(anchor.getTime() + YEAR_MS);
}

export function dueDateHint(priority: TaskPriority): string {
  switch (priority) {
    case 'High':
      return 'High-priority tasks must be due within 48 hours of creation.';
    case 'Standard':
    case 'Low':
      return 'Standard and Low priority tasks can be due up to one year from creation.';
    default:
      return '';
  }
}

export function toDatetimeLocalValue(date: Date): string {
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function fromDatetimeLocalValue(value: string): Date {
  return new Date(value);
}

export function toApiDateTime(date: Date): string {
  return date.toISOString();
}

export function formatDueDate(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}

export function isDueSoon(iso: string, withinHours = 24): boolean {
  const due = new Date(iso).getTime();
  const now = Date.now();
  return due > now && due - now <= withinHours * 60 * 60 * 1000;
}

export function isOverdue(iso: string): boolean {
  return new Date(iso).getTime() < Date.now();
}
