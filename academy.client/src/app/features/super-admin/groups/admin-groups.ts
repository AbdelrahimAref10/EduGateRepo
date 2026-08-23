import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import {
  AdminGroupListItemDto,
  AdminGroupSessionsDto,
  LessonsOverviewClient,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { PageLoaderComponent } from '../../../shared/page-loader/page-loader';

type Panel = 'members' | 'sessions';

@Component({
  selector: 'app-admin-groups',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink, PageLoaderComponent],
  templateUrl: './admin-groups.html',
  styleUrl: './admin-groups.css',
})
export class AdminGroupsComponent implements OnInit {
  private readonly api = inject(LessonsOverviewClient);
  private readonly destroyRef = inject(DestroyRef);

  readonly loading = signal(true);
  readonly ready = signal(false);
  readonly error = signal<string | null>(null);
  readonly groups = signal<AdminGroupListItemDto[]>([]);
  readonly selectedGroupId = signal<number | null>(null);
  readonly panel = signal<Panel | null>(null);
  readonly sessionsLoadingId = signal<number | null>(null);
  readonly sessionsCache = signal<Record<number, AdminGroupSessionsDto>>({});

  readonly selectedGroup = computed(() => {
    const id = this.selectedGroupId();
    if (id == null) return null;
    return this.groups().find((x) => x.id === id) ?? null;
  });

  readonly selectedSessions = computed(() => {
    const id = this.selectedGroupId();
    if (id == null || this.panel() !== 'sessions') return null;
    return this.sessionsCache()[id] ?? null;
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.sessionsCache.set({});
    this.api.getAllGroups().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (items) => {
        this.groups.set(items ?? []);
        this.loading.set(false);
        this.ready.set(true);
        const selected = this.selectedGroupId();
        if (selected && !(items ?? []).some((x) => x.id === selected)) {
          this.selectedGroupId.set(null);
          this.panel.set(null);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.ready.set(true);
        this.error.set(this.apiError(err, 'Failed to load groups.'));
      },
    });
  }

  toggleMembers(id: number): void {
    if (this.selectedGroupId() === id && this.panel() === 'members') {
      this.closePanel();
      return;
    }
    this.selectedGroupId.set(id);
    this.panel.set('members');
  }

  toggleSessions(id: number): void {
    if (this.selectedGroupId() === id && this.panel() === 'sessions') {
      this.closePanel();
      return;
    }
    this.selectedGroupId.set(id);
    this.panel.set('sessions');
    this.ensureSessions(id);
  }

  billingLabel(value?: string): string {
    return value === 'Monthly' ? 'lessons.monthly' : 'lessons.perSession';
  }

  price(group: { billingType?: string; sessionPrice?: number; monthlyPrice?: number }): string | number {
    if (group.billingType === 'Monthly') return group.monthlyPrice ?? '—';
    return group.sessionPrice ?? '—';
  }

  toTime(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  sessionStatusKey(session: { hasEnded?: boolean; hasStarted?: boolean }): string {
    if (session.hasEnded) return 'adminGroups.ended';
    if (session.hasStarted) return 'adminGroups.started';
    return 'adminGroups.notStarted';
  }

  private closePanel(): void {
    this.selectedGroupId.set(null);
    this.panel.set(null);
  }

  private ensureSessions(groupId: number): void {
    if (this.sessionsCache()[groupId] || this.sessionsLoadingId() === groupId) return;

    this.sessionsLoadingId.set(groupId);
    this.api.getGroupSessions(groupId).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (data) => {
        this.sessionsCache.update((cache) => ({ ...cache, [groupId]: data }));
        if (this.sessionsLoadingId() === groupId) this.sessionsLoadingId.set(null);
      },
      error: (err) => {
        if (this.sessionsLoadingId() === groupId) this.sessionsLoadingId.set(null);
        this.error.set(this.apiError(err, 'Failed to load sessions.'));
      },
    });
  }

  private apiError(err: unknown, fallback: string): string {
    const e = err as { detail?: string; title?: string; result?: { detail?: string; title?: string } };
    return e?.detail || e?.title || e?.result?.detail || e?.result?.title || fallback;
  }
}
