using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VibrationPlayerDirect : MonoBehaviour
{
    [SerializeField] XRNode _handedness;
    private InputDevice _controller;
    private HapticCapabilities _hapticCapabities;

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
        if (_controller == null)
        {
            if (TryInitializeDevice() == false) return;
        }
        if (_controller.isValid == false)
        {
            if (TryInitializeDevice() == false) return;
        }

        _controller.SendHapticImpulse(0, vibrationSettings._amplitude, vibrationSettings._duration);
    }

    /*
    private void OnTriggerStay(Collider other)
    {
        if (_controller == null)
        {
            if (TryInitializeDevice() == false) return;
        }
        if(_controller.isValid == false)
        {
            if (TryInitializeDevice() == false) return;
        }

        if(_hapticCapabities.supportsImpulse == true)
        {
            if (_touchedObjectCollider != null)
            {
                Vector3 closestPoint = _touchedObjectCollider.ClosestPointOnBounds(transform.position);
                float distance = Vector3.Distance(transform.position, closestPoint);
                if (distance > 0.001f)
                {
                    _intervalTimer += Time.deltaTime;
                    CountUpToIntervalAndHandlePulse(_touchingEdgeVibration);
                }
                else
                {
                    CountUpToIntervalAndHandlePulse(_handInsideVibration);
                }
            }
        }

    }

    private void CountUpToIntervalAndHandlePulse(VibrationSettingsSO vibrationSettings)
    {
        if (_intervalTimer > vibrationSettings._interval)
        {
            _controller.SendHapticImpulse(0, vibrationSettings._amplitude, vibrationSettings._duration);
            _intervalTimer -= vibrationSettings._interval;
        }
    }

    */

}
