import { Component, HostListener, OnDestroy, effect, inject, input, output, signal } from '@angular/core';
import {
  AnswerStudentExamQuestionRequest,
  ClassroomClient,
  StudentExamDto,
  StudentExamOptionDto,
  StudentExamQuestionDto,
} from '../../../core/api/academy-api.generated';
import { TranslatePipe } from '../../../core/i18n/translate.pipe';

type ReviewTone = 'correct' | 'wrong' | 'answer' | 'idle';
type QuestionOutcome = 'correct' | 'wrong' | 'skipped';

@Component({
  selector: 'app-student-exam-workspace',
  standalone: true,
  imports: [TranslatePipe],
  templateUrl: './student-exam-workspace.html',
  styleUrls: ['../../classroom/classroom-theme.css'],
})
export class StudentExamWorkspaceComponent implements OnDestroy {
  private readonly classroomApi = inject(ClassroomClient);

  readonly sessionId = input<number | null>(null);
  readonly closed = output<void>();
  readonly completed = output<void>();

  readonly exam = signal<StudentExamDto | null>(null);
  readonly error = signal<string | null>(null);
  readonly loadingExam = signal(false);
  readonly submittingExam = signal(false);
  readonly startingExam = signal(false);
  readonly selectedOptionId = signal<number | null>(null);
  readonly remainingSeconds = signal(0);
  private timerId: ReturnType<typeof setInterval> | null = null;
  private lastSessionId: number | null = null;

  constructor() {
    effect(() => {
      const id = this.sessionId();
      if (id && id !== this.lastSessionId) {
        this.lastSessionId = id;
        this.open(id);
      }
      if (!id && this.lastSessionId) {
        this.lastSessionId = null;
        this.reset();
      }
    });
  }

  ngOnDestroy(): void {
    this.reset();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.sessionId()) this.close();
  }

  close(): void {
    this.reset();
    this.closed.emit();
  }

  startExam(): void {
    const id = this.sessionId();
    if (!id) return;
    this.error.set(null);
    this.startingExam.set(true);
    this.classroomApi.startExam(id).subscribe({
      next: (data) => {
        this.startingExam.set(false);
        this.applyExam(data);
      },
      error: (err) => {
        this.startingExam.set(false);
        this.error.set(err?.error?.detail || err?.result?.detail || err?.message || 'Failed to start exam.');
      },
    });
  }

  selectOption(optionId: number): void {
    if (this.exam()?.hasSubmitted) return;
    this.selectedOptionId.set(optionId);
  }

  submitCurrent(): void {
    const current = this.exam();
    const id = this.sessionId();
    if (!current || !id || current.hasSubmitted || this.submittingExam()) return;

    this.error.set(null);
    this.submittingExam.set(true);
    const request = new AnswerStudentExamQuestionRequest({
      optionId: this.selectedOptionId() ?? undefined,
    });

    this.classroomApi.answerExamQuestion(id, request).subscribe({
      next: (data) => {
        this.submittingExam.set(false);
        this.applyExam(data);
        if (data.hasSubmitted) this.completed.emit();
      },
      error: (err) => {
        this.submittingExam.set(false);
        this.error.set(err?.error?.detail || err?.result?.detail || err?.message || 'Failed to submit answer.');
      },
    });
  }

  formatRemaining(): string {
    const total = Math.max(0, this.remainingSeconds());
    const minutes = Math.floor(total / 60);
    const seconds = total % 60;
    return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
  }

  questionOutcome(question: StudentExamQuestionDto): QuestionOutcome {
    if (question.selectedOptionId == null) return 'skipped';
    const picked = question.options?.find((option) => option.id === question.selectedOptionId);
    return picked?.isCorrect === true ? 'correct' : 'wrong';
  }

  optionTone(question: StudentExamQuestionDto, option: StudentExamOptionDto): ReviewTone {
    const selected = question.selectedOptionId === option.id;
    const correct = option.isCorrect === true;
    if (correct && selected) return 'correct';
    if (correct && !selected) return 'answer';
    if (!correct && selected) return 'wrong';
    return 'idle';
  }

  correctCount(): number {
    return (this.exam()?.questions ?? []).filter((question) => this.questionOutcome(question) === 'correct').length;
  }

  wrongCount(): number {
    return (this.exam()?.questions ?? []).filter((question) => this.questionOutcome(question) !== 'correct').length;
  }

  private open(id: number): void {
    document.body.style.overflow = 'hidden';
    this.error.set(null);
    this.loadingExam.set(true);
    this.classroomApi.getExam2(id).subscribe({
      next: (data) => {
        this.applyExam(data);
        this.loadingExam.set(false);
      },
      error: (err) => {
        this.loadingExam.set(false);
        this.error.set(err?.error?.detail || err?.result?.detail || err?.message || 'Failed to load exam.');
      },
    });
  }

  private applyExam(data: StudentExamDto | null | undefined): void {
    this.exam.set(data?.id ? data : null);
    this.selectedOptionId.set(data?.currentQuestion?.selectedOptionId ?? null);
    this.remainingSeconds.set(data?.remainingSeconds ?? 0);
    this.syncTimer();
  }

  private syncTimer(): void {
    this.stopTimer();
    const exam = this.exam();
    if (!exam?.hasStarted || exam.hasSubmitted || !exam.currentQuestion) return;

    this.timerId = setInterval(() => {
      const next = this.remainingSeconds() - 1;
      if (next <= 0) {
        this.remainingSeconds.set(0);
        this.stopTimer();
        this.submitCurrent();
        return;
      }
      this.remainingSeconds.set(next);
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timerId) {
      clearInterval(this.timerId);
      this.timerId = null;
    }
  }

  private reset(): void {
    this.stopTimer();
    document.body.style.overflow = '';
    this.exam.set(null);
    this.error.set(null);
    this.selectedOptionId.set(null);
    this.remainingSeconds.set(0);
  }
}
