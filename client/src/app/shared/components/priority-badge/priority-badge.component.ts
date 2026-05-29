import { Component, Input } from '@angular/core';
import { TaskPriority } from '../../../core/models/task.model';

@Component({
  selector: 'app-priority-badge',
  standalone: true,
  template: `
    <span class="badge" [class]="priorityClass">{{ priority }}</span>
  `,
  styles: [
    `
      .badge {
        display: inline-flex;
        align-items: center;
        padding: 0.22rem 0.6rem;
        border-radius: 999px;
        font-size: 0.68rem;
        font-weight: 700;
        letter-spacing: 0.05em;
        text-transform: uppercase;
        border: 1px solid transparent;
      }

      .badge--high {
        color: #92400e;
        background: rgba(224, 149, 48, 0.18);
        border-color: rgba(224, 149, 48, 0.4);
      }

      .badge--standard {
        color: #1e5f56;
        background: rgba(78, 184, 168, 0.15);
        border-color: rgba(78, 184, 168, 0.35);
      }

      .badge--low {
        color: #64748b;
        background: #f1f5f9;
        border-color: #e2e8f0;
      }
    `
  ]
})
export class PriorityBadgeComponent {
  @Input({ required: true }) priority!: TaskPriority;

  get priorityClass(): string {
    switch (this.priority) {
      case 'High':
        return 'badge--high';
      case 'Standard':
        return 'badge--standard';
      default:
        return 'badge--low';
    }
  }
}
