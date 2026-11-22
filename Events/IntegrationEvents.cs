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

public record CourseUnpublishedIntegrationEvent(
    Guid CourseId,
    Guid InstructorId,
    string Title) : IntegrationEvent;

public record LessonCompletedIntegrationEvent(
    Guid StudentId,
    Guid CourseId,
    Guid LessonId) : IntegrationEvent;

// EnrolledStudentIds: the lesson-service resolves the recipients (it can ask the course-service),
// so consumers like the notification-service don't need to know about enrollments.
public record LessonCreatedIntegrationEvent(
    Guid LessonId,
    Guid CourseId,
    string Title,
    IReadOnlyList<Guid> EnrolledStudentIds) : IntegrationEvent;

public record LessonPublishedIntegrationEvent(
    Guid LessonId,
    Guid CourseId,
    string Title,
    IReadOnlyList<Guid> EnrolledStudentIds) : IntegrationEvent;

public record QuizCompletedIntegrationEvent(
    Guid StudentId,
    Guid QuizId,
    int? Score,
    bool Passed) : IntegrationEvent;

public record QuizCreatedIntegrationEvent(
    Guid QuizId,
    Guid CourseId,
    Guid CreatorId,
    string Title,
    IReadOnlyList<Guid> EnrolledStudentIds) : IntegrationEvent;

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

// ── Booking: private-lesson <-> appointment (A-01) ───────────
// The appointment-service is the only owner of the calendar (slots + appointments).
// The private-lesson-service owns requests, payment and the lesson itself and
// keeps only the AppointmentId. Both sides talk through these events instead of HTTP.
//
//   private-lesson                         appointment
//   LessonRequestAccepted     ───────────►  book the slot
//                             ◄───────────  AppointmentBooked | AppointmentBookingFailed
//   LessonRequestCancelled    ───────────►  cancel the appointment, free the slot
//                             ◄───────────  AppointmentCancelled (also when cancelled in the calendar)

// The teacher accepted a request: book TimeSlotId for the student.
public record LessonRequestAcceptedIntegrationEvent(
    Guid LessonRequestId,
    Guid StudentId,
    Guid TeacherId,
    Guid TimeSlotId,
    string? Notes) : IntegrationEvent;

// A request was rejected or cancelled: cancel its appointment and free the slot.
// AppointmentId can still be null if the AppointmentBooked answer has not arrived yet;
// the appointment-service then finds the booking by TimeSlotId + StudentId.
public record LessonRequestCancelledIntegrationEvent(
    Guid LessonRequestId,
    Guid StudentId,
    Guid TeacherId,
    Guid? TimeSlotId,
    Guid? AppointmentId,
    string Reason) : IntegrationEvent;

// A slot was booked. LessonRequestId is set when the booking came from a private lesson,
// null for a direct booking in the calendar.
public record AppointmentBookedIntegrationEvent(
    Guid AppointmentId,
    Guid TimeSlotId,
    Guid HostId,
    Guid BookerId,
    DateTime StartsAt,
    DateTime EndsAt,
    Guid? LessonRequestId) : IntegrationEvent;

// The slot from LessonRequestAccepted could not be booked (taken meanwhile, deleted, ...).
// The private-lesson-service has to cancel the request and refund the escrow.
public record AppointmentBookingFailedIntegrationEvent(
    Guid LessonRequestId,
    Guid TimeSlotId,
    string Reason) : IntegrationEvent;

// An appointment was cancelled - by a LessonRequestCancelled event or directly in the calendar.
public record AppointmentCancelledIntegrationEvent(
    Guid AppointmentId,
    Guid TimeSlotId,
    Guid HostId,
    Guid BookerId,
    Guid? CancelledByUserId,
    DateTime StartsAt) : IntegrationEvent;
