using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class CameraFollowSmooth : MonoBehaviour
{
    [Header("Camera Smoothing Settings")]
    [SerializeField] private Transform _followTarget;
    [SerializeField] private float positionSmoothTime = 0.05f;
    [SerializeField] private float positionDeadZone = 0.002f;
    [SerializeField] private float rotationSmoothTime = 0.05f;
    [SerializeField] private float rotationDeadZone = 0.2f;
    [SerializeField] private Vector3 _rotationOffset = Vector3.zero;


    private Vector3 positionVelocity;
    private Quaternion smoothedRotation;
    Camera recorderCamera;

    private void Awake()
    {
        recorderCamera = GetComponent<Camera>();

        if (_followTarget == null)
        {
            Debugger.Log($"{nameof(CameraFollowSmooth)}: No target assigned. Defaulting to Main Camera", gameObject);
            _followTarget = Camera.main.transform;
        }

        transform.position = _followTarget.position;
        transform.rotation = _followTarget.rotation;
        smoothedRotation = transform.rotation;
        

        DontDestroyOnLoad(gameObject);
    }

    private void LateUpdate()
    {
        SmoothPosition();
        SmoothRotation();
    }

    private void SmoothPosition()
    {
        if (_followTarget == null)
        {
            Debug.Log($"{nameof(CameraFollowSmooth)}: No target assigned. Defaulting to Main Camera", this);
            _followTarget = Camera.main.transform;
        }

        Vector3 delta = _followTarget.position - transform.position;

        if (delta.sqrMagnitude < positionDeadZone * positionDeadZone)
            return;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            _followTarget.position,
            ref positionVelocity,
            positionSmoothTime
        );
    }

    private void SmoothRotation()
    {
        if (_followTarget == null)
        {
            Debug.Log($"{nameof(CameraFollowSmooth)}: No target assigned. Defaulting to Main Camera", this);
            _followTarget = Camera.main.transform;
        }

        float angle = Quaternion.Angle(smoothedRotation, _followTarget.rotation);

        if (angle < rotationDeadZone)
            return;

        smoothedRotation = SmoothDampQuaternion(
            smoothedRotation,
            _followTarget.rotation,
            rotationSmoothTime,
            _rotationOffset
        );

        transform.rotation = smoothedRotation;
    }

    private Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, float smoothTime, Vector3 rotationOffset = default)
    {
        var offset = Quaternion.Euler(rotationOffset);

        if (Time.deltaTime < Mathf.Epsilon)
            return current;

        float t = 1f - Mathf.Exp(-Time.deltaTime / smoothTime);
        return Quaternion.Slerp(current, target * offset, t);
    }
}

