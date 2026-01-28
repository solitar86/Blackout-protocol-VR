using UnityEngine;

public class ToggleOnAccessibilityChanged : MonoBehaviour
{
    private void Start()
    {
        EventManager.OnAccessibilitySettingsChanged.AddListener(this, AccessibilitySettingsChanged);
        AccessibilitySettingsChanged(PlayerSettings.Accessibility.Enabled);
    }

    private void AccessibilitySettingsChanged(bool enabled)
    {
        gameObject.SetActive(enabled);
    }

    private void OnDestroy()
    {
        EventManager.OnAccessibilitySettingsChanged.RemoveListener(this, AccessibilitySettingsChanged);
    }
}
