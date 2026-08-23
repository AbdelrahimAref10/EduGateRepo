namespace Academy.Domain.Enums;

public enum NotificationType
{
    LessonBookingRequested = 1,
    LessonBookingConfirmed = 2,
    LessonBookingRejected = 3,
    StudentAddedToLesson = 4,
    LessonStarted = 5,
    LessonGroupEnded = 6,
    TeacherReviewReceived = 7,
    LessonReviewReceived = 8,
    SessionReviewReceived = 9,
    StudentAddedToGroup = 10,
    SessionStarted = 11,
    ExamPublished = 12,
    StudentExamSubmitted = 13
}
