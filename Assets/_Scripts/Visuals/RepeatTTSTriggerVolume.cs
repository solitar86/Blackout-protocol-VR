
using UnityEngine;

public class RepeatTTSTriggerVolume : MonoBehaviour
{
    private PlayerHand handInTrigger;

    private void Start()
    {
#if UNITY_EDITOR
        EventManager.OnGripPressed.AddListener(this, ToggleAccessibilityFeatures);
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
    private void ToggleAccessibilityFeatures(bool isRightHand)
    {
        if (handInTrigger == null) return;

        if (handInTrigger.IsRightHand == isRightHand)
        {
            PlayerSettings.Accessibility.ToggleAll();
            EventManager.OnAccessibilitySettingsChanged.Raise(this, -1);

        }
        Debugger.Log(handInTrigger.IsRightHand + " : " + isRightHand);
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
