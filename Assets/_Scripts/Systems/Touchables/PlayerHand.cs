using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    private VibrationPlayerDirect _hapticPlayer;

    private void Awake()
    {
        _hapticPlayer = GetComponent<VibrationPlayerDirect>();
    }

    public void PlayHapticFeedback(VibrationSettingsSO hapticSettings)
    {
        _hapticPlayer.PlayHaptic(hapticSettings);
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
