import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ClassroomClient,
  ClassroomMaterialDto,
  StudentClassroomDto,
  StudentExamDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';
import { StudentExamWorkspaceComponent } from './student-exam-workspace';

type ClassroomTab = 'stream' | 'people';

@Component({
  selector: 'app-student-classroom',
  standalone: true,
  imports: [TranslatePipe, DatePipe, RouterLink, StudentExamWorkspaceComponent, UserAvatarComponent],
  templateUrl: './student-classroom.html',
  styleUrls: ['../../classroom/classroom-theme.css', './student-classroom.css'],
})
export class StudentClassroomComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly classroomApi = inject(ClassroomClient);

  readonly sessionId = signal(0);
  readonly loading = signal(false);
  readonly downloadingMaterialId = signal<number | null>(null);
  readonly openingMaterialId = signal<number | null>(null);
  readonly tab = signal<ClassroomTab>('stream');
  readonly examWorkspaceOpen = signal(false);
  readonly error = signal<string | null>(null);
  readonly classroom = signal<StudentClassroomDto | null>(null);
  readonly exam = signal<StudentExamDto | null>(null);
  readonly loadingExam = signal(false);

  ngOnInit(): void {
    this.sessionId.set(Number(this.route.snapshot.paramMap.get('sessionId')));
    this.load();
  }

  load(): void {
    const id = this.sessionId();
    if (!id) {
      this.error.set('Classroom not found.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.classroomApi.getClassroom2(id).subscribe({
      next: (data) => {
        this.classroom.set(data);
        this.loading.set(false);
        this.loadExam();
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load classroom.');
      },
    });
  }

  setTab(tab: ClassroomTab): void {
    this.tab.set(tab);
  }

  openExamWorkspace(): void {
    this.examWorkspaceOpen.set(true);
  }

  closeExamWorkspace(): void {
    this.examWorkspaceOpen.set(false);
    this.loadExam();
  }

  loadExam(): void {
    const id = this.sessionId();
    if (!id) return;

    this.loadingExam.set(true);
    this.classroomApi.getExam2(id).subscribe({
      next: (data) => {
        this.exam.set(data?.id ? data : null);
        this.loadingExam.set(false);
      },
      error: (err) => {
        this.loadingExam.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load exam.');
      },
    });
  }

  examActionLabel(): string {
    const exam = this.exam();
    if (!exam) return 'classroom.openExam';
    if (exam.hasSubmitted) return 'classroom.viewResults';
    if (exam.hasStarted) return 'classroom.continueExam';
    return 'classroom.openExam';
  }

  initials(name?: string | null): string {
    if (!name?.trim()) return 'S';
    const parts = name.trim().split(/\s+/).filter(Boolean);
    return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase() || 'S';
  }

  fileLabel(material: ClassroomMaterialDto): string {
    const name = material.originalFileName || material.title || '';
    const ext = name.includes('.') ? name.slice(name.lastIndexOf('.') + 1).toUpperCase() : 'FILE';
    if (ext === 'PDF') return 'PDF';
    if (ext === 'DOC' || ext === 'DOCX') return 'WORD';
    if (ext === 'XLS' || ext === 'XLSX') return 'EXCEL';
    return ext || 'FILE';
  }

  formatSize(bytes?: number | null): string {
    if (!bytes || bytes <= 0) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  downloadMaterial(material: ClassroomMaterialDto): void {
    this.fetchMaterialBlob(material, 'download');
  }

  openMaterial(material: ClassroomMaterialDto): void {
    this.fetchMaterialBlob(material, 'open');
  }

  statusKey(data: StudentClassroomDto): string {
    if (data.hasEnded) return 'lessons.sessionEndedStatus';
    if (data.hasStarted) return 'lessons.sessionRunning';
    return 'lessons.sessionPending';
  }

  toTimeInput(value?: string): string {
    if (!value) return '—';
    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  private fetchMaterialBlob(material: ClassroomMaterialDto, mode: 'download' | 'open'): void {
    if (!material.hasFile) return;

    if (mode === 'download') this.downloadingMaterialId.set(material.id);
    else this.openingMaterialId.set(material.id);
    this.error.set(null);

    this.classroomApi.downloadMaterialFile2(this.sessionId(), material.id).subscribe({
      next: (response) => {
        this.downloadingMaterialId.set(null);
        this.openingMaterialId.set(null);
        const name = response.fileName || material.originalFileName || material.title;
        if (mode === 'open') this.previewBlob(response.data, name);
        else this.saveBlob(response.data, name);
      },
      error: (err) => {
        this.downloadingMaterialId.set(null);
        this.openingMaterialId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to download file.');
      },
    });
  }

  private saveBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  }

  private previewBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const opened = window.open(url, '_blank', 'noopener');
    if (!opened) {
      this.saveBlob(blob, fileName);
    }
    setTimeout(() => URL.revokeObjectURL(url), 60_000);
  }
}
