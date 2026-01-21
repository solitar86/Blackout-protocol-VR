using UnityEngine;

/// <summary>
/// This class is a centralized Debug.Log() caller so all logs can be disabled globally.
/// </summary>
public static class Debugger
{
#if UNITY_EDITOR
    /// <summary>
    /// Set this to false to globally stop Debug.Logs();
    /// </summary>
    private static bool _isEnabled = true;
    public static bool isEnabled => _isEnabled;
#else
    /// <inheritdoc cref="isEnabled"/>
    public static bool isEnabled = false;
#endif


    public static void Logthis(this object obj)
    {
        if (_isEnabled == false || obj == null) return;

        Log(obj.ToString() + " : " + obj.GetType().Name);
    }

    public static void Logthis(this object obj, TextColor color)
    {
        if (_isEnabled == false || obj == null) return;

        var c = GetColorStringFromEnum(color);
        Log("<color=#" + c + ">" + obj.ToString() + " : </color>" + obj.GetType().Name);
    }

    public static void Log(string t)
    {
        if (_isEnabled == false) return;

        Debug.Log(t);
    }

    public static void Log(object o, TextColor c)
    {
        if (_isEnabled == false) return;

        var s = o.ToString();
        Log(s, c);
    }

    public static void Log(string t, TextColor color)
    {
        if (_isEnabled == false) return;

        var c = GetColorStringFromEnum(color);
        Log("<color=#" + c + ">" + t + "</color>");
    }


    public static void Log(string t, GameObject go)
    {
        if (_isEnabled == false) return;

        Debug.Log(t, go);
    }

    public static void Log(string t, TextColor color, GameObject go)
    {
        if (_isEnabled == false) return;

        var c = GetColorStringFromEnum(color);
        var tc = "<color=#" + c + ">" + t + "</color>";
        Debug.Log(tc, go);
    }

    public static void LogError(string t)
    {
        if (_isEnabled == false) return;

        Debug.LogError(t);
    }

    public static void LogWarning(string t)
    {
        if (_isEnabled == false) return;

        Debug.LogWarning(t);
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
        }
        return "Invalid color";

    }

    public static void DisableLogs() => _isEnabled = false;
    public static void EnableLogs() => _isEnabled = true;
    public enum TextColor
    {
        Red, LightRed, Green, LightGreen, Blue, LightBlue, Purple, Yellow
    }
}