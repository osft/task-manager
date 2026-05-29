import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TaskService } from '../../../core/services/task.service';
import { TASK_PRIORITIES, TaskItem, TaskPriority, TaskStatus } from '../../../core/models/task.model';
import {
  dueDateHint,
  fromDatetimeLocalValue,
  maxDueDateForPriority,
  toApiDateTime,
  toDatetimeLocalValue
} from '../../../core/utils/task-rules';
import { DatetimePickerComponent } from '../../../shared/components/datetime-picker/datetime-picker.component';

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, DatetimePickerComponent],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss'
})
export class TaskFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly tasksApi = inject(TaskService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly priorities = TASK_PRIORITIES;
  readonly statuses = signal<TaskStatus[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly taskId = signal<string | null>(null);
  readonly createdOnUtc = signal<Date | null>(null);

  readonly isEdit = computed(() => this.taskId() !== null);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: [''],
    priority: ['Standard' as TaskPriority, Validators.required],
    dueDateLocal: ['', Validators.required],
    statusId: [1, Validators.required]
  });

  readonly priority = toSignal(this.form.controls.priority.valueChanges, {
    initialValue: 'Standard' as TaskPriority
  });

  readonly hint = computed(() => dueDateHint(this.priority()));

  readonly maxDueLocal = computed(() => {
    const anchor = this.createdOnUtc() ?? new Date();
    return toDatetimeLocalValue(maxDueDateForPriority(this.priority(), anchor));
  });

  readonly minDueLocal = computed(() => {
    const anchor = this.createdOnUtc() ?? new Date();
    const min = new Date(anchor.getTime() + 60_000);
    return toDatetimeLocalValue(min);
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.taskId.set(id);

    this.loading.set(true);

    this.tasksApi.listStatuses().subscribe({
      next: (statuses) => {
        this.statuses.set([...statuses].sort((a, b) => a.sortOrder - b.sortOrder));

        if (id) {
          this.loadTask(id);
        } else {
          this.setDefaultDueDate('Standard');
          this.loading.set(false);
        }
      },
      error: (message: string) => {
        this.error.set(message);
        this.loading.set(false);
      }
    });

    this.form.controls.priority.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((priority) => {
        this.clampDueDate(priority as TaskPriority);
      });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const raw = this.form.getRawValue();
    const dueDate = toApiDateTime(fromDatetimeLocalValue(raw.dueDateLocal));

    const payload = {
      title: raw.title.trim(),
      description: raw.description?.trim() || null,
      priority: raw.priority,
      dueDate
    };

    const id = this.taskId();
    const statusId = Number(raw.statusId);

    const request$ = id
      ? this.tasksApi.update(id, { ...payload, statusId })
      : this.tasksApi.create(payload);

    request$.subscribe({
      next: () => void this.router.navigate(['/tasks']),
      error: (message: string) => {
        this.error.set(message);
        this.saving.set(false);
      },
      complete: () => this.saving.set(false)
    });
  }

  private loadTask(id: string): void {
    this.tasksApi.getById(id).subscribe({
      next: (task) => this.patchFromTask(task),
      error: (message: string) => {
        this.error.set(message);
        this.loading.set(false);
      }
    });
  }

  private patchFromTask(task: TaskItem): void {
    this.createdOnUtc.set(new Date(task.createdOnUtc));

    this.form.patchValue({
      title: task.title,
      description: task.description ?? '',
      priority: task.priority,
      dueDateLocal: toDatetimeLocalValue(new Date(task.dueDate)),
      statusId: task.statusId
    });

    this.loading.set(false);
  }

  private setDefaultDueDate(priority: TaskPriority): void {
    const anchor = new Date();
    const offsetHours = priority === 'High' ? 24 : 168;
    const due = new Date(anchor.getTime() + offsetHours * 60 * 60 * 1000);
    const max = maxDueDateForPriority(priority, anchor);
    const value = due.getTime() > max.getTime() ? max : due;
    this.form.patchValue({ dueDateLocal: toDatetimeLocalValue(value) });
  }

  private clampDueDate(priority: TaskPriority): void {
    const current = this.form.controls.dueDateLocal.value;
    if (!current) {
      this.setDefaultDueDate(priority);
      return;
    }

    const anchor = this.createdOnUtc() ?? new Date();
    const selected = fromDatetimeLocalValue(current);
    const max = maxDueDateForPriority(priority, anchor);
    const min = new Date(anchor.getTime() + 60_000);

    if (selected > max) {
      this.form.patchValue({ dueDateLocal: toDatetimeLocalValue(max) });
    } else if (selected < min) {
      this.form.patchValue({ dueDateLocal: toDatetimeLocalValue(min) });
    }
  }
}
