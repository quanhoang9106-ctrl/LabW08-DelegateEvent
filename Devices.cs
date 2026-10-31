
using System;

namespace LabW08_DelegateEvent;

public class ProjectorRemote : TeachingDevice
{
    public int BatteryLevel { get; private set; } = 100;

    public ProjectorRemote(string id, string name)
        : base(id, name)
    {
    }

    public void UpdateBatteryLevel(int newLevel)
    {
        if (newLevel < 0 || newLevel > 100)
        {
            throw new InvalidDeviceReadingException(Id, newLevel);
        }

        int oldLevel = BatteryLevel;
        BatteryLevel = newLevel;

        if (newLevel == 0 && oldLevel != 0)
        {
            RaiseAlert(
                "Projector remote battery is empty",
                AlertSeverity.Critical);
        }
        else if (newLevel > 0 && newLevel < 20
                 && oldLevel >= 20)
        {
            RaiseAlert(
                "Projector remote battery is low",
                AlertSeverity.Warning);
        }
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
        AssignedRoom = assignedRoom;
    }

    public void UpdateResponseState(bool isResponsive)
    {
        bool oldResponse = IsResponsive;
        IsResponsive = isResponsive;

        if (oldResponse && !isResponsive)
        {
            RaiseAlert(
                "Air conditioner remote is not responding",
                AlertSeverity.Critical);
        }
    }
}

public class WirelessMicrophone : TeachingDevice
{
    public int BatteryLevel { get; private set; } = 100;
    public int SignalQuality { get; private set; } = 100;

    public WirelessMicrophone(string id, string name)
        : base(id, name)
    {
    }

    public void UpdateBatteryLevel(int newLevel)
    {
        if (newLevel < 0 || newLevel > 100)
        {
            throw new InvalidDeviceReadingException(Id, newLevel);
        }

        int oldLevel = BatteryLevel;
        BatteryLevel = newLevel;

        if (newLevel == 0 && oldLevel != 0)
        {
            RaiseAlert(
                "Microphone battery is empty",
                AlertSeverity.Critical);
        }
        else if (newLevel > 0 && newLevel < 30
                 && oldLevel >= 30)
        {
            RaiseAlert(
                "Microphone battery is low",
                AlertSeverity.Warning);
        }
    }

    public void UpdateSignalQuality(int newQuality)
    {
        if (newQuality < 0 || newQuality > 100)
        {
            throw new InvalidDeviceReadingException(Id, newQuality);
        }

        int oldQuality = SignalQuality;
        SignalQuality = newQuality;

        if (newQuality == 0 && oldQuality != 0)
        {
            RaiseAlert(
                "Microphone signal is lost",
                AlertSeverity.Critical);
        }
        else if (newQuality > 0 && newQuality < 40
                 && oldQuality >= 40)
        {
            RaiseAlert(
                "Microphone signal is weak",
                AlertSeverity.Warning);
        }
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
