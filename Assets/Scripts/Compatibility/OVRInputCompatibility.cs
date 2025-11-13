#if UNITY_2019_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Lightweight replacement for the deprecated Oculus Integration OVRInput API,
/// implemented on top of Unity's XR Input layer so existing gameplay code keeps working.
/// </summary>
public static class OVRInput
{
    public enum Controller
    {
        None,
        LTouch,
        RTouch,
        LTrackedRemote,
        RTrackedRemote
    }

    public enum Button
    {
        One,
        Two,
        Three,
        Four,
        Start
    }

    [Flags]
    public enum RawButton
    {
        None = 0,
        RThumbstick = 1 << 0,
        RThumbstickLeft = 1 << 1,
        RThumbstickRight = 1 << 2
    }

    public enum Axis1D
    {
        PrimaryIndexTrigger,
        PrimaryHandTrigger,
        SecondaryIndexTrigger,
        SecondaryHandTrigger
    }

    public enum Axis2D
    {
        PrimaryThumbstick,
        SecondaryThumbstick
    }

    private const float ThumbstickDirectionThreshold = 0.5f;

    private static readonly Button[] ButtonValues = (Button[])Enum.GetValues(typeof(Button));
    private static readonly RawButton[] RawButtonValues = (RawButton[])Enum.GetValues(typeof(RawButton));
    private static readonly Axis1D[] Axis1DEnumValues = (Axis1D[])Enum.GetValues(typeof(Axis1D));
    private static readonly Axis2D[] Axis2DEnumValues = (Axis2D[])Enum.GetValues(typeof(Axis2D));

    private static readonly Dictionary<Button, bool> CurrentButtonStates = new Dictionary<Button, bool>();
    private static readonly Dictionary<Button, bool> PreviousButtonStates = new Dictionary<Button, bool>();
    private static readonly Dictionary<RawButton, bool> CurrentRawButtonStates = new Dictionary<RawButton, bool>();
    private static readonly Dictionary<RawButton, bool> PreviousRawButtonStates = new Dictionary<RawButton, bool>();
    private static readonly Dictionary<Axis1D, float> Axis1DValues = new Dictionary<Axis1D, float>();
    private static readonly Dictionary<Axis2D, Vector2> Axis2DValues = new Dictionary<Axis2D, Vector2>();

    private static int lastUpdatedFrame = -1;

    static OVRInput()
    {
        foreach (Button button in ButtonValues)
        {
            CurrentButtonStates[button] = false;
            PreviousButtonStates[button] = false;
        }

        foreach (RawButton button in RawButtonValues)
        {
            if (button == RawButton.None)
            {
                continue;
            }

            CurrentRawButtonStates[button] = false;
            PreviousRawButtonStates[button] = false;
        }

        foreach (Axis1D axis in Axis1DEnumValues)
        {
            Axis1DValues[axis] = 0f;
        }

        foreach (Axis2D axis in Axis2DEnumValues)
        {
            Axis2DValues[axis] = Vector2.zero;
        }
    }

    public static bool GetDown(Button button)
    {
        UpdateState();
        return CurrentButtonStates[button] && !PreviousButtonStates[button];
    }

    public static bool Get(Button button)
    {
        UpdateState();
        return CurrentButtonStates[button];
    }

    public static bool GetUp(Button button)
    {
        UpdateState();
        return !CurrentButtonStates[button] && PreviousButtonStates[button];
    }

    public static bool GetDown(RawButton button)
    {
        UpdateState();
        return EvaluateRawButton(button, flag => CurrentRawButtonStates[flag] && !PreviousRawButtonStates[flag]);
    }

    public static bool Get(RawButton button)
    {
        UpdateState();
        return EvaluateRawButton(button, flag => CurrentRawButtonStates[flag]);
    }

    public static bool GetUp(RawButton button)
    {
        UpdateState();
        return EvaluateRawButton(button, flag => !CurrentRawButtonStates[flag] && PreviousRawButtonStates[flag]);
    }

