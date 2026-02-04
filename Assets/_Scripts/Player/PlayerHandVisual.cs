using System;
using UnityEngine;

public class PlayerHandVisual : MonoBehaviour
{
    [SerializeField] Transform _followTarget;
    private Rigidbody _rigidbody;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        EventManager.OnAccessibilitySettingsChanged.AddListener(this, AccessibilitySettingsChanged);
        AccessibilitySettingsChanged(-1);
    }

    private void AccessibilitySettingsChanged(int i)
    {
        SnapToFollowPosition();
        gameObject.SetActive(PlayerSettings.Accessibility.Hands);
    }

    void FixedUpdate()
    {

        if (PlayerInputHandler.PlayerIsMoving == true)
        {
            SnapToFollowPosition();
        }
        else
        {
            FollowWithForces();
        }
    }

    void SnapToFollowPosition()
    {
        if (_rigidbody.isKinematic == false) _rigidbody.isKinematic = true;

        _rigidbody.MovePosition(_followTarget.position);
        _rigidbody.MoveRotation(_followTarget.rotation);
    }

    void FollowWithForces()
    {
        if (_rigidbody.isKinematic == true) _rigidbody.isKinematic = false;

        _rigidbody.linearVelocity = (_followTarget.position - _rigidbody.position) / Time.fixedDeltaTime;
        Quaternion rotationDifference = _followTarget.rotation * Quaternion.Inverse(_rigidbody.rotation);
        rotationDifference.ToAngleAxis(out float angle, out Vector3 axis);
        _rigidbody.angularVelocity = (axis * angle * Mathf.Deg2Rad) / Time.fixedDeltaTime;
    }

    private void OnDestroy()
    {
        EventManager.OnAccessibilitySettingsChanged.RemoveListener(this, AccessibilitySettingsChanged);
    }
}
