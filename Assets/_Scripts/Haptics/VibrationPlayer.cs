
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

[RequireComponent(typeof(Collider)), RequireComponent(typeof(HapticImpulsePlayer))]
public class VibrationPlayer : MonoBehaviour
{
    [SerializeField] float _interval = 1;
    [SerializeField, Range(0f,1f)] float _amplitude = 1;
    [SerializeField, Range(0.001f, 5f)] float _duration = 1;
    [SerializeField] float _frequency = 1;
    [Space(15)]
    [SerializeField] private HapticImpulsePlayer _impulsePlayer;

    //float _intervalTimer = 0;



    private void OnTriggerStay(Collider other)
    {
        _impulsePlayer.SendHapticImpulse(_amplitude, _duration, _frequency);
    }

    private void OnValidate()
    {
        if (_interval > _duration)
        {
            Debug.Log("<color=#6CE322> Interval must be less than duration</color>");
            _interval = _duration - 0.001f;
        }

        if (_duration < 0)
        {
            _duration = 0;
        }

        if (_interval < 0)
        {
            _interval = 0;
        }
    }


}
