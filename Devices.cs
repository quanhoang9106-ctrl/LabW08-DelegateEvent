
using System;

namespace LabW08_DelegateEvent;

// Quy tac chung cho pin va chat luong tin hieu
internal static class DeviceReadingRules
{
    public static void Validate(string deviceId, int value)
    {
        if (value < 0 || value > 100)
            throw new InvalidDeviceReadingException(deviceId, value);
    }

    public static AlertSeverity GetSeverity(int value, int warningThreshold)
    {
        if (value == 0)
            return AlertSeverity.Critical;

        if (value < warningThreshold)
            return AlertSeverity.Warning;

        return AlertSeverity.Information;
    }

    public static bool BecameMoreSevere(
        int oldValue, int newValue, int warningThreshold)
    {
        return GetSeverity(newValue, warningThreshold)
            > GetSeverity(oldValue, warningThreshold);
    }
}

public class ProjectorRemote : TeachingDevice
{
    private const int LowBatteryThreshold = 20;

    public int BatteryLevel { get; private set; } = 100;

    public ProjectorRemote(string id, string name)
        : base(id, name)
    {
    }

    public void UpdateBatteryLevel(int newLevel)
    {
        DeviceReadingRules.Validate(Id, newLevel);

        int oldLevel = BatteryLevel;

        if (oldLevel == newLevel)
            return;

        BatteryLevel = newLevel;

        if (!DeviceReadingRules.BecameMoreSevere(
                oldLevel, newLevel, LowBatteryThreshold))
            return;

        if (newLevel == 0)
            RaiseAlert(
                "Projector remote battery is empty",
                AlertSeverity.Critical);
        else
            RaiseAlert(
                "Projector remote battery is low",
                AlertSeverity.Warning);
    }
}

public class AirConditionerRemote : TeachingDevice
{
    public string AssignedRoom { get; }
    public bool IsResponsive { get; private set; } = true;

    public AirConditionerRemote(
        string id, string name, string assignedRoom)
        : base(id, name)
    {
        if (string.IsNullOrWhiteSpace(assignedRoom))
            throw new ArgumentException(
                "Room cannot be empty.", nameof(assignedRoom));

        AssignedRoom = assignedRoom;
    }

    public void UpdateResponseState(bool isResponsive)
    {
        bool wasResponsive = IsResponsive;
        IsResponsive = isResponsive;

        if (wasResponsive && !isResponsive)
            RaiseAlert(
                "Air conditioner remote is not responding",
                AlertSeverity.Critical);
    }
}

public class WirelessMicrophone : TeachingDevice
{
    private const int LowBatteryThreshold = 30;
    private const int LowSignalThreshold = 40;

    public int BatteryLevel { get; private set; } = 100;
    public int SignalQuality { get; private set; } = 100;

    public WirelessMicrophone(string id, string name)
        : base(id, name)
    {
    }

    public void UpdateBatteryLevel(int newLevel)
    {
        DeviceReadingRules.Validate(Id, newLevel);

        int oldLevel = BatteryLevel;

        if (oldLevel == newLevel)
            return;

        BatteryLevel = newLevel;

        if (!DeviceReadingRules.BecameMoreSevere(
                oldLevel, newLevel, LowBatteryThreshold))
            return;

        if (newLevel == 0)
            RaiseAlert(
                "Microphone battery is empty",
                AlertSeverity.Critical);
        else
            RaiseAlert(
                "Microphone battery is low",
                AlertSeverity.Warning);
    }

    public void UpdateSignalQuality(int newQuality)
    {
        DeviceReadingRules.Validate(Id, newQuality);

        int oldQuality = SignalQuality;

        if (oldQuality == newQuality)
            return;

        SignalQuality = newQuality;

        if (!DeviceReadingRules.BecameMoreSevere(
                oldQuality, newQuality, LowSignalThreshold))
            return;

        if (newQuality == 0)
            RaiseAlert(
                "Microphone signal is lost",
                AlertSeverity.Critical);
        else
            RaiseAlert(
                "Microphone signal is weak",
                AlertSeverity.Warning);
    }
}

public class InvalidDeviceReadingException : Exception
{
    public string DeviceId { get; }
    public int InvalidValue { get; }

    public InvalidDeviceReadingException(
        string deviceId, int invalidValue)
        : base($"Invalid value {invalidValue} for device {deviceId}. Expected 0..100.")
    {
        DeviceId = deviceId;
        InvalidValue = invalidValue;
    }
}