    public static float Get(Axis1D axis)
    {
        UpdateState();
        return Axis1DValues[axis];
    }

    public static Vector2 Get(Axis2D axis)
    {
        UpdateState();
        return Axis2DValues[axis];
    }

    private static void UpdateState()
    {
        int targetFrame = Application.isPlaying ? Time.frameCount : -1;
        if (lastUpdatedFrame == targetFrame)
        {
            return;
        }

        lastUpdatedFrame = targetFrame;

        foreach (Button button in ButtonValues)
        {
            PreviousButtonStates[button] = CurrentButtonStates[button];
        }

        foreach (RawButton button in RawButtonValues)
        {
            if (button == RawButton.None)
            {
                continue;
            }

            PreviousRawButtonStates[button] = CurrentRawButtonStates[button];
        }

        SampleDevices();
    }

    private static void SampleDevices()
    {
        var leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        CurrentButtonStates[Button.One] = SampleButton(rightHand, CommonUsages.primaryButton);
        CurrentButtonStates[Button.Two] = SampleButton(rightHand, CommonUsages.secondaryButton);
        CurrentButtonStates[Button.Three] = SampleButton(leftHand, CommonUsages.primaryButton);
        CurrentButtonStates[Button.Four] = SampleButton(leftHand, CommonUsages.secondaryButton);
        CurrentButtonStates[Button.Start] = SampleButton(leftHand, CommonUsages.menuButton) || SampleButton(rightHand, CommonUsages.menuButton);

        Axis1DValues[Axis1D.PrimaryIndexTrigger] = SampleFloat(leftHand, CommonUsages.trigger);
        Axis1DValues[Axis1D.PrimaryHandTrigger] = SampleFloat(leftHand, CommonUsages.grip);
        Axis1DValues[Axis1D.SecondaryIndexTrigger] = SampleFloat(rightHand, CommonUsages.trigger);
        Axis1DValues[Axis1D.SecondaryHandTrigger] = SampleFloat(rightHand, CommonUsages.grip);

        Axis2DValues[Axis2D.PrimaryThumbstick] = SampleVector2(leftHand, CommonUsages.primary2DAxis);
        Axis2DValues[Axis2D.SecondaryThumbstick] = SampleVector2(rightHand, CommonUsages.primary2DAxis);

        bool rightThumbstickClick = SampleButton(rightHand, CommonUsages.primary2DAxisClick);
        Vector2 rightThumbstick = Axis2DValues[Axis2D.SecondaryThumbstick];

        CurrentRawButtonStates[RawButton.RThumbstick] = rightThumbstickClick;
        CurrentRawButtonStates[RawButton.RThumbstickLeft] = rightThumbstick.x < -ThumbstickDirectionThreshold;
        CurrentRawButtonStates[RawButton.RThumbstickRight] = rightThumbstick.x > ThumbstickDirectionThreshold;
    }

    private static bool EvaluateRawButton(RawButton button, Func<RawButton, bool> predicate)
    {
        if (button == RawButton.None)
        {
            return false;
        }

        bool result = false;
        foreach (RawButton flag in RawButtonValues)
        {
            if (flag == RawButton.None || !button.HasFlag(flag))
            {
                continue;
            }

            result |= predicate(flag);
        }

        return result;
    }

    private static bool SampleButton(InputDevice device, InputFeatureUsage<bool> usage)
    {
        return device.isValid && device.TryGetFeatureValue(usage, out bool value) && value;
    }

    private static float SampleFloat(InputDevice device, InputFeatureUsage<float> usage)
    {
        if (device.isValid && device.TryGetFeatureValue(usage, out float value))
        {
            return value;
        }

        return 0f;
    }

    private static Vector2 SampleVector2(InputDevice device, InputFeatureUsage<Vector2> usage)
    {
        if (device.isValid && device.TryGetFeatureValue(usage, out Vector2 value))
        {
            return value;
        }

        return Vector2.zero;
    }
}
#endif
