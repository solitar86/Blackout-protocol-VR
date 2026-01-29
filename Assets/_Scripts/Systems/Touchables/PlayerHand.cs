using System;
using UnityEngine;
using UnityEngine.XR;

public class PlayerHand : MonoBehaviour
{
    private VibrationPlayerDirect _hapticPlayer;
    private TouchRippleSpawner _touchRippleSpawner;
    private Vector3 _handVelocity;
    private bool _isRightHand;
    public bool IsRightHand => _isRightHand;

    #region UnityCallbacks
    private void Awake()
    {
        _hapticPlayer = GetComponent<VibrationPlayerDirect>();
        _touchRippleSpawner = GetComponent<TouchRippleSpawner>();
        _isRightHand = GetHandXRNode() == XRNode.RightHand ? true : false;
    }
    #endregion
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
    public void HandlePickUpOrDropObject(VibrationSettingsSO hapticSettings, int numToRepeat = 1)
    {
        float interval = 0.05f;
        for (int i = 0; i < numToRepeat; i++)
        {
            this.CallWithDelay(() => PlayHapticFeedback(hapticSettings), i * (interval + hapticSettings.Duration));
        }
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
        _hapticPlayer?.PlayHaptic(hapticSettings);
    }
    private void SpawnTouchVisual(Vector3 position)
    {
        _touchRippleSpawner?.SpawnTouchVisual(position);
    }
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



}
