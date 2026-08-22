import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  AdminLessonListItemDto,
  LessonsOverviewClient,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-admin-lessons',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './admin-lessons.html',
  styleUrl: './admin-lessons.css',
})
export class AdminLessonsComponent implements OnInit {
  private readonly api = inject(LessonsOverviewClient);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly lessons = signal<AdminLessonListItemDto[]>([]);
  readonly selectedLessonId = signal<number | null>(null);

  readonly selectedLesson = computed(() => {
    const id = this.selectedLessonId();
    if (id == null) return null;
    return this.lessons().find((x) => x.id === id) ?? null;
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAllLessons().subscribe({
      next: (items) => {
        this.lessons.set(items ?? []);
        this.loading.set(false);
        const selected = this.selectedLessonId();
        if (selected && !(items ?? []).some((x) => x.id === selected)) {
          this.selectedLessonId.set(null);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.detail || err?.title || 'Failed to load lessons.');
      },
    });
  }

  selectLesson(id: number): void {
    this.selectedLessonId.set(this.selectedLessonId() === id ? null : id);
  }
}
