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
    #region Fields
    private static string TTS_ERROR_NOFILE_FILEPATH = "TTS/TTS_Error_TTSFileNotFound";
    private static string TTS_ERROR_NOREPEAT_FILEPATH = "TTS/TTS_Error_NothingToRepeat";
    public static bool TTSIsPlaying
    {
        get
        {
            return Time.time < _nextTimeAllowTTS;
        }
    }
    private static AudioClip _TTSToRepeat;

    private static List<AudioClip> _currentlyLoadedClips = new List<AudioClip>();
    private static List<GameObject> _queuedTTSSequenceGameObjects = new List<GameObject>();
    private static AudioSource _ttsSource;
    private const string TTSNUMBERSPATH = "TTS/Numbers/TTS_Numbers_";
    private static float _nextTimeAllowTTS = 0f;
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        EventManager.OnRadialMenuClose.AddListener("TTSPlayer", UnloadUsedTTSClips);
        EventManager.OnRepeatTTSCalled.AddListener("TTSPlayer", RepeatTTS);
    }
    private void OnDisable()
    {
        EventManager.OnRadialMenuClose.RemoveListener("TTSPlayer", UnloadUsedTTSClips);
        EventManager.OnRepeatTTSCalled.RemoveListener("TTSPlayer", RepeatTTS);
    }
    #endregion

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
        if (_ttsSource == null) _ttsSource = FindFirstObjectByType<TTS_SpeedControl>().TTSSource;
        _ttsSource.clip = clipToPlay;
        _ttsSource.loop = false; // This resets us if we are playing a TTS file on loop before.
        _ttsSource.Play();

        TryAddClipToLoadedAssetsList(clipToPlay);

        if (preventInterrupt == true) _nextTimeAllowTTS = Time.time + _ttsSource.clip.length / PlayerSettings.Audio.TTS_Speed;

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
            if (clip == null)
            {
                clip = Resources.Load<AudioClip>(TTS_ERROR_NOFILE_FILEPATH);
            }
            else clips.Add(clip);
        }

        float totalDelay = 0f;

        foreach (var clip in clips)
        {
            var delayObject = new GameObject(clip.name);
            var mono = delayObject.AddComponent<Delay>();

            mono.CallWithDelay(() =>
            {
                // Remove itself from queued object list when 
                // TTS file starts to play.
                RemoveFromQueuedList(mono.gameObject);
                PlayTTS(clip, "TTS Sequence :" + clip.name, preventInterrupt);
            }, totalDelay);

            float buffer = 0.01f; // Currently this is necessary to prevent clips from
                                  // interrupting each other. TODO: FIX
            totalDelay += clip.length / PlayerSettings.Audio.TTS_Speed + buffer;

            // This list is used to prevent TTS sequence from playing
            // if 
            _queuedTTSSequenceGameObjects.Add(mono.gameObject);
            Destroy(mono.gameObject, totalDelay);

        }
    }
    /// <summary>
    /// Play a single TTS file from the Resources-folder.
    /// </summary>
    /// <param name="path">Where in Resources is this file (without file-extention) Usually "TTS/Folder/Filename"</param>
    /// <param name="preventInterrupt">Can this be interrupted by another TTS call</param>
    public static void PlayTTSWithFilePath(string path, bool preventInterrupt = false)
    {
        var clip = Resources.Load<AudioClip>(path);
        PlayTTS(clip, path, preventInterrupt);
    }
    /// <summary>
    /// An overload that can give the duration of the clip is an out float.
    /// </summary>
    /// <param name="path">Where in Resources is this file (without file-extention)</param>
    /// <param name="clipDuration">Out how long is the audio file</param>
    /// <param name="preventInterrupt">Can another TTS line interrupt this TTS line</param>
    public static void PlayTTSWithFilePath(string path, out float clipDuration, bool preventInterrupt = false)
    {
        var clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            Debugger.LogWarning("No TTS Clip found at: " + path);
            PlayTTSFileNotFoundError();
            clipDuration = 0f;
            return;
        }
        clipDuration = clip.length * (1 / _ttsSource.pitch);
        PlayTTS(clip, path, preventInterrupt);
    }
    public static void PlayNumber(int number)
    {
        string numberString = GetStringFromNumber(number);
        var clip = Resources.Load<AudioClip>(TTSNUMBERSPATH + numberString);
        PlayTTS(clip, "Number: " + number);
    }
    private static string GetTTSErrorFilePath() => TTS_ERROR_NOFILE_FILEPATH;
    public static void RepeatTTS(int value)
    {
        if(TTSIsPlaying == true)
        {
            EventManager.OnRepeatTTSFailed.Raise("TTS is Playing", -1);
        }
        if(_TTSToRepeat == null)
        {
            PlayTTSNothingToRepeatError();
            return;
        }
        if(TTSIsPlaying == false)
        {
            PlayTTS(_TTSToRepeat, _TTSToRepeat.name, preventInterrupt: false);
        }
    }
    private static void TryAddClipToLoadedAssetsList(AudioClip clipToPlay)
    {
        if (_currentlyLoadedClips.Contains(clipToPlay)) return;
        _currentlyLoadedClips.Add(clipToPlay);
    }
    public static void PlayOnLoopUntilInterruptWithFilePath(string path)
    {
        PlayTTSWithFilePath(path);
        _ttsSource.loop = true;
    }
    public static void ForceStopAllTTS()
    {
        Debugger.Log("Force stop all TTS called", Debugger.TextColor.LightRed);
        _nextTimeAllowTTS = 0f;
        DestroyAndClearQueuedTTSCalls();
        UnloadUsedTTSClips(-1);

        if(_ttsSource != null) // When reloading scenes this can be null
        {
            _ttsSource.Stop();
            _ttsSource.loop = false;
        }
    }

    #region Helpers etc.
    private static void PlayTTSFileNotFoundError()
    {
        var clip = Resources.Load<AudioClip>(TTS_ERROR_NOFILE_FILEPATH);
        PlayTTS(clip, "No FIle Found Error Clip");
    }
    private static void PlayTTSNothingToRepeatError()
    {
        var clip = Resources.Load<AudioClip>(TTS_ERROR_NOREPEAT_FILEPATH);
        PlayTTS(clip, "Nothing to Repeat Error Clip");
    }
    
    /// <summary>
    /// Store a TTS clip as the clip to repeat on player input by path.
    /// </summary>
    /// <remarks>This will not play the clip.</remarks>
    /// <param name="path">Filepath to clip</param>
    public static void AddRepeatableTTS(string path)
    {
        var clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            _TTSToRepeat = null;
            return;
        }

        _TTSToRepeat = clip;
    }
    /// <summary>
    /// How long will it take with current speed settings to play TTS clip until the end.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Time to finish or 0 if clip not found</returns>
    public static float GetDurationOfTTSClipWithPath(string path)
    {
        var clip = Resources.Load<AudioClip>(path);
        if (clip == null)
        {
            return 0f;
        }
        return clip.length * (1 / _ttsSource.pitch);
    }
    private static void DestroyAndClearQueuedTTSCalls()
    {
        foreach (var item in _queuedTTSSequenceGameObjects)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        _queuedTTSSequenceGameObjects.Clear();
    }
    public static void RemoveFromQueuedList(GameObject go)
    {
        if (_queuedTTSSequenceGameObjects.Contains(go))
        {
            _queuedTTSSequenceGameObjects.Remove(go);
        }
    }
    public static void ResetStaticVariables()
    {
        _nextTimeAllowTTS = 0f;
    }
    private static void UnloadUsedTTSClips(int value)
    {
        // If this doesn't cause any frame issues then just keep this global.
        Debugger.Log("Unloading unused Resource-assets globally");
        Resources.UnloadUnusedAssets();
        // else
        /*
        foreach (var item in _currentlyLoadedClips)
        {
            Resources.UnloadAsset(item);
        }
        */
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
    #endregion
}
