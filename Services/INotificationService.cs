using System;
using System.Threading.Tasks;

namespace EduPlatform.SharedKernel.Services;

public interface INotificationService
{
    // Sent when a minor student requests a parent link. Goes to the parent contact and must
    // include only the student display name and a neutral link (no sensitive student data).
    Task SendParentLinkRequestAsync(
        string parentContact, string studentName, string linkUrl);

    // Reminder for an outstanding parent link request. Should not reveal details about
    // the student's activity or performance, only that approval is pending.
    Task SendParentLinkReminderAsync(
        string parentContact, string studentName);

    // Notifies both guardians that root authority has been transferred. Message must avoid
    // disclosing the reason for transfer or any private student data.
    Task SendRootTransferNotificationAsync(
        Guid fromUserId, Guid toUserId);

    // Sent to a parent when an adult student reduces sharing. Do not include what changed
    // or why, to respect the adult student's privacy choices.
    Task SendSharingReducedNotificationAsync(
        Guid parentId, Guid studentId);

    // Notifies both parties that custody transfer is in progress or completed. This must
    // exclude the sensitive justification or verification details.
    Task SendCustodyTransferNotificationAsync(
        Guid fromParentId, Guid toParentId);

    // Sent to a child (and optionally a guardian log) when a parent unlocks their
    // privacy settings. Must include the timestamp and the details the parent provided,
    // but must NOT include the parent's personal contact info.
    Task SendParentalUnlockNotificationAsync(
        Guid childUserId, Guid parentId, DateTime unlockedAt, string unlockDetails);

    // Sent to a parent when a minor child updates their own privacy settings.
    // Informs the parent that the settings have changed so they can review and re-lock if desired.
    Task SendPrivacySettingsChangedNotificationAsync(
        Guid parentId, Guid studentId);
}
