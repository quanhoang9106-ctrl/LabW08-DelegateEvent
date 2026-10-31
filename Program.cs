
using System;
using System.Collections.Generic;

namespace LabW08_DelegateEvent;

internal class Program
{
    static void Main()
    {
        RunDemo();
        RunTests();
    }

    private static void RunDemo()
    {
        ProjectorRemote projector = new("P01", "Projector A");
        AirConditionerRemote air = new(
            "A01", "Air conditioner", "B1-201");
        WirelessMicrophone microphone = new("M01", "Microphone");

        List<TeachingDevice> devices = new()
        {
            projector, air, microphone
        };

        ConsoleNotificationService notification = new();
        DeviceAuditLogger logger = new();
        MaintenanceService maintenance = new();

        FilteredAlertSubscriber criticalOnly = new(
            alert => alert.Severity == AlertSeverity.Critical,
            alert => Console.WriteLine(
                $"[FILTER] CRITICAL: {alert.DeviceId} - {alert.Message}"));

        using DeviceMonitoringCoordinator coordinator = new(
            devices, notification, logger, maintenance, criticalOnly);

        coordinator.Subscribe();
        coordinator.Subscribe();

        Console.WriteLine("=== STATUS CHANGES ===");

        projector.ChangeStatus(DeviceStatus.Borrowed);
        projector.ChangeStatus(DeviceStatus.Borrowed);
        projector.ChangeStatus(DeviceStatus.UnderMaintenance);
        microphone.ChangeStatus(DeviceStatus.Borrowed);

        Console.WriteLine("\n=== ALERTS ===");

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
            Console.WriteLine(entry);

        Console.WriteLine("\n=== MAINTENANCE ===");

        foreach (string request in maintenance.Requests)
            Console.WriteLine(request);

        Console.WriteLine(
            $"Total status changes: {logger.Entries.Count}");

        Console.WriteLine(
            $"Total maintenance requests: {maintenance.Requests.Count}");

        coordinator.Unsubscribe();

        projector.UpdateBatteryLevel(80);
        projector.UpdateBatteryLevel(10);

        Console.WriteLine(
            $"Requests after unsubscribe: {maintenance.Requests.Count}");
    }

