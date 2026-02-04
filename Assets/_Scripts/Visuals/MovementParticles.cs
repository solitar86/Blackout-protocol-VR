using UnityEngine;

public class MovementParticles : MonoBehaviour
{
    private void Start()
    {
        EventManager.OnAccessibilitySettingsChanged.AddListener(this, AccessibilitySettingsChanged);
        AccessibilitySettingsChanged(-1);
    }

    private void AccessibilitySettingsChanged(int i)
    {
        gameObject.SetActive(PlayerSettings.Accessibility.Particles);
    }
    private void OnDestroy()
    {
        EventManager.OnAccessibilitySettingsChanged.RemoveListener(this, AccessibilitySettingsChanged);
    }
}
