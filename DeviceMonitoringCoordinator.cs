
using System;
using System.Collections.Generic;
using System.Linq;

namespace LabW08_DelegateEvent;

public class DeviceMonitoringCoordinator : IDisposable
{
    private readonly List<TeachingDevice> devices;

    private readonly ConsoleNotificationService notificationService;
    private readonly DeviceAuditLogger auditLogger;
    private readonly MaintenanceService maintenanceService;
    private readonly FilteredAlertSubscriber? filteredSubscriber;

    private bool subscribed;
    private bool disposed;

    public DeviceMonitoringCoordinator(
        IEnumerable<TeachingDevice> devices,
        ConsoleNotificationService notificationService,
        DeviceAuditLogger auditLogger,
        MaintenanceService maintenanceService,
        FilteredAlertSubscriber? filteredSubscriber = null)
    {
        ArgumentNullException.ThrowIfNull(devices);

        this.notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));

        this.auditLogger = auditLogger
            ?? throw new ArgumentNullException(nameof(auditLogger));

        this.maintenanceService = maintenanceService
            ?? throw new ArgumentNullException(nameof(maintenanceService));

        this.filteredSubscriber = filteredSubscriber;

        this.devices = devices.Distinct().ToList();

        if (this.devices.Any(d => d is null))
            throw new ArgumentException(
                "Device list cannot contain null.", nameof(devices));
    }

    public void Subscribe()
    {
        if (disposed)
            throw new ObjectDisposedException(
                nameof(DeviceMonitoringCoordinator));

        if (subscribed)
            return;

        foreach (TeachingDevice device in devices)
        {
            device.AlertRaised += notificationService.HandleAlert;
            device.AlertRaised += maintenanceService.HandleCriticalAlert;
            device.StatusChanged += auditLogger.HandleStatusChanged;

            if (filteredSubscriber != null)
                device.AlertRaised += filteredSubscriber.HandleAlert;
        }

        subscribed = true;
    }

    public void Unsubscribe()
    {
        if (!subscribed)
            return;

        foreach (TeachingDevice device in devices)
        {
            device.AlertRaised -= notificationService.HandleAlert;
            device.AlertRaised -= maintenanceService.HandleCriticalAlert;
            device.StatusChanged -= auditLogger.HandleStatusChanged;

            if (filteredSubscriber != null)
                device.AlertRaised -= filteredSubscriber.HandleAlert;
        }

        subscribed = false;
    }

    public void Dispose()
    {
        if (disposed)
            return;

        Unsubscribe();
        disposed = true;
    }
}