    private static void RunTests()
    {
        Console.WriteLine("\n=== AUTOMATIC TESTS ===");

        int passed = 0;
        int failed = 0;

        void Check(string name, bool ok)
        {
            Console.WriteLine(
                $"{(ok ? "PASS" : "FAIL")} - {name}");

            if (ok)
                passed++;
            else
                failed++;
        }

        bool Throws<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (T)
            {
                return true;
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        // Test TeachingDevice va ProjectorRemote

        ProjectorRemote projector =
            new("P02", "Projector test");

        int statuses = 0;
        int warnings = 0;
        int criticals = 0;
        bool updatedBeforeEvent = false;

        projector.StatusChanged += (sender, e) =>
        {
            statuses++;

            updatedBeforeEvent =
                ReferenceEquals(sender, projector)
                && projector.Status == e.NewStatus
                && e.OldStatus == DeviceStatus.Available;
        };

        projector.AlertRaised += (sender, e) =>
        {
            if (e.Severity == AlertSeverity.Warning)
                warnings++;

            if (e.Severity == AlertSeverity.Critical)
                criticals++;
        };

        projector.ChangeStatus(DeviceStatus.Borrowed);
        projector.ChangeStatus(DeviceStatus.Borrowed);

        Check(
            "StatusChanged only once for same status",
            statuses == 1);

        Check(
            "Status updated before event",
            updatedBeforeEvent);

        Check(
            "Invalid enum is rejected",
            Throws<ArgumentOutOfRangeException>(
                () => projector.ChangeStatus((DeviceStatus)999))
            && projector.Status == DeviceStatus.Borrowed);

        projector.UpdateBatteryLevel(20);

        Check(
            "Battery at 20 is safe",
            warnings == 0);

        projector.UpdateBatteryLevel(19);
        projector.UpdateBatteryLevel(15);

        Check(
            "One warning when entering low zone",
            warnings == 1);

        projector.UpdateBatteryLevel(0);

        Check(
            "Battery at 0 is critical",
            criticals == 1);

        projector.UpdateBatteryLevel(10);

        Check(
            "Recovery does not trigger new warning",
            warnings == 1);

        projector.UpdateBatteryLevel(0);

        Check(
            "New critical transition is reported",
            criticals == 2);

        Check(
            "Invalid battery rejected and unchanged",
            Throws<InvalidDeviceReadingException>(
                () => projector.UpdateBatteryLevel(-1))
            && projector.BatteryLevel == 0);

        // Test WirelessMicrophone

        WirelessMicrophone mic = new("M02", "Mic test");

        int micWarnings = 0;
        int micCriticals = 0;

        mic.AlertRaised += (sender, e) =>
        {
            if (e.Severity == AlertSeverity.Warning)
                micWarnings++;

            if (e.Severity == AlertSeverity.Critical)
                micCriticals++;
        };

        mic.UpdateBatteryLevel(30);
        mic.UpdateSignalQuality(40);

        Check(
            "Mic thresholds 30 and 40 are safe",
            micWarnings == 0);

        mic.UpdateBatteryLevel(29);
        mic.UpdateSignalQuality(39);

        Check(
            "Mic battery and signal warnings",
            micWarnings == 2);

        mic.UpdateBatteryLevel(0);
        mic.UpdateSignalQuality(0);

        Check(
            "Mic battery and signal criticals",
            micCriticals == 2);

        Check(
            "Invalid signal rejected and unchanged",
            Throws<InvalidDeviceReadingException>(
                () => mic.UpdateSignalQuality(101))
            && mic.SignalQuality == 0);

        // Test AirConditionerRemote

        AirConditionerRemote air =
            new("A02", "Air test", "B1");

        int airAlerts = 0;

        air.AlertRaised += (sender, e) =>
        {
            airAlerts++;
        };

        air.UpdateResponseState(false);
        air.UpdateResponseState(false);

        Check(
            "Unresponsive air alerts once",
            airAlerts == 1);

        air.UpdateResponseState(true);
        air.UpdateResponseState(false);

        Check(
            "New air failure raises new alert",
            airAlerts == 2);

        // Test Subscriber, Delegate va Coordinator

        ProjectorRemote p3 =
            new("P03", "Projector 3");

        DeviceAuditLogger logger = new();
        MaintenanceService maintenance = new();

        int filteredCount = 0;

        FilteredAlertSubscriber filtered = new(
            e => e.Severity == AlertSeverity.Critical,
            e => filteredCount++);

        DeviceMonitoringCoordinator coordinator = new(
            new TeachingDevice[] { p3, p3 },
            new ConsoleNotificationService(),
            logger,
            maintenance,
            filtered);

        coordinator.Subscribe();
        coordinator.Subscribe();

        p3.ChangeStatus(DeviceStatus.Borrowed);
        p3.UpdateBatteryLevel(19);
        p3.UpdateBatteryLevel(0);

        Check(
            "No duplicated status handler",
            logger.Entries.Count == 1);

        Check(
            "Maintenance receives only critical",
            maintenance.Requests.Count == 1);

        Check(
            "Delegate filters only critical",
            filteredCount == 1);

        coordinator.Unsubscribe();
        coordinator.Unsubscribe();

        p3.ChangeStatus(DeviceStatus.UnderMaintenance);
        p3.UpdateBatteryLevel(80);
        p3.UpdateBatteryLevel(0);

        Check(
            "Unsubscribe stops all handlers",
            logger.Entries.Count == 1
            && maintenance.Requests.Count == 1
            && filteredCount == 1);

        coordinator.Dispose();
        coordinator.Dispose();

        Check(
            "Cannot subscribe after Dispose",
            Throws<ObjectDisposedException>(
                () => coordinator.Subscribe()));

        // Test constructor

        Check(
            "Rejects empty device ID",
            Throws<ArgumentException>(
                () => new ProjectorRemote(" ", "Projector")));

        Check(
            "Rejects empty assigned room",
            Throws<ArgumentException>(
                () => new AirConditionerRemote("A03", "Air", " ")));

        Console.WriteLine(
            $"Result: {passed} passed, {failed} failed");

        if (failed > 0)
            throw new Exception("Some automatic tests failed.");
    }
}
