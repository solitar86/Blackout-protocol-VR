using UnityEngine;

public class ToggleOnAccessibilityChanged : MonoBehaviour
{
    private void Start()
    {
        EventManager.OnAccessibilitySettingsChanged.AddListener(this, AccessibilitySettingsChanged);
        AccessibilitySettingsChanged(-1);
    }

    private void AccessibilitySettingsChanged(int i)
    {
        gameObject.SetActive(PlayerSettings.Accessibility.DebugLight);
    }

    private void OnDestroy()
    {
        EventManager.OnAccessibilitySettingsChanged.RemoveListener(this, AccessibilitySettingsChanged);
    }
}
