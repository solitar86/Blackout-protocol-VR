using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Hands;

public class VibrationPlayerDirect : MonoBehaviour
{
    [SerializeField] XRNode _handedness;
    private InputDevice _controller;
    private HapticCapabilities _hapticCapabities;

    float _nextTimeCanPlayHaptics = 0f;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Start()
    {
        TryInitializeDevice();
    }

    private bool TryInitializeDevice()
    {
        List<InputDevice> XRdevices = new();
        InputDevices.GetDevicesAtXRNode(_handedness, XRdevices);
        if (XRdevices.Count > 0)
        {
            Debugger.Log("Device found for " + _handedness.ToString(), Debugger.TextColor.Green);
            _controller = XRdevices[0];
            _controller.TryGetHapticCapabilities(out _hapticCapabities);
            return true;
        }
        else
        {
            Debugger.Log("Device not found for " + _handedness.ToString(), Debugger.TextColor.Red);
            return false;
        }
    }

    public void PlayHaptic(VibrationSettingsSO vibrationSettings)
    {
        if (PreviousHapticDurationHasElapsed() == false) return;

        if (_controller == null)
        {
            if (TryInitializeDevice() == false) return;
        }
        if (_controller.isValid == false)
        {
            if (TryInitializeDevice() == false) return;
        }
        _controller.SendHapticImpulse(0, vibrationSettings.Amplitude, vibrationSettings.Duration);
        _nextTimeCanPlayHaptics = Time.time + vibrationSettings.Duration;
    }
    /// <summary>
    /// Playes default haptic feedback with amplitude = 1 and duration = 0.2f;
    /// </summary>
    public void PlayDefaultHaptic()
    {
        string haptic = "HAPTIC 2";
        haptic.Logthis();
        if (PreviousHapticDurationHasElapsed() == false) return;

        if (_controller == null)
        {
            if (TryInitializeDevice() == false) return;
        }
        if (_controller.isValid == false)
        {
            if (TryInitializeDevice() == false) return;
        }

        var amplitude = 1;
        var duration = 0.2f;
        _controller.SendHapticImpulse(0, amplitude, duration);
        _nextTimeCanPlayHaptics = Time.time + duration;
    }

    private bool PreviousHapticDurationHasElapsed()
    {
        return Time.time > _nextTimeCanPlayHaptics;
    }

    public XRNode GetXRNode() => _handedness;
}
