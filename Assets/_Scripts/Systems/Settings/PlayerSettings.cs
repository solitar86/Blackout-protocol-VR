using Project.SFX;
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
    }

    public static void SaveSettings()
    {
        PlayerSettingsStorage.Save<AudioPreferences>(AUDIO_STRING, Audio);
    }

    public static void SetAllDefaults()
    {
        // TODO Implement this function.
    }

    #endregion



    // Classes that hold settings data.
    #region Serializable Settings classes

    [Serializable]
    public class AudioPreferences
    {
        public const string TTS_VOLUME_STRING = "TTSVolume";

        public float MasterVolume = 1;
        public float TTS_Volume = 1;
        public float TTS_Speed = 1;

        private float _maxTTS_Speed = 2f;
        private float _minTTS_Speed = 0.4f;
        private float _maxTTSVolume = 2f;
        private float _minTTSVolume = 0.1f;


        public void LowerTTS_Speed()
        {
            TTS_Speed -= 0.2f;
            TTS_Speed = Mathf.Clamp(TTS_Speed, _minTTS_Speed, _maxTTS_Speed);
            EventManager.OnTTSSPeedChange.Raise(this, TTS_Speed);
            Debugger.Log("TTS_Speed lowered to: " + TTS_Speed, Debugger.TextColor.LightGreen);
        }
        public void IncreaseTTS_Speed()
        {
            TTS_Speed += 0.2f;
            TTS_Speed = Mathf.Clamp(TTS_Speed, _minTTS_Speed, _maxTTS_Speed);
            EventManager.OnTTSSPeedChange.Raise(this, TTS_Speed);
            Debugger.Log("TTS_Speed increased to: " + TTS_Speed, Debugger.TextColor.LightGreen);
        }

        public void LowerTTS_Volume()
        {
            TTS_Volume -= 0.2f;
            TTS_Volume = Mathf.Clamp(TTS_Volume, _minTTSVolume, _maxTTSVolume);
            TryChangeTTSVolume();
        }

        public void IncreaseTTS_Volume()
        {
            TTS_Volume += 0.2f;
            TTS_Volume = Mathf.Clamp(TTS_Volume, _minTTSVolume, _maxTTSVolume);

            TryChangeTTSVolume();
        }

        private void TryChangeTTSVolume()
        {
            var succesful = AudioPlayer.Instance?.MainMixer?.SetFloat(TTS_VOLUME_STRING, GetDecibelsFromNormalizedFloat(TTS_Volume));
            if (succesful.Value == true)
            {
                EventManager.OnTTSVolumeChange.Raise(this, TTS_Volume);
            }
            else
            {
                Debugger.LogError("Failed to edit exposed param named: " + TTS_VOLUME_STRING);
            }
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
