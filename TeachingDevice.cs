
using System;

namespace LabW08_DelegateEvent;

public abstract class TeachingDevice
{
    public string Id { get; }
    public string Name { get; }
    public DeviceStatus Status { get; private set; }

    public event EventHandler<DeviceStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<DeviceAlertEventArgs>? AlertRaised;

    protected TeachingDevice(string id, string name)
    {
        Id = id;
        Name = name;
        Status = DeviceStatus.Available;
    }

    public void ChangeStatus(DeviceStatus newStatus)
    {
        if (newStatus == Status)
        {
            return;
        }

        DeviceStatus oldStatus = Status;
        Status = newStatus;

        DeviceStatusChangedEventArgs info =
            new DeviceStatusChangedEventArgs(
                Id, oldStatus, newStatus, DateTime.Now);

        OnStatusChanged(info);
    }

    protected void RaiseAlert(
        string message,
        AlertSeverity severity)
    {
        DeviceAlertEventArgs info =
            new DeviceAlertEventArgs(
                Id, message, severity, DateTime.Now);

        OnAlertRaised(info);
    }

    protected virtual void OnStatusChanged(
        DeviceStatusChangedEventArgs e)
    {
        StatusChanged?.Invoke(this, e);
    }

    protected virtual void OnAlertRaised(
        DeviceAlertEventArgs e)
    {
        AlertRaised?.Invoke(this, e);
    }
}
