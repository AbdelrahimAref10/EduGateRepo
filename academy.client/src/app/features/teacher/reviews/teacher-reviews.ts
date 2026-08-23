import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ReviewInboxKind,
  ReviewStatDto,
  TeacherReviewInboxItemDto,
  TeacherReviewsClient,
  TeacherReviewSummaryDto,
} from '../../../core/api/academy-api.generated';
import { NotificationService } from '../../../core/notifications/notification.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { RatingStarsComponent } from '../../marketplace/rating-stars';

type InboxTab = 'all' | 'teacher' | 'lesson' | 'session';

@Component({
  selector: 'app-teacher-reviews',
  standalone: true,
  imports: [DatePipe, DecimalPipe, RouterLink, TranslatePipe, UserAvatarComponent, RatingStarsComponent],
  templateUrl: './teacher-reviews.html',
})
export class TeacherReviewsComponent implements OnInit, OnDestroy {
  private readonly reviewsApi = inject(TeacherReviewsClient);
  private readonly notifications = inject(NotificationService);
  private stopLive?: () => void;

  readonly loadingSummary = signal(false);
  readonly loadingList = signal(false);
  readonly loadingMore = signal(false);
  readonly error = signal<string | null>(null);
  readonly summary = signal<TeacherReviewSummaryDto | null>(null);
  readonly items = signal<TeacherReviewInboxItemDto[]>([]);
  readonly total = signal(0);
  readonly tab = signal<InboxTab>('all');

  private readonly pageSize = 20;

  ngOnInit(): void {
    this.stopLive = this.notifications.when(
      ['TeacherReviewReceived', 'LessonReviewReceived', 'SessionReviewReceived'],
      0,
      () => this.refresh(true),
    );
    this.loadSummary();
    this.loadList(true);
  }

  ngOnDestroy(): void {
    this.stopLive?.();
  }

  setTab(tab: InboxTab): void {
    if (this.tab() === tab) return;
    this.tab.set(tab);
    this.loadList(true);
  }

  refresh(silent = false): void {
    this.loadSummary(silent);
    this.loadList(true, silent);
  }

  loadMore(): void {
    if (this.loadingMore() || this.items().length >= this.total()) return;
    this.loadList(false);
  }

  stat(kind: InboxTab): ReviewStatDto | null {
    const summary = this.summary();
    if (!summary) return null;
    if (kind === 'teacher') return summary.teacher ?? null;
    if (kind === 'lesson') return summary.lessons ?? null;
    if (kind === 'session') return summary.sessions ?? null;
    return summary.all ?? null;
  }

  kindKey(kind?: number): string {
    if (kind === ReviewInboxKind.Lesson) return 'reviews.lesson';
    if (kind === ReviewInboxKind.Session) return 'reviews.session';
    return 'reviews.teacher';
  }

  context(item: TeacherReviewInboxItemDto): string {
    if (item.kind === ReviewInboxKind.Lesson) return item.subject || '';
    if (item.kind === ReviewInboxKind.Session) {
      const bits = [item.subject, item.groupName, item.topic].filter(Boolean);
      return bits.join(' · ');
    }
    return '';
  }

  toTime(value?: string): string {
    if (!value) return '';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  itemLink(item: TeacherReviewInboxItemDto): string[] | null {
    if (item.kind === ReviewInboxKind.Lesson && item.lessonId) {
      return ['/teacher/lessons', String(item.lessonId)];
    }
    if (item.kind === ReviewInboxKind.Session && item.sessionId) {
      return ['/teacher/classroom', String(item.sessionId)];
    }
    return null;
  }

  private kindValue(): ReviewInboxKind {
    switch (this.tab()) {
      case 'teacher':
        return ReviewInboxKind.Teacher;
      case 'lesson':
        return ReviewInboxKind.Lesson;
      case 'session':
        return ReviewInboxKind.Session;
      default:
        return ReviewInboxKind.All;
    }
  }

  private loadSummary(silent = false): void {
    if (!silent) this.loadingSummary.set(true);
    this.reviewsApi.getSummary().subscribe({
      next: (data) => {
        this.summary.set(data);
        this.loadingSummary.set(false);
      },
      error: (err) => {
        this.loadingSummary.set(false);
        if (silent) return;
        this.error.set(err?.result?.detail || err?.message || 'Failed to load reviews.');
      },
    });
  }

  private loadList(reset: boolean, silent = false): void {
    const skip = reset ? 0 : this.items().length;
    if (!silent) {
      if (reset) this.loadingList.set(true);
      else this.loadingMore.set(true);
      this.error.set(null);
    }

    this.reviewsApi.getInbox(this.kindValue(), skip, this.pageSize).subscribe({
      next: (data) => {
        this.total.set(data.total);
        this.items.set(reset ? data.items ?? [] : [...this.items(), ...(data.items ?? [])]);
        this.loadingList.set(false);
        this.loadingMore.set(false);
      },
      error: (err) => {
        this.loadingList.set(false);
        this.loadingMore.set(false);
        if (silent) return;
        this.error.set(err?.result?.detail || err?.message || 'Failed to load reviews.');
      },
    });
  }
}
