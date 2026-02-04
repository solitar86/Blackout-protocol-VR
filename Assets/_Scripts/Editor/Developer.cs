using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class Developer
{

    [MenuItem("Developer/Settings/Deleter All Player Prefs")]
    public static void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debugger.Log("Player Prefs have been cleared", Debugger.TextColor.LightGreen);
    }

    [MenuItem("Developer/Settings/Load Settings")]
    public static void LoadSettings()
    {
        PlayerSettings.LoadSettings();
        Debugger.Log("Player Settings Loaded", Debugger.TextColor.LightGreen);
    }

    [MenuItem("Developer/Settings/Save Settings")]
    public static void SaveSettings()
    {
        PlayerSettings.SaveSettings();
        Debugger.Log("Player Settings Saved", Debugger.TextColor.LightGreen);
    }

    [MenuItem("Developer/Settings/TTS Faster")]
    public static void IncreaseTTSSpeed()
    {
        PlayerSettings.Audio.IncreaseTTS_Speed();
    }

    [MenuItem("Developer/Settings/TTS Slower")]
    public static void LowerTTSSpeed()
    {
        PlayerSettings.Audio.DecreaseTTS_Speed();
    }

    [MenuItem("Developer/Settings/TTS Volume UP")]
    public static void IncreaseTTSVolume()
    {
        PlayerSettings.Audio.IncreaseTTS_Volume();
    }

    [MenuItem("Developer/Settings/TTS Volume Down")]
    public static void LowerTTSVolume()
    {
        PlayerSettings.Audio.LowerTTS_Volume();
    }

    [MenuItem("Developer/Logging/Disable Logs")]
    public static void DisableLogs()
    {
        Debugger.DisableLogs();
        Debug.Log("Debugger logs disabled");
    }

    [MenuItem("Developer/Logging/Enable Logs")]
    public static void EnableLogs()
    {
        Debugger.EnableLogs();
        Debug.Log("Debugger logs enabled");
    }

    [MenuItem("Developer/Logging/Disable Event logs")]
    public static void DisableEventLogs()
    {
        EventManager.DisableEventLogs();
        Debug.Log("Event logs disabled");
    }

    [MenuItem("Developer/Logging/Enable Event Logs")]
    public static void EnableEventLogs()
    {
        EventManager.EnableEventLogs();
        Debug.Log("Event logs enabled");

        if (Debugger.isEnabled == false) Debug.Log("Debugger class is disabled so now logs will be shown");
    }



}

#endif
