using System;
using System.Threading.Tasks;

namespace EduPlatform.SharedKernel.Services;

public class NotificationServiceStub : INotificationService
{
    public Task SendParentLinkRequestAsync(string parentContact, string studentName, string linkUrl)
    {
        return Task.CompletedTask;
    }

    public Task SendParentLinkReminderAsync(string parentContact, string studentName)
    {
        return Task.CompletedTask;
    }

    public Task SendRootTransferNotificationAsync(Guid fromUserId, Guid toUserId)
    {
        return Task.CompletedTask;
    }

    public Task SendSharingReducedNotificationAsync(Guid parentId, Guid studentId)
    {
        return Task.CompletedTask;
    }

    public Task SendCustodyTransferNotificationAsync(Guid fromParentId, Guid toParentId)
    {
        return Task.CompletedTask;
    }

    public Task SendParentalUnlockNotificationAsync(
        Guid childUserId, Guid parentId, DateTime unlockedAt, string unlockDetails)
    {
        return Task.CompletedTask;
    }

    public Task SendPrivacySettingsChangedNotificationAsync(
        Guid parentId, Guid studentId)
    {
        return Task.CompletedTask;
    }
}
