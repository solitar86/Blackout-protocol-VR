using System;
using UnityEngine;

/// <summary>
/// This class is a centralized Debug.Log() caller so all logs can be disabled globally.
/// </summary>

[DefaultExecutionOrder(-9999)]
public static class Debugger
{
#if UNITY_EDITOR

    private static string PREFSKEY = "DebuggerEnabled";

    /// <summary>
    /// Set this to false to globally stop Debug.Log calls.
    /// </summary>
    public static bool isEnabled = true;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadDebuggerSettings()
    {
        int defaultValue = 1;
        isEnabled = PlayerPrefs.GetInt(PREFSKEY, defaultValue) == 1 ? true : false;
    }

#else
    public static bool isEnabled = false;
#endif

    public static void Logthis(this object obj)
    {
        if (isEnabled == false || obj == null) return;

        Log(obj.ToString() + " : " + obj.GetType().Name);
    }

    public static void Logthis(this object obj, TextColor color)
    {
        if (isEnabled == false || obj == null) return;

        var c = GetColorStringFromEnum(color);
        Log("<color=#" + c + ">" + obj.ToString() + " : </color>" + obj.GetType().Name);
    }

    public static void Log(string t)
    {
        if (isEnabled == false) return;

        Debug.Log(t);
    }

    public static void Log(object o)
    {
        if (isEnabled == false) return;

        Debug.Log(o);
    }

    public static void Log(object o, TextColor c)
    {
        if (isEnabled == false) return;

        var s = o.ToString();
        Log(s, c);
    }

    public static void Log(string t, TextColor color)
    {
        if (isEnabled == false) return;

        var c = GetColorStringFromEnum(color);
        Log("<color=#" + c + ">" + t + "</color>");
    }

    public static void Log(string t, GameObject go)
    {
        if (isEnabled == false) return;

        Debug.Log(t, go);
    }

    public static void Log(string t, TextColor color, GameObject go)
    {
        if (isEnabled == false) return;

        var c = GetColorStringFromEnum(color);
        var tc = "<color=#" + c + ">" + t + "</color>";
        Debug.Log(tc, go);
    }

    public static void LogError(string t, bool crash = false)
    {
        if (isEnabled == false) return;

        Debug.LogError(t);
        if (crash) Debug.Break();
    }
    public static void LogError(string t, GameObject g, bool crash = false)
    {
        if (isEnabled == false) return;

        Debug.LogError(t,g);
        if (crash) Debug.Break();
    }
    private static void LogError(string t, TextColor color, bool crash = false)
    {
        if (isEnabled == false) return;
        var c = GetColorStringFromEnum(color);
        var tc = "<color=#" + c + ">" + t + "</color>";
        Debug.LogError(tc);
        if(crash) Debug.Break();
    }
    public static void LogWarning(string t)
    {
        if (isEnabled == false) return;

        Debug.LogWarning(t);
    }
    public static void LogWarning(string t, TextColor color)
    {
        if (isEnabled == false) return;
        var c = GetColorStringFromEnum(color);
        var tc = "<color=#" + c + ">" + t + "</color>";
        Debug.LogWarning(tc);
    }
    public static void LogWarning(string t, GameObject go)
    {
        if (isEnabled == false) return;

        Debug.LogWarning(t, go);
    }
    public static void PlayBlipSound(string t = "")
    {
        AudioPlayer.PlayClipAtPoint("Debugger", Resources.Load<AudioClip>("Audio/SFX_Blip"), Vector3.zero, 0.3f, false, false);

        if (t != string.Empty) Log(t);
    }

    private static string GetColorStringFromEnum(TextColor color)
    {
        switch (color)
        {
            case TextColor.Red:
                return "ff0000";

            case TextColor.LightRed:
                return "FF6347";

            case TextColor.Green:
                return "00ff00";

            case TextColor.LightGreen:
                return "90EE90";

            case TextColor.Blue:
                return "0000ff";

            case TextColor.Purple:
                return "ff00ff";

            case TextColor.Yellow:
                return "FFFF00";

            case TextColor.LightBlue:
                return "00c3ff";

            case TextColor.Orange:
                return "FF7100";

            case TextColor.White:
                return "FFFFFF";
        }
        return "Invalid color";

    }

    public static void DisableLogs()
    {
        isEnabled = false;
#if UNITY_EDITOR
        PlayerPrefs.SetInt(PREFSKEY, 0);
#endif
    }
    
    public static void EnableLogs()
    {
        isEnabled = true;
#if UNITY_EDITOR
        PlayerPrefs.SetInt(PREFSKEY, 1);
#endif
    }

    public static void WorldSpaceText(string text, Vector3 spawnPoint)
    {
#if UNITY_EDITOR
        FloatingText.Create(spawnPoint, text, Color.white);
#endif
    }

    public static void Break()
    {
        Debug.Break();
    }

    public static void Break(string t = "")
    {
        LogError(t);
        Break();
    }
    public static void Break(string t = "", TextColor color = TextColor.White)
    {
        LogError(t, color);
        Break();
    }

    public enum TextColor
    {
        Red, LightRed, Green, LightGreen, Blue, LightBlue, Purple, Yellow, Orange, White
    }
}