using System;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class TTSPlayer : MonoBehaviour
{
    public static void PlayNumber(int number)
    {
        string numberString = GetStringFromNumber(number);
        var clip = Resources.Load<AudioClip>("TTS/Numbers/TTS_Numbers_" + numberString);

        if (clip == null)
        {
            PlayTTSFileNotFoundError();
            return;
        }
        var source = FindFirstObjectByType<TTS_SpeedControl>().TTSSource;
        source.clip = clip;
        source.Play();
    }

    public static void PlayTTSFileNotFoundError()
    {
        Debugger.Log("TTS FILE NOT FOUND", Debugger.TextColor.Red);
        var clip = Resources.Load<AudioClip>("TTS/TTS_Error_TTSFileNotFound");
        var source = FindFirstObjectByType<TTS_SpeedControl>().TTSSource;

        if (clip == null)
        {
            Debugger.Log("Error Clip Not Found", Debugger.TextColor.Red);
        }
        source.clip = clip;
        source.Play();
    }

    private static string GetStringFromNumber(int number)
    {
        if (number > 10)
        {
            PlayTTSFileNotFoundError();
            return string.Empty;
        }

        switch (number)
        {
            case 0: return "Zero";
            case 1: return "One";
            case 2: return "Two";
            case 3: return "Three";
            case 4: return "Four";
            case 5: return "Five";
            case 6: return "Six";
            case 7: return "Seven";
            case 8: return "Eight";
            case 9: return "Nine";
            case 10: return "Ten";
            default:
                return string.Empty;
        }
    }
}
