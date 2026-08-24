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
    StudentExamSubmitted = 13,
    ClassroomMaterialAdded = 14,
    PaymentRecorded = 15,
    ChargeCreated = 16,
    MakeupSessionScheduled = 17,
    StudentAbsent = 18,
    StudentPresent = 19,
    SessionStartingSoon = 20
}
