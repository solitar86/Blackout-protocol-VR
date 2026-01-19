using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class Developer
{

    [MenuItem("Developer/Deleter All Player Prefs")]
    public static void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debugger.Log("Player Prefs have been cleared", Debugger.TextColor.LightGreen);
    }

    [MenuItem("Developer/Load Settings")]
    public static void LoadSettings()
    {
        PlayerSettings.LoadSettings();
        Debugger.Log("Player Settings Loaded", Debugger.TextColor.LightGreen);
    }

    [MenuItem("Developer/Save Settings")]
    public static void SaveSettings()
    {
        PlayerSettings.SaveSettings();
        Debugger.Log("Player Settings Saved", Debugger.TextColor.LightGreen);
    }



}

#endif
