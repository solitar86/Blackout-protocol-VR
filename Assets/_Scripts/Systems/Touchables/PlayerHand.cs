using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private VibrationPlayerDirect _hapticPlayer;
    private TouchRippleSpawner _touchRippleSpawner;

    private void Awake()
    {
        _hapticPlayer = GetComponent<VibrationPlayerDirect>();
        _touchRippleSpawner = GetComponent<TouchRippleSpawner>();
    }

    public void PlayHapticFeedback(VibrationSettingsSO hapticSettings)
    {
        _hapticPlayer?.PlayHaptic(hapticSettings);
    }

    public void SpawnTouchVisual(Vector3 position)
    {
        _touchRippleSpawner?.SpawnTouchVisual(position);
    }

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
