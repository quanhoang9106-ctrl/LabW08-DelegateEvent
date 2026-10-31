
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

    private bool subscribed = false;

    public DeviceMonitoringCoordinator(
        IEnumerable<TeachingDevice> devices,
        ConsoleNotificationService notificationService,
        DeviceAuditLogger auditLogger,
        MaintenanceService maintenanceService,
        FilteredAlertSubscriber? filteredSubscriber = null)
    {
        this.devices = devices.Distinct().ToList();
        this.notificationService = notificationService;
        this.auditLogger = auditLogger;
        this.maintenanceService = maintenanceService;
        this.filteredSubscriber = filteredSubscriber;
    }

    public void Subscribe()
    {
        if (subscribed)
        {
            return;
        }

        foreach (TeachingDevice device in devices)
        {
            device.AlertRaised += notificationService.HandleAlert;
            device.AlertRaised += maintenanceService.HandleCriticalAlert;
            device.StatusChanged += auditLogger.HandleStatusChanged;

            if (filteredSubscriber != null)
            {
                device.AlertRaised += filteredSubscriber.HandleAlert;
            }
        }

        subscribed = true;
    }

    public void Unsubscribe()
    {
        if (!subscribed)
        {
            return;
        }

        foreach (TeachingDevice device in devices)
        {
            device.AlertRaised -= notificationService.HandleAlert;
            device.AlertRaised -= maintenanceService.HandleCriticalAlert;
            device.StatusChanged -= auditLogger.HandleStatusChanged;

            if (filteredSubscriber != null)
            {
                device.AlertRaised -= filteredSubscriber.HandleAlert;
            }
        }

        subscribed = false;
    }

    public void Dispose()
    {
        Unsubscribe();
    }
}
