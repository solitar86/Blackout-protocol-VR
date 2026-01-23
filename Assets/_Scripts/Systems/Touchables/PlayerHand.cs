using System;
using UnityEngine;
using UnityEngine.XR;

public class PlayerHand : MonoBehaviour
{
    private VibrationPlayerDirect _hapticPlayer;
    private TouchRippleSpawner _touchRippleSpawner;

    private void Awake()
    {
        _hapticPlayer = GetComponent<VibrationPlayerDirect>();
        _touchRippleSpawner = GetComponent<TouchRippleSpawner>();
    }

    public void HandleTouchBegin(VibrationSettingsSO hapticSettings, Vector3 position)
    {
        PlayHapticFeedback(hapticSettings);
        SpawnTouchVisual(position);
    }
    public void HandleTouchEnd(VibrationSettingsSO hapticSettings)
    {
        PlayHapticFeedback(hapticSettings);
    }

    public void HandleTouchSlide(VibrationSettingsSO hapticSettings)
    {
        PlayHapticFeedback(hapticSettings);
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

        if(collider != null)
        {
            Gizmos.color = Color.forestGreen;
            Gizmos.DrawSphere(collider.transform.position, radius);
        }
    }


}
