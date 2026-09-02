
using UnityEngine;

/// <summary>
/// This class is used in the trigger volume behind the players head to repeat a TTS instruction.
/// NOTE: as a debug features also toggles all accessibility features.
/// </summary>
public class RepeatTTSTriggerVolume : MonoBehaviour
{
    private PlayerHand handInTrigger;

    private void Start()
    {
#if UNITY_EDITOR
        EventManager.OnGripHeld.AddListener(this, ToggleAccessibilityFeatures);
#endif
        EventManager.OnTriggerPressed.AddListener(this, TryRepeatTTS);
    }

    private void TryRepeatTTS(bool isRightHand)
    {
        if (handInTrigger == null) return;

        if (handInTrigger.IsRightHand == isRightHand)
        {
            EventManager.OnRepeatTTSCalled.Raise(this, -1);
        }
    }



#if UNITY_EDITOR

    [ContextMenu("Toggle Accessibility Features")]
    public void ToggleAccessibilityContextMenu()
    {
        handInTrigger = Player.Instance.GetRightHand();
        ToggleAccessibilityFeatures(handInTrigger.IsRightHand);
        handInTrigger = null;
    }

    private void ToggleAccessibilityFeatures(bool isRightHand)
    {
        if (handInTrigger == null) return;

        if (handInTrigger.IsRightHand == isRightHand)
        {
            PlayerSettings.Accessibility.ToggleAll();
            EventManager.OnAccessibilitySettingsChanged.Raise(this, -1);
            TTSPlayer.PlayTTSWithFilePath("TTS/Menu/TTS_Menu_AccessibilityAllToggled");

        }
    }

#endif

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerHand>(out var hand)) handInTrigger = hand;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerHand>(out var hand) == handInTrigger) handInTrigger = null;
    }
}
