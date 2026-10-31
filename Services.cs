
using System;
using System.Collections.Generic;

namespace LabW08_DelegateEvent;

public class ConsoleNotificationService
{
    public void HandleAlert(object? sender, DeviceAlertEventArgs e)
    {
        Console.WriteLine($"[{e.Severity}] {e.DeviceId}: {e.Message}");
    }
}

public class DeviceAuditLogger
{
    private readonly List<string> entries = new();

    public IReadOnlyList<string> Entries => entries.AsReadOnly();

    public void HandleStatusChanged(
        object? sender, DeviceStatusChangedEventArgs e)
    {
        entries.Add(
            $"{e.ChangedAt}: {e.DeviceId}, {e.OldStatus} -> {e.NewStatus}");
    }
}

public class MaintenanceService
{
    private readonly List<string> requests = new();

    public IReadOnlyList<string> Requests => requests.AsReadOnly();

    public void HandleCriticalAlert(
        object? sender, DeviceAlertEventArgs e)
    {
        if (e.Severity == AlertSeverity.Critical)
            requests.Add(
                $"Maintenance request for {e.DeviceId}: {e.Message}");
    }
}

public delegate bool AlertFilter(DeviceAlertEventArgs alert);

public class FilteredAlertSubscriber
{
    private readonly AlertFilter filter;
    private readonly Action<DeviceAlertEventArgs> action;

    public FilteredAlertSubscriber(
        AlertFilter filter,
        Action<DeviceAlertEventArgs> action)
    {
        this.filter = filter
            ?? throw new ArgumentNullException(nameof(filter));

        this.action = action
            ?? throw new ArgumentNullException(nameof(action));
    }

    public void HandleAlert(object? sender, DeviceAlertEventArgs e)
    {
        if (filter(e))
            action(e);
    }
}
