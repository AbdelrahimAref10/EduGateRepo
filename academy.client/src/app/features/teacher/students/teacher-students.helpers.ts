import { DayOfWeek, TeacherStudentGroupDto, TeacherStudentListItemDto } from '../../../core/api/academy-api.generated';
import { TranslationService } from '../../../core/i18n/translation.service';

export function parsePositiveId(value: string | null | undefined): number | null {
  if (!value) return null;
  const id = Number(value);
  return Number.isInteger(id) && id > 0 ? id : null;
}

export function readApiError(err: unknown, fallback: string): string {
  const error = err as { result?: { detail?: string }; message?: string };
  return error?.result?.detail || error?.message || fallback;
}

export function parentsLabel(student: TeacherStudentListItemDto, i18n: TranslationService): string {
  const parents = student.parents ?? [];
  if (parents.length === 0) return i18n.t('myStudents.noParent');
  return parents
    .map((parent) => (parent.phoneNumber ? `${parent.fullName} · ${parent.phoneNumber}` : parent.fullName))
    .join(' · ');
}

export function billingKey(type?: string): string {
  return type === 'Monthly' ? 'lessons.monthly' : 'lessons.perSession';
}

export function dayLabel(day: DayOfWeek | number, i18n: TranslationService): string {
  switch (Number(day)) {
    case DayOfWeek.Sunday:
      return i18n.t('lessons.daySunday');
    case DayOfWeek.Monday:
      return i18n.t('lessons.dayMonday');
    case DayOfWeek.Tuesday:
      return i18n.t('lessons.dayTuesday');
    case DayOfWeek.Wednesday:
      return i18n.t('lessons.dayWednesday');
    case DayOfWeek.Thursday:
      return i18n.t('lessons.dayThursday');
    case DayOfWeek.Friday:
      return i18n.t('lessons.dayFriday');
    case DayOfWeek.Saturday:
      return i18n.t('lessons.daySaturday');
    default:
      return String(day);
  }
}

export function formatTime(value?: string): string {
  if (!value) return '—';
  return value.length >= 5 ? value.slice(0, 5) : value;
}

export function formatSchedule(group: TeacherStudentGroupDto, i18n: TranslationService): string {
  const dates = group.dates ?? [];
  if (dates.length === 0) return '—';
  return dates.map((d) => `${dayLabel(d.dayOfWeek, i18n)} ${formatTime(d.startTime)}`).join(' · ');
}

export function capacityLabel(group: TeacherStudentGroupDto, i18n: TranslationService): string {
  if (group.maxCapacity == null) return i18n.t('myStudents.unlimited');
  return `${group.membersCount} ${i18n.t('myStudents.seatsOf')} ${group.maxCapacity}`;
}

export function capacityPercent(group: TeacherStudentGroupDto): number {
  if (!group.maxCapacity || group.maxCapacity <= 0) return 12;
  return Math.min(100, Math.round((group.membersCount / group.maxCapacity) * 100));
}

export function groupStatusKey(group: TeacherStudentGroupDto): string {
  if (group.hasEnded) return 'myStudents.ended';
  if (group.isFull) return 'myStudents.full';
  if (group.isEmpty) return 'myStudents.emptyGroup';
  if (group.hasStarted) return 'myStudents.running';
  return 'myStudents.openSeats';
}

export function canSelectGroup(group: TeacherStudentGroupDto): boolean {
  return !group.isCurrentStudentGroup && !group.isFull && !group.hasEnded;
}

export async function copyText(value?: string | null): Promise<boolean> {
  if (!value) return false;
  try {
    await navigator.clipboard.writeText(value);
    return true;
  } catch {
    return false;
  }
}
