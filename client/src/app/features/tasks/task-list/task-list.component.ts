import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';
import { TaskItem } from '../../../core/models/task.model';
import { PriorityBadgeComponent } from '../../../shared/components/priority-badge/priority-badge.component';
import { formatDueDate, isDueSoon, isOverdue } from '../../../core/utils/task-rules';

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [RouterLink, PriorityBadgeComponent],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss'
})
export class TaskListComponent implements OnInit {
  private readonly tasksApi = inject(TaskService);

  readonly tasks = signal<TaskItem[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly deletingId = signal<string | null>(null);

  readonly formatDueDate = formatDueDate;
  readonly isDueSoon = isDueSoon;
  readonly isOverdue = isOverdue;

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tasksApi.list().subscribe({
      next: (items) => {
        const sorted = [...items].sort(
          (a, b) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime()
        );
        this.tasks.set(sorted);
        this.loading.set(false);
      },
      error: (message: string) => {
        this.error.set(message);
        this.loading.set(false);
      }
    });
  }

  remove(task: TaskItem): void {
    if (!confirm(`Delete "${task.title}"?`)) {
      return;
    }

    this.deletingId.set(task.id);
    this.tasksApi.delete(task.id).subscribe({
      next: () => {
        this.tasks.update((list) => list.filter((t) => t.id !== task.id));
        this.deletingId.set(null);
      },
      error: (message: string) => {
        this.error.set(message);
        this.deletingId.set(null);
      }
    });
  }

  dueClass(task: TaskItem): string {
    if (isOverdue(task.dueDate)) {
      return 'due--overdue';
    }

    if (task.priority === 'High' || isDueSoon(task.dueDate)) {
      return 'due--soon';
    }

    return '';
  }
}
