import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ClassroomClient,
  StudentClassroomSessionListItemDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-student-classroom-list',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink],
  templateUrl: './student-classroom-list.html',
  styleUrls: ['../../classroom/classroom-theme.css', './student-classroom-list.css'],
})
export class StudentClassroomListComponent implements OnInit {
  private readonly classroomApi = inject(ClassroomClient);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly items = signal<StudentClassroomSessionListItemDto[]>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.classroomApi.getMyClassrooms().subscribe({
      next: (data) => {
        this.items.set(data ?? []);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load classrooms.');
      },
    });
  }

  toTimeInput(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  statusKey(item: StudentClassroomSessionListItemDto): string {
    if (item.hasEnded) return 'lessons.sessionEndedStatus';
    if (item.startedAtUtc) return 'lessons.sessionRunning';
    return 'lessons.sessionPending';
  }
}
