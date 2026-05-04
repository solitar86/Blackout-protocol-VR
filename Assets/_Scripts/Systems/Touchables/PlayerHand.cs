using System;
using UnityEngine;
using UnityEngine.XR;

public class PlayerHand : MonoBehaviour
{
    private VibrationPlayerDirect _hapticPlayer;
    private TouchRippleSpawner _touchRippleSpawner;
    private SphereCollider _sphereCollider;
    private Vector3 _handVelocity;
    private bool _isRightHand;
    private PickUpObject _pickUpObject;
    public bool IsHoldingObject => _pickUpObject != null;
    public bool IsRightHand => _isRightHand;

    private float nextTimeAllowHandInsideInteractorHapticToPlay = 0f;

    #region UnityCallbacks
    private void Awake()
    {
        _hapticPlayer = GetComponent<VibrationPlayerDirect>();
        _touchRippleSpawner = GetComponent<TouchRippleSpawner>();
        _isRightHand = GetHandXRNode() == XRNode.RightHand ? true : false;
    }
    #endregion

    #region Touch And Interactions Functionality
    public void HandleTouchBegin(VibrationSettingsSO hapticSettings, Vector3 touchRipplePos)
    {
        PlayHapticFeedback(hapticSettings);
        SpawnTouchVisual(touchRipplePos);
    }
    public void HandleTouchEnd(VibrationSettingsSO hapticSettings)
    {
        PlayHapticFeedback(hapticSettings);
    }
    public void HandleTouchSlide(VibrationSettingsSO hapticSettings)
    {
        PlayHapticFeedback(hapticSettings);
    }
    public void HandleHandInsideInteractable(VibrationSettingsSO hapticSettings)
    {
        if(nextTimeAllowHandInsideInteractorHapticToPlay < Time.time)
        {
            PlayHapticFeedback(hapticSettings);
            nextTimeAllowHandInsideInteractorHapticToPlay = 
                    Time.time + ((hapticSettings.TimeInterval + hapticSettings.Duration)
                    * hapticSettings.RepeatTimes);
        }
    }
    public void HandlePickUpOrDropObject(VibrationSettingsSO hapticSettings, PickUpObject @object)
    {
        _pickUpObject = @object;
        PlayHapticFeedback(hapticSettings);
    }

    #endregion

    #region Haptic feedback functions
    /// <summary>
    /// This overload plays a default haptic feedback set in 
    /// Vibration Player without picking up anything.
    /// </summary>
    public void HandlePickUpOrDropObject()
    {
        PlayDefaultHapticFeedback();
    }
    public void HandleHandInsideCollider(VibrationSettingsSO hapticSettings)
    {
        PlayHapticFeedback(hapticSettings);
    }
    public void HandleSingleVibration(VibrationSettingsSO hapticSettings)
    {
        PlayHapticFeedback(hapticSettings);
    }
    private void PlayHapticFeedback(VibrationSettingsSO hapticSettings)
    {
        if (hapticSettings.RepeatTimes == 1)
        {
            _hapticPlayer?.PlayHaptic(hapticSettings);
            return;
        }


        float totalDelay = 0f;
        for (int i = 0; i < hapticSettings.RepeatTimes; i++)
        {
            var delayObject = new GameObject(hapticSettings.name);
            var mono = delayObject.AddComponent<Delay>();

            mono.CallWithDelay(() =>
            {
                _hapticPlayer?.PlayHaptic(hapticSettings);
            }, totalDelay);

            float interval = hapticSettings.TimeInterval;
            totalDelay += hapticSettings.Duration + interval;
            Destroy(mono.gameObject, totalDelay);
        }

    }
    /// <summary>
    /// This overload plays a default haptic feedback set in Vibration Player.
    /// </summary>
    /// 
    private void PlayDefaultHapticFeedback()
    {
        _hapticPlayer?.PlayDefaultHaptic();
    }

    #endregion
    private void SpawnTouchVisual(Vector3 position)
    {
        _touchRippleSpawner?.SpawnTouchVisual(position);
    }

    #region Helpers, organization etc.
    public XRNode GetHandXRNode() => _hapticPlayer.GetXRNode();
    private void OnDrawGizmosSelected()
    {
        var collider = GetComponent<SphereCollider>();
        var radius = collider.radius;

        if (collider != null)
        {
            Gizmos.color = Color.green;
            Vector3 worldCenter = collider.transform.TransformPoint(collider.center);
            Gizmos.DrawSphere(worldCenter, collider.radius);
        }
    }
    public float GetColliderRadius()
    {
        if (_sphereCollider == null) _sphereCollider = GetComponent<SphereCollider>();
        return _sphereCollider.radius;
    }
    public float GetCurrentVelocity()
    {
        var controller = InputDevices.GetDeviceAtXRNode(GetHandXRNode());
        if(controller.TryGetFeatureValue(CommonUsages.deviceVelocity, out var velocity))
        {
            //Debugger.WorldSpaceText(velocity.magnitude.ToString("F2"), transform.position);
            return velocity.magnitude;
        }
        Debugger.LogWarning("Could not get hand velocity. Returning 0");
        return 0f;
    }

    #endregion
}
