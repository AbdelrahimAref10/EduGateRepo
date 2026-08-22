import { DatePipe } from '@angular/common';
import { Component, HostListener, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  ClassroomClient,
  ClassroomMaterialDto,
  ClassroomStudentDetailDto,
  TeacherClassroomDto,
  TeacherExamDto,
  TeacherExamResultsDto,
  TeacherExamReviewOptionDto,
  TeacherExamReviewQuestionDto,
  TeacherStudentExamReviewDto,
  UpdateClassroomInfoRequest,
  UpdateClassroomMaterialRequest,
  UpdateStudentSessionDetailRequest,
} from '../../../core/api/academy-api.generated';
import { ClassroomUploadService } from '../../../core/api/classroom-upload.service';
import { ConfirmDialogService } from '../../../core/ui/confirm-dialog.service';
import { TranslationService } from '../../../core/i18n/translation.service';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';
import { UserAvatarComponent } from '../../../shared/user-avatar/user-avatar';

type ClassroomTab = 'stream' | 'people';
type ExamWorkspaceTab = 'questions' | 'results' | 'review';
type ReviewTone = 'correct' | 'wrong' | 'answer' | 'idle';
type QuestionOutcome = 'correct' | 'wrong' | 'skipped';

interface StudentDraft {
  isPresent: boolean;
  isPaid: boolean;
  teacherNotes: string;
}

const ALLOWED_EXTENSIONS = ['.pdf', '.doc', '.docx', '.xls', '.xlsx'];
const EXAM_FILE_EXTENSIONS = ['.pdf', '.doc', '.docx', '.jpg', '.jpeg', '.png', '.webp'];

