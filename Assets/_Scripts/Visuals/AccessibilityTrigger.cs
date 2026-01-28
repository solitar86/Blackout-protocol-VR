using System;
using UnityEngine;

public class AccessibilityTrigger : MonoBehaviour
{
    private PlayerHand handInTrigger;

    private void Start()
    {
        EventManager.OnTriggerPressed.AddListener(this, ToggleAccessibilityFeatures);
    }

    private void ToggleAccessibilityFeatures(bool isRightHand)
    {
        if (handInTrigger == null) return;

        if(handInTrigger.IsRightHand == isRightHand)
        {
            PlayerSettings.Accessibility.ToggleAll();
            EventManager.OnAccessibilitySettingsChanged.Raise(this, PlayerSettings.Accessibility.Enabled);

        }
        Debugger.Log(handInTrigger.IsRightHand + " : " + isRightHand);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<PlayerHand>(out var hand)) handInTrigger = hand;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerHand>(out var hand) == handInTrigger) handInTrigger = null;
    }
}
