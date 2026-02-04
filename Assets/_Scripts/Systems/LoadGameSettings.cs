using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class LoadGameSettings : MonoBehaviour
{
    private void Awake()
    {
        Debugger.Log("Loading player preferences", Debugger.TextColor.LightGreen);
        PlayerSettings.LoadSettings();

        //Raise settings events so systems can react.
        EventManager.OnAccessibilitySettingsChanged.Raise(this, -1); // remove the "true" from here.
    }
}
