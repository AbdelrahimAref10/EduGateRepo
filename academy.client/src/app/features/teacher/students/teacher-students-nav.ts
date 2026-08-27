import { Injectable, signal } from '@angular/core';
import { TeacherStudentLessonDto, TeacherStudentListItemDto } from '../../../core/api/academy-api.generated';

@Injectable({ providedIn: 'root' })
export class TeacherStudentsNav {
  readonly student = signal<TeacherStudentListItemDto | null>(null);
  readonly lessons = signal<TeacherStudentLessonDto[]>([]);
  private lessonsStudentId: number | null = null;

  rememberStudent(student: TeacherStudentListItemDto): void {
    this.student.set(student);
  }

  studentFor(studentId: number): TeacherStudentListItemDto | null {
    const current = this.student();
    return current?.studentId === studentId ? current : null;
  }

  rememberLessons(studentId: number, items: TeacherStudentLessonDto[]): void {
    this.lessonsStudentId = studentId;
    this.lessons.set(items);
  }

  lessonsFor(studentId: number): TeacherStudentLessonDto[] | null {
    return this.lessonsStudentId === studentId ? this.lessons() : null;
  }

  lessonFor(studentId: number, lessonId: number): TeacherStudentLessonDto | null {
    return this.lessonsFor(studentId)?.find((item) => item.lessonId === lessonId) ?? null;
  }

  patchAssignedGroup(lessonId: number, groupId: number, groupName: string): void {
    this.lessons.update((items) =>
      items.map((item) => {
        if (item.lessonId !== lessonId) return item;
        item.assignedGroupId = groupId;
        item.assignedGroupName = groupName;
        return item;
      }),
    );
  }
}
