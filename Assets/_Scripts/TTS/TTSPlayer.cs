using System;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class TTSPlayer : MonoBehaviour
{
    private static void PlayTTS(AudioClip clipToPlay, string debugInfo)
    {
        if (clipToPlay == null)
        {
            PlayTTSFileNotFoundError();
            Debugger.LogWarning("No TTS file found with : " + debugInfo);
            return;
        }
        var source = FindFirstObjectByType<TTS_SpeedControl>().TTSSource;
        source.clip = clipToPlay;
        source.Play();
    }

    public static void PlayTTSWithFilePath(string path)
    {
        var clip = Resources.Load<AudioClip>(path);
        PlayTTS(clip, path);
    }

    public static void PlayNumber(int number)
    {
        string numberString = GetStringFromNumber(number);
        var clip = Resources.Load<AudioClip>("TTS/Numbers/TTS_Numbers_" + numberString);
        PlayTTS(clip, "Number: " + number);
    }


    public static void PlayTTSFileNotFoundError()
    {
        var clip = Resources.Load<AudioClip>("TTS/TTS_Error_TTSFileNotFound");
        PlayTTS(clip, "Error Clip");
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
