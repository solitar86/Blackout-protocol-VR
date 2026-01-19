using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.XR;

public class VibrationPlayerDirect : MonoBehaviour, ITouchAudio
{
    [SerializeField] XRNode _handedness;
    private InputDevice _controller;
    private HapticCapabilities _hapticCapabities;

    [SerializeField] VibrationSettingsSO _handInsideVibration;
    [SerializeField] VibrationSettingsSO _touchingEdgeVibration;

    private float _intervalTimer = 0f;

    private Collider _touchedObjectCollider;




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
            Debug.Log("Device found for " + _handedness.ToString());
            _controller = XRdevices[0];
            _controller.TryGetHapticCapabilities(out _hapticCapabities);
            return true;
        }
        else
        {
            Debug.Log("Device not found for " + _handedness.ToString());
            return false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _touchedObjectCollider = other.gameObject.GetComponent<Collider>();
    }
    private void OnTriggerExit(Collider other)
    {
        _touchedObjectCollider = _touchedObjectCollider = other.gameObject.GetComponent<Collider>() ? null : _touchedObjectCollider;
    }

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


}
