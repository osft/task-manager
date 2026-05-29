import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, shareReplay, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateTaskRequest,
  TaskItem,
  TaskStatus,
  UpdateTaskRequest
} from '../models/task.model';
import { ApiErrorBody, readApiError } from '../models/api-error.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  private statusesCache$?: Observable<TaskStatus[]>;

  list(): Observable<TaskItem[]> {
    return this.http
      .get<TaskItem[]>(`${this.baseUrl}/api/tasks`)
      .pipe(catchError((err) => throwError(() => this.extractError(err))));
  }

  getById(id: string): Observable<TaskItem> {
    return this.http
      .get<TaskItem>(`${this.baseUrl}/api/tasks/${id}`)
      .pipe(catchError((err) => throwError(() => this.extractError(err))));
  }

  create(request: CreateTaskRequest): Observable<TaskItem> {
    return this.http
      .post<TaskItem>(`${this.baseUrl}/api/tasks`, request)
      .pipe(catchError((err) => throwError(() => this.extractError(err))));
  }

  update(id: string, request: UpdateTaskRequest): Observable<TaskItem> {
    return this.http
      .put<TaskItem>(`${this.baseUrl}/api/tasks/${id}`, request)
      .pipe(catchError((err) => throwError(() => this.extractError(err))));
  }

  delete(id: string): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/api/tasks/${id}`)
      .pipe(catchError((err) => throwError(() => this.extractError(err))));
  }

  listStatuses(): Observable<TaskStatus[]> {
    if (!this.statusesCache$) {
      this.statusesCache$ = this.http
        .get<TaskStatus[]>(`${this.baseUrl}/api/task-statuses`)
        .pipe(
          shareReplay(1),
          catchError((err) => throwError(() => this.extractError(err)))
        );
    }

    return this.statusesCache$;
  }

  private extractError(err: { error?: ApiErrorBody }): string {
    return readApiError(err?.error ?? err);
  }
}
