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
}