@Component({
  selector: 'app-teacher-classroom',
  standalone: true,
  imports: [ReactiveFormsModule, TranslatePipe, DatePipe, RouterLink, UserAvatarComponent],
  templateUrl: './teacher-classroom.html',
  styleUrls: ['../../classroom/classroom-theme.css', './teacher-classroom.css'],
})
export class TeacherClassroomComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly classroomApi = inject(ClassroomClient);
  private readonly uploadApi = inject(ClassroomUploadService);
  private readonly i18n = inject(TranslationService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly sessionId = signal(0);
  readonly loading = signal(false);
  readonly savingInfo = signal(false);
  readonly savingStudentId = signal<number | null>(null);
  readonly savingMaterial = signal(false);
  readonly uploading = signal(false);
  readonly deletingMaterialId = signal<number | null>(null);
  readonly downloadingMaterialId = signal<number | null>(null);
  readonly openingMaterialId = signal<number | null>(null);
  readonly materialModalOpen = signal(false);
  readonly editingMaterialId = signal<number | null>(null);
  readonly tab = signal<ClassroomTab>('stream');
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);
  readonly classroom = signal<TeacherClassroomDto | null>(null);
  readonly exam = signal<TeacherExamDto | null>(null);
  readonly examResults = signal<TeacherExamResultsDto | null>(null);
  readonly generatingExam = signal(false);
  readonly examModalOpen = signal(false);
  readonly examWorkspaceOpen = signal(false);
  readonly examWorkspaceTab = signal<ExamWorkspaceTab>('questions');
  readonly studentReview = signal<TeacherStudentExamReviewDto | null>(null);
  readonly loadingStudentReview = signal(false);
  readonly loadingExamResults = signal(false);
  readonly examFiles = signal<File[]>([]);
  readonly loadingExam = signal(false);
  readonly studentDrafts = signal<Record<number, StudentDraft>>({});
  readonly selectedFile = signal<File | null>(null);

  readonly examForm = this.fb.nonNullable.group({
    questionCount: [10, [Validators.required, Validators.min(5), Validators.max(20)]],
    minutesPerQuestion: [10, [Validators.required, Validators.min(1), Validators.max(60)]],
  });

  readonly infoForm = this.fb.nonNullable.group({
    topic: [''],
    description: [''],
  });

  readonly materialForm = this.fb.nonNullable.group({
    description: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  readonly editMaterialForm = this.fb.nonNullable.group({
    description: ['', [Validators.required, Validators.maxLength(2000)]],
  });

  ngOnInit(): void {
    this.sessionId.set(Number(this.route.snapshot.paramMap.get('sessionId')));
    this.loadClassroom();
  }

  ngOnDestroy(): void {
    this.unlockPage();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.examModalOpen()) {
      this.closeExamModal();
      return;
    }
    if (this.examWorkspaceTab() === 'review') {
      this.closeStudentReview();
      return;
    }
    if (this.examWorkspaceOpen()) this.closeExamWorkspace();
  }

  loadClassroom(): void {
    const id = this.sessionId();
    if (!id) {
      this.error.set('Classroom not found.');
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.classroomApi.getClassroom(id).subscribe({
      next: (data) => {
        this.classroom.set(data);
        this.infoForm.patchValue({
          topic: data.topic ?? '',
          description: data.description ?? '',
        });
        const drafts: Record<number, StudentDraft> = {};
        for (const student of data.students ?? []) {
          drafts[student.studentId] = {
            isPresent: student.isPresent,
            isPaid: student.isPaid,
            teacherNotes: student.teacherNotes ?? '',
          };
        }
        this.studentDrafts.set(drafts);
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
    this.examWorkspaceTab.set(this.exam() ? 'questions' : 'questions');
    this.lockPage();
    this.loadExam();
    if (this.exam()) this.loadExamResults();
  }

  closeExamWorkspace(): void {
    if (this.generatingExam()) return;
    this.examWorkspaceOpen.set(false);
    this.studentReview.set(null);
    this.unlockPage();
  }

  setExamWorkspaceTab(tab: ExamWorkspaceTab): void {
    this.examWorkspaceTab.set(tab);
    this.studentReview.set(null);
    if (tab === 'results') this.loadExamResults();
  }

  openStudentReview(studentId: number): void {
    this.error.set(null);
    this.loadingStudentReview.set(true);
    this.classroomApi.getStudentExamReview(this.sessionId(), studentId).subscribe({
      next: (data) => {
        this.studentReview.set(data);
        this.examWorkspaceTab.set('review');
        this.loadingStudentReview.set(false);
      },
      error: (err) => {
        this.loadingStudentReview.set(false);
        this.error.set(this.httpErrorMessage(err, 'Failed to load student exam.'));
      },
    });
  }

  closeStudentReview(): void {
    this.studentReview.set(null);
    this.examWorkspaceTab.set('results');
  }

  reviewQuestionOutcome(question: TeacherExamReviewQuestionDto): QuestionOutcome {
    if (question.selectedOptionId == null) return 'skipped';
    const picked = question.options?.find((option) => option.id === question.selectedOptionId);
    return picked?.isCorrect === true ? 'correct' : 'wrong';
  }

  reviewOptionTone(question: TeacherExamReviewQuestionDto, option: TeacherExamReviewOptionDto): ReviewTone {
    const selected = question.selectedOptionId === option.id;
    const correct = option.isCorrect === true;
    if (correct && selected) return 'correct';
    if (correct && !selected) return 'answer';
    if (!correct && selected) return 'wrong';
    return 'idle';
  }

  reviewCorrectCount(): number {
    return (this.studentReview()?.questions ?? []).filter(
      (question) => this.reviewQuestionOutcome(question) === 'correct',
    ).length;
  }

  reviewWrongCount(): number {
    return (this.studentReview()?.questions ?? []).filter(
      (question) => this.reviewQuestionOutcome(question) !== 'correct',
    ).length;
  }

  loadExam(): void {
    const id = this.sessionId();
    if (!id) return;

    this.loadingExam.set(true);
    this.classroomApi.getExam(id).subscribe({
      next: (data) => {
        this.exam.set(data?.id ? data : null);
        this.loadingExam.set(false);
        if (data?.id && data.status === 2) this.loadExamResults();
      },
      error: (err) => {
        this.loadingExam.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to load exam.');
      },
    });
  }

  loadExamResults(): void {
    const id = this.sessionId();
    if (!id) return;

    this.loadingExamResults.set(true);
    this.classroomApi.getExamResults(id).subscribe({
      next: (data) => {
        this.examResults.set(data);
        this.loadingExamResults.set(false);
      },
      error: (err) => {
        this.examResults.set(null);
        this.loadingExamResults.set(false);
        this.error.set(this.httpErrorMessage(err, 'Failed to load exam results.'));
      },
    });
  }

  openExamModal(): void {
    this.error.set(null);
    this.examForm.reset({ questionCount: 10, minutesPerQuestion: 10 });
    this.examFiles.set([]);
    this.examModalOpen.set(true);
  }

  closeExamModal(): void {
    if (this.generatingExam()) return;
    this.examModalOpen.set(false);
    this.examFiles.set([]);
  }

  onExamFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const added = Array.from(input.files ?? []);
    const valid: File[] = [];
    for (const file of added) {
      const lower = file.name.toLowerCase();
      if (!EXAM_FILE_EXTENSIONS.some((ext) => lower.endsWith(ext))) {
        this.error.set(this.i18n.t('classroom.examFileInvalid'));
        continue;
      }
      valid.push(file);
    }
    this.examFiles.update((current) => [...current, ...valid].slice(0, 10));
    input.value = '';
  }

  removeExamFile(index: number): void {
    this.examFiles.update((current) => current.filter((_, i) => i !== index));
  }

  generateExam(): void {
    if (this.examForm.invalid) {
      this.examForm.markAllAsTouched();
      return;
    }

    const files = this.examFiles();
    if (!files.length) {
      this.error.set(this.i18n.t('classroom.examNoFiles'));
      return;
    }

    this.error.set(null);
    this.success.set(null);
    this.generatingExam.set(true);

    this.uploadApi.generateExam(
      this.sessionId(),
      this.examForm.controls.questionCount.value,
      this.examForm.controls.minutesPerQuestion.value,
      files,
    ).subscribe({
      next: (data) => {
        this.exam.set(data);
        this.generatingExam.set(false);
        this.examModalOpen.set(false);
        this.examFiles.set([]);
        this.success.set('examGenerated');
        this.examWorkspaceOpen.set(true);
        this.examWorkspaceTab.set('questions');
        this.lockPage();
        this.loadExamResults();
      },
      error: (err) => {
        this.generatingExam.set(false);
        this.error.set(this.httpErrorMessage(err, 'Failed to generate exam.'));
      },
    });
  }

  initials(name?: string | null): string {
    if (!name?.trim()) return 'A';
    const parts = name.trim().split(/\s+/).filter(Boolean);
    return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase() || 'A';
  }

  presentCount(): number {
    return (this.classroom()?.students ?? []).filter((s) => this.studentDraft(s.studentId).isPresent)
      .length;
  }

  fileLabel(material: ClassroomMaterialDto): string {
    const name = material.originalFileName || material.title || '';
    const ext = name.includes('.') ? name.slice(name.lastIndexOf('.') + 1).toUpperCase() : 'FILE';
    if (ext === 'PDF') return 'PDF';
    if (ext === 'DOC' || ext === 'DOCX') return 'WORD';
    if (ext === 'XLS' || ext === 'XLSX') return 'EXCEL';
    return ext || 'FILE';
  }

  examMinutes(seconds?: number | null): number {
    const value = seconds && seconds > 0 ? seconds : 600;
    return Math.max(1, Math.round(value / 60));
  }

  examFileExt(fileName: string): string {
    const ext = fileName.includes('.') ? fileName.slice(fileName.lastIndexOf('.') + 1).toUpperCase() : 'FILE';
    if (ext === 'JPEG') return 'JPG';
    return ext || 'FILE';
  }

  formatSize(bytes?: number | null): string {
    if (!bytes || bytes <= 0) return '';
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  openAddMaterial(): void {
    this.resetMaterialForm();
    this.editingMaterialId.set(null);
    this.materialModalOpen.set(true);
  }

  closeMaterialModal(): void {
    this.materialModalOpen.set(false);
    this.resetMaterialForm();
  }

  saveInfo(): void {
    this.error.set(null);
    this.success.set(null);
    this.savingInfo.set(true);

    const value = this.infoForm.getRawValue();
    const request = new UpdateClassroomInfoRequest({
      topic: value.topic.trim() || undefined,
      description: value.description.trim() || undefined,
    });

    this.classroomApi.updateClassroomInfo(this.sessionId(), request).subscribe({
      next: (data) => {
        this.classroom.set(data);
        this.savingInfo.set(false);
        this.success.set('infoSaved');
      },
      error: (err) => {
        this.savingInfo.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to save classroom info.');
      },
    });
  }

  studentDraft(studentId: number): StudentDraft {
    return (
      this.studentDrafts()[studentId] ?? {
        isPresent: false,
        isPaid: false,
        teacherNotes: '',
      }
    );
  }

  patchStudentDraft(studentId: number, patch: Partial<StudentDraft>): void {
    this.studentDrafts.update((map) => ({
      ...map,
      [studentId]: { ...this.studentDraft(studentId), ...patch },
    }));
  }

  saveStudent(student: ClassroomStudentDetailDto): void {
    const draft = this.studentDraft(student.studentId);
    this.savingStudentId.set(student.studentId);
    this.error.set(null);

    const request = new UpdateStudentSessionDetailRequest({
      isPresent: draft.isPresent,
      isPaid: draft.isPaid,
      teacherNotes: draft.teacherNotes.trim() || undefined,
    });

    this.classroomApi.updateStudentDetail(this.sessionId(), student.studentId, request).subscribe({
      next: (updated) => {
        this.savingStudentId.set(null);
        this.classroom.update((c) => {
          if (!c) return c;
          const students = (c.students ?? []).map((s) =>
            s.studentId === updated.studentId ? updated : s,
          );
          return TeacherClassroomDto.fromJS({ ...c.toJSON(), students });
        });
        this.success.set('studentSaved');
      },
      error: (err) => {
        this.savingStudentId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to save student.');
      },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.selectedFile.set(file);
    if (file && !this.isAllowedFile(file.name)) {
      this.error.set(this.i18n.t('classroom.fileTypeInvalid'));
      this.selectedFile.set(null);
      input.value = '';
    }
  }

  submitMaterial(): void {
    this.error.set(null);
    this.success.set(null);

    if (this.materialForm.invalid) {
      this.materialForm.markAllAsTouched();
      return;
    }

    const file = this.selectedFile();
    if (!file) {
      this.error.set(this.i18n.t('classroom.fileRequired'));
      return;
    }
    if (!this.isAllowedFile(file.name)) {
      this.error.set(this.i18n.t('classroom.fileTypeInvalid'));
      return;
    }

    const description = this.materialForm.controls.description.value.trim();
    this.uploading.set(true);

    this.uploadApi
      .uploadMaterial(this.sessionId(), {
        file,
        description,
        title: file.name.replace(/\.[^.]+$/, ''),
      })
      .subscribe({
        next: () => {
          this.uploading.set(false);
          this.closeMaterialModal();
          this.success.set('materialCreated');
          this.loadClassroom();
        },
        error: (err) => {
          this.uploading.set(false);
          this.error.set(err?.result?.detail || err?.message || 'Failed to upload material.');
        },
      });
  }

  startEditMaterial(material: ClassroomMaterialDto): void {
    this.editingMaterialId.set(material.id);
    this.editMaterialForm.patchValue({
      description: material.description ?? '',
    });
  }

  cancelEditMaterial(): void {
    this.editingMaterialId.set(null);
  }

  saveMaterialEdit(material: ClassroomMaterialDto): void {
    if (this.editMaterialForm.invalid) {
      this.editMaterialForm.markAllAsTouched();
      return;
    }

    const description = this.editMaterialForm.controls.description.value.trim();
    this.savingMaterial.set(true);
    this.error.set(null);

    const request = new UpdateClassroomMaterialRequest({
      title: material.title,
      description,
    });

    this.classroomApi.updateMaterial(this.sessionId(), material.id, request).subscribe({
      next: () => {
        this.savingMaterial.set(false);
        this.editingMaterialId.set(null);
        this.success.set('materialUpdated');
        this.loadClassroom();
      },
      error: (err) => {
        this.savingMaterial.set(false);
        this.error.set(err?.result?.detail || err?.message || 'Failed to update material.');
      },
    });
  }

  deleteMaterial(material: ClassroomMaterialDto): void {
    void this.runDeleteMaterial(material);
  }

  private async runDeleteMaterial(material: ClassroomMaterialDto): Promise<void> {
    const ok = await this.confirmDialog.ask({
      messageKey: 'classroom.confirmDeleteMaterial',
      confirmKey: 'common.delete',
      tone: 'danger',
    });
    if (!ok) return;

    this.deletingMaterialId.set(material.id);
    this.error.set(null);

    this.classroomApi.deleteMaterial(this.sessionId(), material.id).subscribe({
      next: () => {
        this.deletingMaterialId.set(null);
        this.success.set('materialDeleted');
        this.loadClassroom();
      },
      error: (err) => {
        this.deletingMaterialId.set(null);
        this.error.set(err?.result?.detail || err?.message || 'Failed to delete material.');
      },
    });
  }

  downloadMaterial(material: ClassroomMaterialDto): void {
    this.fetchMaterialBlob(material, 'download');
  }

  openMaterial(material: ClassroomMaterialDto): void {
    this.fetchMaterialBlob(material, 'open');
  }

  statusKey(data: TeacherClassroomDto): string {
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

    this.classroomApi.downloadMaterialFile(this.sessionId(), material.id).subscribe({
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

  private isAllowedFile(fileName: string): boolean {
    const lower = fileName.toLowerCase();
    return ALLOWED_EXTENSIONS.some((ext) => lower.endsWith(ext));
  }

  private resetMaterialForm(): void {
    this.materialForm.reset({ description: '' });
    this.selectedFile.set(null);
  }

  private lockPage(): void {
    document.body.style.overflow = 'hidden';
  }

  private unlockPage(): void {
    document.body.style.overflow = '';
  }

  private httpErrorMessage(err: unknown, fallback: string): string {
    const body = (err as { error?: unknown; result?: unknown })?.error
      ?? (err as { result?: unknown })?.result;
    if (typeof body === 'string' && body.trim()) return body;
    if (body && typeof body === 'object') {
      const problem = body as { detail?: string; errors?: Record<string, string[] | string> };
      const errors = problem.errors;
      if (errors) {
        const first = Object.values(errors)
          .flatMap((value) => (Array.isArray(value) ? value : [value]))
          .find((item) => !!item);
        if (first) return String(first);
      }
      if (problem.detail) return problem.detail;
    }
    return (err as { message?: string })?.message || fallback;
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
