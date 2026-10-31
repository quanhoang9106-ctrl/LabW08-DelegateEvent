
using System;

namespace LabW08_DelegateEvent;

public class DeviceStatusChangedEventArgs : EventArgs
{
    public string DeviceId { get; }
    public DeviceStatus OldStatus { get; }
    public DeviceStatus NewStatus { get; }
    public DateTime ChangedAt { get; }

    public DeviceStatusChangedEventArgs(
        string deviceId,
        DeviceStatus oldStatus,
        DeviceStatus newStatus,
        DateTime changedAt)
    {
        DeviceId = deviceId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = changedAt;
    }
}

public class DeviceAlertEventArgs : EventArgs
{
    public string DeviceId { get; }
    public string Message { get; }
    public AlertSeverity Severity { get; }
    public DateTime OccurredAt { get; }

    public DeviceAlertEventArgs(
        string deviceId,
        string message,
        AlertSeverity severity,
        DateTime occurredAt)
    {
        DeviceId = deviceId;
        Message = message;
        Severity = severity;
        OccurredAt = occurredAt;
    }
}
