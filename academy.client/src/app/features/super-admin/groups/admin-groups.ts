import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  AdminGroupListItemDto,
  LessonsOverviewClient,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

@Component({
  selector: 'app-admin-groups',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './admin-groups.html',
  styleUrl: './admin-groups.css',
})
export class AdminGroupsComponent implements OnInit {
  private readonly api = inject(LessonsOverviewClient);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly groups = signal<AdminGroupListItemDto[]>([]);
  readonly selectedGroupId = signal<number | null>(null);

  readonly selectedGroup = computed(() => {
    const id = this.selectedGroupId();
    if (id == null) return null;
    return this.groups().find((x) => x.id === id) ?? null;
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);
    this.api.getAllGroups().subscribe({
      next: (items) => {
        this.groups.set(items ?? []);
        this.loading.set(false);
        const selected = this.selectedGroupId();
        if (selected && !(items ?? []).some((x) => x.id === selected)) {
          this.selectedGroupId.set(null);
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.detail || err?.title || 'Failed to load groups.');
      },
    });
  }

  selectGroup(id: number): void {
    this.selectedGroupId.set(this.selectedGroupId() === id ? null : id);
  }
}
