using System;
using UnityEngine;
using static PlayerSettings;

public class PlayerSettings
{
    // Classes that hold data and string for saving them to PlayerPrefs.

    // Audio settings
    public static AudioPreferences Audio;
    public static string AUDIO_STRING = "audio";

    // Other settings

    #region Audiosettings Specific Functions

    public static float GetDecibelsFromNormalizedFloat(float decimalVolume)
    {

        float dbVolume = Mathf.Log10(decimalVolume) * 20;
        if (decimalVolume == 0.0f)
        {
            dbVolume = -80.0f;
        }

        return dbVolume;

    }

    #endregion

    #region Save / Load
    public static void LoadSettings()
    {
        // Get Default Audiosettings
        AudioSettingDefaults defaults = Resources.Load("Settings/AudioDefaultSettings") as AudioSettingDefaults;
        if (defaults == null)
        {
            Debugger.LogError("Audio settings default not found at path 'Settings/AudioDefaultSettings'");
            return;
        }

        // Load audio settings and print values to console.
        Audio = PlayerSettingsStorage.Load<AudioPreferences>(AUDIO_STRING, defaults.settings);
        Audio.Logthis();
    }

    public static void SaveSettings()
    {
        PlayerSettingsStorage.Save<AudioPreferences>(AUDIO_STRING, Audio);
    }

    #endregion



    // Classes that hold settings data.
    #region Serializable Settings classes

    [Serializable]
    public class AudioPreferences
    {
    
        public float MasterVolume = 1;
        public float TTS_Volume = 1;
        public float TTS_Speed = 1;

        private float _maxTTS_Speed = 2f;
        private float _minTTS_Speed = 0.4f;


        // Events
        public static Event OnTTSSpeedChange;

        public void LowerTTSVolume()
        {
            TTS_Speed += 0.2f;
            TTS_Speed = Mathf.Clamp(TTS_Speed, _minTTS_Speed, _maxTTS_Speed);
            Debugger.Log("TTS_Speed lowered to: " + TTS_Speed, Debugger.TextColor.LightGreen);
        }
        public void IncreaseTTSVolume()
        {
            TTS_Speed -= 0.2f;
            TTS_Speed = Mathf.Clamp(TTS_Speed, _minTTS_Speed, _maxTTS_Speed);
            Debugger.Log("TTS_Speed increased to: " + TTS_Speed, Debugger.TextColor.LightGreen);
        }



        public override string ToString()
        {
            return "Master Volume: " + MasterVolume + "\n" +
                "TTS Volume: " + TTS_Volume + "\n" +
                "TTS Speed: " + TTS_Speed;
        }
    }
    #endregion
}


[CreateAssetMenu(fileName = "AudioDefaultSettings", menuName = "Default settings / New Audio Default Settings")]
public class AudioSettingDefaults : ScriptableObject
{
    public AudioPreferences settings;
}
