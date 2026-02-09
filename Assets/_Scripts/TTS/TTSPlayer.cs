using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class plays TTS through a single
/// audiosource. This does not use the
/// Audioplayer class - by design.
/// This is because the pitch of TTS
/// is used to controls the speed
/// and the pitchshift is counter-acted
/// in the mixer with a pitch lowering effect.
/// </summary>

[RequireComponent(typeof(AudioSource))]
public class TTSPlayer : MonoBehaviour
{
    // TODO: Use this list to unload TTS files at appropriate times
    private static List<AudioClip> _currentlyLoadedClips = new List<AudioClip>();

    private const string TTSNUMBERSPATH = "TTS/Numbers/TTS_Numbers_";
    private static float _nextTimeAllowTTS = 0f; // THIS VALUE NEEDS TO BE RESET on ENTER PLAYMODE! TODO!
    private static void PlayTTS(AudioClip clipToPlay, string debugInfo, bool preventInterrupt = false)
    {
        if (Time.time < _nextTimeAllowTTS)
        {
            Debugger.Log(debugInfo + " TTS was blocked by 'prevent interrupt'", Debugger.TextColor.LightRed);
            return;
        }
        if (clipToPlay == null)
        {
            PlayTTSFileNotFoundError();
            Debugger.LogWarning("No TTS file found with : " + debugInfo);
            return;
        }
        // TODO: Make this with a permanent reference.
        var source = FindFirstObjectByType<TTS_SpeedControl>().TTSSource;
        source.clip = clipToPlay;
        source.Play();

        TryAddClipToLoadedAssetsList(clipToPlay);

        if (preventInterrupt) _nextTimeAllowTTS = Time.time + source.clip.length / PlayerSettings.Audio.TTS_Speed;

        EventManager.OnTTSPlay.Raise("TTS Player", debugInfo);
    }
    public static void PlayTTSSequenceWithPaths(bool preventInterrupt = false, params string[] paths)
    {
        //TODO: When quickly opening menus the title of the new menu
        // Is interrupted. Perhaps make a way so that sequences will
        // always play consecutively even if there's a delay?

        List<AudioClip> clips = new();
        foreach (var path in paths)
        {
            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip == null)Debugger.Log("TTS Sequence clip not found with path: " + path);
            else clips.Add(clip);
        }

        float totalDelay = 0f;

        foreach (var clip in clips)
        {
            var delayObject = new GameObject(clip.name);
            var mono = delayObject.AddComponent<Delay>();

            mono.CallWithDelay(() =>
            {
                PlayTTS(clip, "TTS Sequence :" + clip.name, preventInterrupt);
            }, totalDelay);

            float buffer = 0.01f; // Currently this is necessary to prevent clips from
                                // interrupting each other. TODO: FIX
            totalDelay += clip.length / PlayerSettings.Audio.TTS_Speed + buffer;
            Destroy(mono.gameObject, totalDelay);
        }
    }
    public static void PlayTTSWithFilePath(string path, bool preventInterrupt = false)
    {
        var clip = Resources.Load<AudioClip>(path);
        PlayTTS(clip, path, preventInterrupt);
    }
    public static void PlayNumber(int number)
    {
        string numberString = GetStringFromNumber(number);
        var clip = Resources.Load<AudioClip>(TTSNUMBERSPATH + numberString);
        PlayTTS(clip, "Number: " + number);
    }
    private static void PlayTTSFileNotFoundError()
    {
        var clip = Resources.Load<AudioClip>("TTS/TTS_Error_TTSFileNotFound");
        PlayTTS(clip, "Error Clip");
    }
    private static void TryAddClipToLoadedAssetsList(AudioClip clipToPlay)
    {
        if (_currentlyLoadedClips.Contains(clipToPlay)) return;
        _currentlyLoadedClips.Add(clipToPlay);
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
    public static string GetTTSNumberFilePath(int number)
    {
        return TTSNUMBERSPATH + GetStringFromNumber(number);
    }
    public static void ResetStaticVariables()
    {
        _nextTimeAllowTTS = 0f;
    }
}
