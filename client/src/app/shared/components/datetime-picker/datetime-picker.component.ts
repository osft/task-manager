import {
  Component,
  ElementRef,
  HostListener,
  forwardRef,
  input,
  signal,
  computed,
  inject
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { fromDatetimeLocalValue, toDatetimeLocalValue } from '../../../core/utils/task-rules';

const WEEKDAYS = ['Su', 'Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa'];

@Component({
  selector: 'app-datetime-picker',
  standalone: true,
  templateUrl: './datetime-picker.component.html',
  styleUrl: './datetime-picker.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DatetimePickerComponent),
      multi: true
    }
  ]
})
export class DatetimePickerComponent implements ControlValueAccessor {
  private readonly host = inject(ElementRef<HTMLElement>);

  readonly min = input('');
  readonly max = input('');
  readonly placeholder = input('Select date and time');

  readonly isOpen = signal(false);
  readonly viewMonth = signal(startOfMonth(new Date()));
  readonly selected = signal<Date | null>(null);
  readonly hour = signal(9);
  readonly minute = signal(0);
  readonly disabled = signal(false);
  readonly touched = signal(false);

  readonly weekdays = WEEKDAYS;

  readonly monthLabel = computed(() =>
    this.viewMonth().toLocaleDateString(undefined, { month: 'long', year: 'numeric' })
  );

  readonly calendarDays = computed(() => buildCalendarDays(this.viewMonth()));

  readonly displayValue = computed(() => {
    const d = this.selected();
    if (!d) {
      return '';
    }

    const withTime = new Date(d);
    withTime.setHours(this.hour(), this.minute(), 0, 0);
    return withTime.toLocaleString(undefined, {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: '2-digit'
    });
  });

  readonly hourOptions = Array.from({ length: 24 }, (_, i) => i);
  readonly minuteOptions = [0, 15, 30, 45];

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: string): void {
    if (!value) {
      this.selected.set(null);
      return;
    }

    const date = fromDatetimeLocalValue(value);
    this.selected.set(startOfDay(date));
    this.hour.set(date.getHours());
    this.minute.set(date.getMinutes());
    this.viewMonth.set(startOfMonth(date));
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  toggle(): void {
    if (this.disabled()) {
      return;
    }

    this.isOpen.update((v) => !v);
    if (this.isOpen()) {
      const base = this.selected() ?? clampToBounds(new Date(), this.min(), this.max());
      this.selected.set(startOfDay(base));
      this.hour.set(base.getHours());
      this.minute.set(base.getMinutes());
      this.viewMonth.set(startOfMonth(base));
    }
    this.markTouched();
  }

  close(): void {
    this.isOpen.set(false);
  }

  prevMonth(): void {
    const d = new Date(this.viewMonth());
    d.setMonth(d.getMonth() - 1);
    this.viewMonth.set(startOfMonth(d));
  }

  nextMonth(): void {
    const d = new Date(this.viewMonth());
    d.setMonth(d.getMonth() + 1);
    this.viewMonth.set(startOfMonth(d));
  }

  selectDay(day: CalendarDay): void {
    if (!day.inMonth || day.disabled) {
      return;
    }

    this.selected.set(day.date);
    this.emitValue();
  }

  onTimeChange(): void {
    if (this.selected()) {
      this.emitValue();
    }
  }

  isSelected(day: CalendarDay): boolean {
    const sel = this.selected();
    if (!sel || !day.inMonth) {
      return false;
    }

    return isSameDay(sel, day.date);
  }

  isToday(day: CalendarDay): boolean {
    return day.inMonth && isSameDay(day.date, new Date());
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.close();
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.close();
  }

  private emitValue(): void {
    const sel = this.selected();
    if (!sel) {
      this.onChange('');
      return;
    }

    const combined = new Date(sel);
    combined.setHours(this.hour(), this.minute(), 0, 0);

    const minDate = this.min() ? fromDatetimeLocalValue(this.min()) : null;
    const maxDate = this.max() ? fromDatetimeLocalValue(this.max()) : null;

    let clamped = combined;
    if (minDate && clamped < minDate) {
      clamped = minDate;
    }
    if (maxDate && clamped > maxDate) {
      clamped = maxDate;
    }

    this.hour.set(clamped.getHours());
    this.minute.set(clamped.getMinutes());
    this.onChange(toDatetimeLocalValue(clamped));
  }

  private markTouched(): void {
    if (!this.touched()) {
      this.touched.set(true);
      this.onTouched();
    }
  }

  isDayDisabled(day: CalendarDay): boolean {
    if (!day.inMonth) {
      return true;
    }

    const minDate = this.min() ? startOfDay(fromDatetimeLocalValue(this.min())) : null;
    const maxDate = this.max() ? startOfDay(fromDatetimeLocalValue(this.max())) : null;
    const dayStart = startOfDay(day.date);

    if (minDate && dayStart < minDate) {
      return true;
    }

    if (maxDate && dayStart > maxDate) {
      return true;
    }

    return false;
  }
}

interface CalendarDay {
  date: Date;
  inMonth: boolean;
  disabled: boolean;
  label: number;
}

function buildCalendarDays(viewMonth: Date): CalendarDay[] {
  const year = viewMonth.getFullYear();
  const month = viewMonth.getMonth();
  const first = new Date(year, month, 1);
  const startOffset = first.getDay();
  const days: CalendarDay[] = [];

  const gridStart = new Date(year, month, 1 - startOffset);

  for (let i = 0; i < 42; i++) {
    const date = new Date(gridStart);
    date.setDate(gridStart.getDate() + i);
    days.push({
      date,
      inMonth: date.getMonth() === month,
      disabled: false,
      label: date.getDate()
    });
  }

  return days;
}

function startOfMonth(d: Date): Date {
  return new Date(d.getFullYear(), d.getMonth(), 1);
}

function startOfDay(d: Date): Date {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate());
}

function isSameDay(a: Date, b: Date): boolean {
  return (
    a.getFullYear() === b.getFullYear() &&
    a.getMonth() === b.getMonth() &&
    a.getDate() === b.getDate()
  );
}

function clampToBounds(value: Date, min: string, max: string): Date {
  let result = value;
  if (min) {
    const minDate = fromDatetimeLocalValue(min);
    if (result < minDate) {
      result = minDate;
    }
  }
  if (max) {
    const maxDate = fromDatetimeLocalValue(max);
    if (result > maxDate) {
      result = maxDate;
    }
  }
  return result;
}
