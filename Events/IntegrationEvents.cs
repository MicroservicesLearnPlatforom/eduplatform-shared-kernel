namespace EduPlatform.SharedKernel.Events;

public record EnrollmentCreatedIntegrationEvent(
    Guid EnrollmentId,
    Guid StudentId,
    Guid CourseId,
    Guid TeacherId,
    string CourseTitle,
    DateTime EnrolledAt) : IntegrationEvent;

public record CourseCreatedIntegrationEvent(
    Guid CourseId,
    Guid InstructorId,
    string Title) : IntegrationEvent;

public record CourseUpdatedIntegrationEvent(
    Guid CourseId,
    string Title,
    string Description) : IntegrationEvent;

public record PaymentCompletedIntegrationEvent(
    Guid PaymentId,
    Guid StudentId,
    Guid CourseId,
    decimal Amount,
    DateTime PaidAt) : IntegrationEvent;

public record CoursePublishedIntegrationEvent(
    Guid CourseId,
    Guid InstructorId,
    string Title,
    string Subject,
    decimal Price) : IntegrationEvent;

public record CourseSoftDeletedIntegrationEvent(Guid CourseId) : IntegrationEvent;

public record CourseUnpublishedIntegrationEvent(Guid CourseId) : IntegrationEvent;

public record LessonCompletedIntegrationEvent(
    Guid StudentId,
    Guid CourseId,
    Guid LessonId) : IntegrationEvent;

public record LessonCreatedIntegrationEvent(
    Guid LessonId,
    Guid CourseId,
    string Title) : IntegrationEvent;

public record QuizCompletedIntegrationEvent(
    Guid StudentId,
    Guid QuizId,
    int? Score,
    bool Passed) : IntegrationEvent;

public record QuizCreatedIntegrationEvent(
    Guid QuizId,
    Guid CourseId,
    Guid CreatorId,
    string Title) : IntegrationEvent;

public record UserRegisteredIntegrationEvent(
    Guid UserId,
    string DisplayName,
    string Role,
    DateOnly? DateOfBirth,
    string CountryCode) : IntegrationEvent;

public record MessageSentIntegrationEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid? SenderUserId,
    IReadOnlyList<Guid> RecipientUserIds,
    string Preview,
    DateTime SentAt) : IntegrationEvent;