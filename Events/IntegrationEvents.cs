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

// ── Teacher onboarding / activation ──────────────────────────

public record TeacherApplicationSubmittedIntegrationEvent(
    Guid ApplicationId,
    Guid TeacherUserId,
    DateTime SubmittedAt) : IntegrationEvent;

public record TeacherApplicationRejectedIntegrationEvent(
    Guid ApplicationId,
    Guid TeacherUserId,
    Guid AdminUserId,
    string Reason) : IntegrationEvent;

public record TeacherInterviewScheduledIntegrationEvent(
    Guid InterviewId,
    Guid ApplicationId,
    Guid TeacherUserId,
    Guid AdminUserId,
    DateTime ScheduledAtUtc,
    int DurationMinutes) : IntegrationEvent;

public record TeacherInterviewCompletedIntegrationEvent(
    Guid InterviewId,
    Guid ApplicationId,
    Guid TeacherUserId,
    string Outcome,
    DateTime CompletedAt) : IntegrationEvent;

public record TeacherApplicationApprovedIntegrationEvent(
    Guid ApplicationId,
    Guid TeacherUserId,
    Guid AdminUserId) : IntegrationEvent;

public record TeacherContractGeneratedIntegrationEvent(
    Guid ContractId,
    Guid ApplicationId,
    Guid TeacherUserId,
    string TemplateVersion) : IntegrationEvent;

public record TeacherContractSignedIntegrationEvent(
    Guid ContractId,
    Guid ApplicationId,
    Guid TeacherUserId,
    string TemplateVersion,
    string ContractHash,
    DateTime SignedAt) : IntegrationEvent;

public record TeacherActivatedIntegrationEvent(
    Guid TeacherUserId,
    Guid ApplicationId,
    Guid ContractId) : IntegrationEvent;

public record TeacherTierChangedIntegrationEvent(
    Guid TeacherUserId,
    int Level,
    string Reason,
    Guid? ChangedByAdminId) : IntegrationEvent;

public record OtpVerifiedIntegrationEvent(
    Guid? UserId,
    string Purpose,
    string Channel,
    string MaskedDestination) : IntegrationEvent;

// ── Private lesson session lifecycle ─────────────────────────

public record SessionCompletedIntegrationEvent(
    Guid AppointmentId,
    Guid LessonRequestId,
    Guid StudentId,
    Guid TeacherId,
    DateTime ScheduledAt,
    int DurationMinutes,
    DateTime CompletedAt) : IntegrationEvent;

public record SessionCancelledIntegrationEvent(
    Guid AppointmentId,
    Guid LessonRequestId,
    Guid StudentId,
    Guid TeacherId,
    DateTime ScheduledAt,
    string CancelledByRole,
    DateTime CancelledAt,
    int RefundPercent) : IntegrationEvent;

// ── Wallet / escrow ──────────────────────────────────────────

public record EscrowHeldIntegrationEvent(
    Guid EscrowHoldId,
    Guid PayerUserId,
    Guid PayeeUserId,
    Guid ReferenceId,
    string ReferenceType,
    decimal Amount,
    string Currency,
    DateTime HeldAt) : IntegrationEvent;

public record EscrowReleasedIntegrationEvent(
    Guid EscrowHoldId,
    Guid PayerUserId,
    Guid PayeeUserId,
    Guid ReferenceId,
    string ReferenceType,
    decimal Amount,
    string Currency,
    DateTime ReleasedAt) : IntegrationEvent;

public record RefundIssuedIntegrationEvent(
    Guid EscrowHoldId,
    Guid PayerUserId,
    Guid PayeeUserId,
    Guid ReferenceId,
    string ReferenceType,
    decimal RefundedAmount,
    decimal ForfeitedAmount,
    int RefundPercent,
    DateTime RefundedAt) : IntegrationEvent;

public record PayoutRequestedIntegrationEvent(
    Guid PayoutRequestId,
    Guid ProviderUserId,
    decimal Amount,
    string Currency,
    DateTime RequestedAt) : IntegrationEvent;

// ── Ratings ──────────────────────────────────────────────────

public record SessionRatingSubmittedIntegrationEvent(
    Guid RatingId,
    Guid AppointmentId,
    Guid RaterUserId,
    Guid RatedUserId,
    double WeightedScore,
    DateTime SubmittedAt) : IntegrationEvent;

public record TeacherRatingAggregateUpdatedIntegrationEvent(
    Guid TeacherUserId,
    double AverageRating,
    int RatingCount,
    DateTime UpdatedAt) : IntegrationEvent;

// ── Trust & safety ───────────────────────────────────────────

public record MessageFlaggedIntegrationEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid? SenderUserId,
    string EscalationLevel,
    string RuleCode,
    DateTime FlaggedAt) : IntegrationEvent;

public record SafetyCaseOpenedIntegrationEvent(
    Guid CaseId,
    string CaseNumber,
    Guid ReporterUserId,
    Guid? TargetUserId,
    Guid? ConversationId,
    string Source,
    DateTime OpenedAt) : IntegrationEvent;

public record ComplaintFiledIntegrationEvent(
    Guid ComplaintId,
    string? CaseNumber,
    Guid FilerUserId,
    Guid TargetUserId,
    string Tier,
    DateTime FiledAt) : IntegrationEvent;

public record RelationshipFrozenIntegrationEvent(
    Guid FreezeId,
    Guid UserAId,
    Guid UserBId,
    Guid? CaseId,
    string Reason,
    DateTime FrozenAt) : IntegrationEvent;
