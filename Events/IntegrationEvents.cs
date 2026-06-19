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

// Published by Identity.Service after a new user completes registration.
// Profile.Service listens for this to create the base + role-specific profile automatically.
// Role is passed as string to keep SharedKernel free of service-specific enums.
public record UserRegisteredIntegrationEvent(
    Guid UserId,
    string DisplayName,
    string Role,
    DateOnly? DateOfBirth,
    string CountryCode) : IntegrationEvent;
