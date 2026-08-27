import { ApplicationRef, Injectable, NgZone, computed, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { NotificationsClient } from '../api/academy-api.generated';
import { TokenStorageService } from '../auth/token-storage.service';
import { TranslationService } from '../i18n/translation.service';

export interface ExamGenerationProgress {
  step: string;
  current: number;
  total: number;
  percent: number;
}

export interface AppNotification {
  id: number;
  notificationId: number;
  title: string;
  body: string;
  time: string;
  read: boolean;
  type: string;
  entityType: string;
  entityId?: number;
  userTargetId?: number;
  createdAtUtc?: Date;
}

const SOUND_URL = '/assets/sounds/notification.wav';
const POLL_MS = 30_000;

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly api = inject(NotificationsClient);
  private readonly tokens = inject(TokenStorageService);
  private readonly i18n = inject(TranslationService);
  private readonly zone = inject(NgZone);
  private readonly appRef = inject(ApplicationRef);

  private readonly itemsSignal = signal<AppNotification[]>([]);
  private readonly liveListeners = new Set<{
    type: string;
    entityId: number;
    onMatch: (item: AppNotification) => void;
  }>();
  private hub?: signalR.HubConnection;
  private connecting = false;
  private pollTimer: ReturnType<typeof setInterval> | null = null;
  private syncReady = false;
  private seenIds = new Set<number>();
  private audio: HTMLAudioElement | null = null;
  private audioUnlocked = false;
  private unlockBound = false;

  private readonly examProgressSignal = signal<ExamGenerationProgress | null>(null);

  readonly items = this.itemsSignal.asReadonly();
  readonly examGenerationProgress = this.examProgressSignal.asReadonly();
  readonly unreadCount = computed(
    () => this.itemsSignal().filter((item) => !item.read).length,
  );

  /** Initial load + live updates (SignalR + polling). */
  startRealtime(): void {
    this.bindAudioUnlock();
    this.ensureAudio();
    this.pullFromApi(false);
    this.startPolling();
    this.connectHub();
  }

  load(force = false): void {
    this.pullFromApi(force);
  }

  refresh(): void {
    this.pullFromApi(true);
  }

  setExamProgress(progress: ExamGenerationProgress): void {
    this.examProgressSignal.set(progress);
  }

  clearExamProgress(): void {
    this.examProgressSignal.set(null);
  }

  clear(): void {
    this.syncReady = false;
    this.seenIds.clear();
    this.itemsSignal.set([]);
    this.examProgressSignal.set(null);
    this.stopPolling();
    void this.stopHub();
  }

  markAllRead(): void {
    this.itemsSignal.update((items) => items.map((item) => ({ ...item, read: true })));
    this.notifyUi();
    this.api.markAllRead().subscribe({ error: () => this.pullFromApi(true) });
  }

  /**
   * Run when a new notification of this type arrives (SignalR or poll).
   * entityId 0 = any. Call the returned function in ngOnDestroy.
   */
  when(
    type: string | readonly string[],
    entityId: number,
    onMatch: (item: AppNotification) => void,
  ): () => void {
    const types = typeof type === 'string' ? [type] : [...type];
    const listeners = types.map((itemType) => ({ type: itemType, entityId, onMatch }));
    for (const listener of listeners) this.liveListeners.add(listener);
    return () => {
      for (const listener of listeners) this.liveListeners.delete(listener);
    };
  }

  markRead(id: number): void {
    this.itemsSignal.update((items) =>
      items.map((item) => (item.id === id ? { ...item, read: true } : item)),
    );
    this.notifyUi();
    this.api.markRead(id).subscribe({ error: () => this.pullFromApi(true) });
  }

  unlockAudio(): void {
    const audio = this.ensureAudio();
    if (!audio) return;

    // Must run inside a user gesture to unlock autoplay.
    const prev = audio.volume;
    audio.muted = true;
    audio.volume = 0;
    void audio
      .play()
      .then(() => {
        audio.pause();
        audio.currentTime = 0;
        audio.muted = false;
        audio.volume = prev || 1;
        this.audioUnlocked = true;
      })
      .catch(() => {
        audio.muted = false;
        audio.volume = prev || 1;
      });
  }

  private pullFromApi(force: boolean): void {
    if (!this.tokens.getAccessToken()) return;

    this.api.getMine().subscribe({
      next: (items) => {
        this.zone.run(() => {
          this.mergeApiItems(
            (items ?? []).map((item) => this.mapApiItem(item)),
            /*allowSound*/ this.syncReady || force,
          );
          this.syncReady = true;
        });
      },
      error: () => undefined,
    });
  }

  private mergeApiItems(incoming: AppNotification[], allowSound: boolean): void {
    const previous = this.itemsSignal();
    const previousIds = new Set(previous.map((x) => x.id));
    const hadSync = this.syncReady;
    const sameSnapshot =
      previous.length === incoming.length &&
      previous.every((item, i) => item.id === incoming[i]?.id && item.read === incoming[i]?.read);

    if (!sameSnapshot) this.itemsSignal.set(incoming);
    for (const item of incoming) {
      this.seenIds.add(item.id);
    }

    // Play only for items that appeared after the first successful sync.
    if (!allowSound || !hadSync) return;

    const brandNew = incoming.filter((item) => !previousIds.has(item.id) && !item.read);
    if (brandNew.length > 0) {
      this.playSound();
      for (const item of brandNew) this.notifyLive(item);
    }
  }

  private ingestExamProgress(raw: unknown): void {
    if (!raw || typeof raw !== 'object') return;
    const p = raw as Record<string, unknown>;
    const current = Number(p['current'] ?? p['Current'] ?? 0);
    const total = Number(p['total'] ?? p['Total'] ?? 0);
    const percent = Number(p['percent'] ?? p['Percent'] ?? 0);
    const step = String(p['step'] ?? p['Step'] ?? '');
    if (!step || !Number.isFinite(current) || !Number.isFinite(total)) return;

    this.examProgressSignal.set({
      step,
      current,
      total: Math.max(1, total),
      percent: Math.max(0, Math.min(100, percent)),
    });
  }

  private ingestPush(raw: unknown): void {
    const payload = this.normalizePayload(raw);
    if (!payload?.id) return;

    if (this.seenIds.has(payload.id) || this.itemsSignal().some((x) => x.id === payload.id)) {
      return;
    }

    const isArabic = this.i18n.language() === 'ar';
    const createdAt = payload.createdAtUtc ? new Date(payload.createdAtUtc) : new Date();

    const next: AppNotification = {
      id: payload.id,
      notificationId: payload.notificationId,
      title: (isArabic ? payload.titleAr : payload.titleEn) || payload.titleEn || payload.titleAr,
      body: (isArabic ? payload.bodyAr : payload.bodyEn) || payload.bodyEn || payload.bodyAr,
      read: payload.isRead,
      type: payload.type,
      entityType: payload.entityType,
      entityId: payload.entityId,
      userTargetId: payload.userTargetId,
      createdAtUtc: createdAt,
      time: this.formatTime(createdAt),
    };

    this.seenIds.add(next.id);
    this.itemsSignal.update((items) => [next, ...items].slice(0, 50));
    this.syncReady = true;
    this.notifyUi();
    this.notifyLive(next);
    if (!next.read) {
      this.playSound();
    }
  }

  private notifyLive(item: AppNotification): void {
    for (const listener of this.liveListeners) {
      if (!this.sameType(listener.type, item.type)) continue;
      if (listener.entityId > 0 && item.entityId !== listener.entityId) continue;
      listener.onMatch(item);
    }
  }

  private sameType(expected: string, actual: string): boolean {
    if (expected === actual) return true;
    const aliases: Record<string, string> = {
      '7': 'TeacherReviewReceived',
      '8': 'LessonReviewReceived',
      '9': 'SessionReviewReceived',
      '12': 'ExamPublished',
      '13': 'StudentExamSubmitted',
      '14': 'ClassroomMaterialAdded',
      '15': 'PaymentRecorded',
      '16': 'ChargeCreated',
    };
    return aliases[actual] === expected;
  }

  private connectHub(): void {
    const token = this.tokens.getAccessToken();
    if (!token || this.connecting) return;
    if (
      this.hub?.state === signalR.HubConnectionState.Connected ||
      this.hub?.state === signalR.HubConnectionState.Connecting ||
      this.hub?.state === signalR.HubConnectionState.Reconnecting
    ) {
      return;
    }

    this.connecting = true;
    void this.stopHub().finally(() => {
      this.hub = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/notifications', {
          accessTokenFactory: () => this.tokens.getAccessToken() ?? '',
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .build();

      this.hub.on('notificationReceived', (payload: unknown) => {
        this.zone.run(() => this.ingestPush(payload));
      });

      this.hub.on('examGenerationProgress', (payload: unknown) => {
        this.zone.run(() => this.ingestExamProgress(payload));
      });

      this.hub.onreconnected(() => {
        this.zone.run(() => this.pullFromApi(true));
      });

      this.hub
        .start()
        .catch(() => undefined)
        .finally(() => {
          this.connecting = false;
        });
    });
  }

  private async stopHub(): Promise<void> {
    const current = this.hub;
    this.hub = undefined;
    if (!current) return;
    try {
      await current.stop();
    } catch {
      /* ignore */
    }
  }

  private startPolling(): void {
    if (this.pollTimer != null) return;
    // Run outside Angular to avoid timer spam, then re-enter zone on updates.
    this.zone.runOutsideAngular(() => {
      this.pollTimer = setInterval(() => {
        if (this.hub?.state === signalR.HubConnectionState.Connected) return;
        this.pullFromApi(false);
      }, POLL_MS);
    });
  }

  private stopPolling(): void {
    if (this.pollTimer != null) {
      clearInterval(this.pollTimer);
      this.pollTimer = null;
    }
  }

  private bindAudioUnlock(): void {
    if (this.unlockBound || typeof document === 'undefined') return;
    this.unlockBound = true;

    const unlock = () => this.unlockAudio();
    document.addEventListener('pointerdown', unlock, { passive: true });
    document.addEventListener('keydown', unlock);
    document.addEventListener('touchstart', unlock, { passive: true });
  }

  private ensureAudio(): HTMLAudioElement | null {
    if (typeof Audio === 'undefined') return null;
    if (!this.audio) {
      this.audio = new Audio(SOUND_URL);
      this.audio.preload = 'auto';
      this.audio.volume = 1;
    }
    return this.audio;
  }

  private playSound(): void {
    const audio = this.ensureAudio();
    if (!audio) return;

    try {
      audio.pause();
      audio.currentTime = 0;
      audio.muted = false;
      audio.volume = 1;
      void audio.play().catch(() => {
        // Still locked — wait for next user gesture.
        this.bindAudioUnlock();
      });
    } catch {
      this.bindAudioUnlock();
    }
  }

  private notifyUi(): void {
    // Force a tick so badge updates even if SignalR/timer ran oddly with CD.
    try {
      this.appRef.tick();
    } catch {
      /* ignore during teardown */
    }
  }

  private normalizePayload(raw: unknown): {
    id: number;
    notificationId: number;
    titleAr: string;
    titleEn: string;
    bodyAr: string;
    bodyEn: string;
    isRead: boolean;
    type: string;
    entityType: string;
    entityId?: number;
    userTargetId?: number;
    createdAtUtc?: string;
  } | null {
    if (!raw || typeof raw !== 'object') return null;
    const p = raw as Record<string, unknown>;
    const id = Number(p['id'] ?? p['Id']);
    if (!Number.isFinite(id) || id <= 0) return null;

    return {
      id,
      notificationId: Number(p['notificationId'] ?? p['NotificationId'] ?? 0),
      titleAr: String(p['titleAr'] ?? p['TitleAr'] ?? ''),
      titleEn: String(p['titleEn'] ?? p['TitleEn'] ?? ''),
      bodyAr: String(p['bodyAr'] ?? p['BodyAr'] ?? ''),
      bodyEn: String(p['bodyEn'] ?? p['BodyEn'] ?? ''),
      isRead: Boolean(p['isRead'] ?? p['IsRead'] ?? false),
      type: String(p['type'] ?? p['Type'] ?? ''),
      entityType: String(p['entityType'] ?? p['EntityType'] ?? ''),
      entityId: this.optionalNumber(p['entityId'] ?? p['EntityId']),
      userTargetId: this.optionalNumber(p['userTargetId'] ?? p['UserTargetId']),
      createdAtUtc: (p['createdAtUtc'] ?? p['CreatedAtUtc']) as string | undefined,
    };
  }

  private optionalNumber(value: unknown): number | undefined {
    if (value == null || value === '') return undefined;
    const n = Number(value);
    return Number.isFinite(n) ? n : undefined;
  }

  private mapApiItem(item: {
    id: number;
    notificationId: number;
    title: string;
    body: string;
    isRead: boolean;
    type: string;
    entityType: string;
    entityId?: number;
    userTargetId?: number;
    createdAtUtc?: Date;
  }): AppNotification {
    return {
      id: item.id,
      notificationId: item.notificationId,
      title: item.title,
      body: item.body,
      read: item.isRead,
      type: item.type,
      entityType: item.entityType,
      entityId: item.entityId,
      userTargetId: item.userTargetId,
      createdAtUtc: item.createdAtUtc,
      time: this.formatTime(item.createdAtUtc),
    };
  }

  private formatTime(value?: Date): string {
    if (!value) return '';
    const date = value instanceof Date ? value : new Date(value);
    const diffMs = Date.now() - date.getTime();
    const minutes = Math.max(0, Math.floor(diffMs / 60000));
    if (minutes < 1) return 'now';
    if (minutes < 60) return `${minutes}m`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours}h`;
    return date.toLocaleDateString();
  }
}
