
using System;
using System.Collections.Generic;

namespace LabW08_DelegateEvent;

internal class Program
{
    static void Main()
    {
        ProjectorRemote projector =
            new ProjectorRemote("P01", "Projector A");

        AirConditionerRemote air =
            new AirConditionerRemote(
                "A01", "Air conditioner", "B1-201");

        WirelessMicrophone microphone =
            new WirelessMicrophone("M01", "Microphone");

        List<TeachingDevice> devices = new List<TeachingDevice>
        {
            projector, air, microphone
        };

        ConsoleNotificationService notification =
            new ConsoleNotificationService();

        DeviceAuditLogger logger = new DeviceAuditLogger();
        MaintenanceService maintenance = new MaintenanceService();

        FilteredAlertSubscriber criticalOnly =
            new FilteredAlertSubscriber(
                alert => alert.Severity == AlertSeverity.Critical,
                alert => Console.WriteLine(
                    $"[FILTER] CRITICAL: {alert.DeviceId} - {alert.Message}"));

        using (DeviceMonitoringCoordinator coordinator =
            new DeviceMonitoringCoordinator(
                devices, notification, logger,
                maintenance, criticalOnly))
        {
            coordinator.Subscribe();
            coordinator.Subscribe();

            Console.WriteLine("=== DEVICE STATUS ===");

            projector.ChangeStatus(DeviceStatus.Borrowed);
            projector.ChangeStatus(DeviceStatus.Borrowed);
            projector.ChangeStatus(DeviceStatus.UnderMaintenance);
            microphone.ChangeStatus(DeviceStatus.Borrowed);

            Console.WriteLine("\n=== DEVICE ALERTS ===");

            projector.UpdateBatteryLevel(19);
            projector.UpdateBatteryLevel(15);
            projector.UpdateBatteryLevel(0);

            air.UpdateResponseState(false);
            air.UpdateResponseState(false);

            microphone.UpdateBatteryLevel(29);
            microphone.UpdateBatteryLevel(0);

            microphone.UpdateSignalQuality(35);
            microphone.UpdateSignalQuality(0);

            Console.WriteLine("\n=== INVALID INPUT ===");

            try
            {
                microphone.UpdateSignalQuality(150);
            }
            catch (InvalidDeviceReadingException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\n=== AUDIT LOG ===");

            foreach (string entry in logger.Entries)
            {
                Console.WriteLine(entry);
            }

            Console.WriteLine("\n=== MAINTENANCE ===");

            foreach (string request in maintenance.Requests)
            {
                Console.WriteLine(request);
            }

            Console.WriteLine(
                $"Total status changes: {logger.Entries.Count}");

            Console.WriteLine(
                $"Total maintenance requests: {maintenance.Requests.Count}");

            Console.WriteLine("\n=== TEST UNSUBSCRIBE ===");

            coordinator.Unsubscribe();

            projector.UpdateBatteryLevel(80);
            projector.UpdateBatteryLevel(10);

            Console.WriteLine(
                $"Requests after unsubscribe: {maintenance.Requests.Count}");
        }
    }
}
