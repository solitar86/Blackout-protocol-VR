using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class LoadGameSettings : MonoBehaviour
{
    private void Awake()
    {
        Debugger.Log("Loading player preferences", Debugger.TextColor.LightGreen);
        PlayerSettings.LoadSettings();
    }
}
