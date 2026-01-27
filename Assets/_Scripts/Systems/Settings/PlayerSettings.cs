using System;
using UnityEngine;
using static PlayerSettings.AudioPreferences;

public class PlayerSettings
{
    // Classes that hold data and string for saving them to PlayerPrefs.

    // Audio settings
    public static AudioPreferences Audio;
    public static string AUDIO_STRING = "audio";

    // Other settings
    public static DeveloperSettings Developer;
    public static string DEV_STRING = "dev";

    #region Save / Load
    public static void LoadSettings()
    {
        ////////////////////////////
        // Get Default Audiosettings
        ////////////////////////////
        AudioSettingDefaultsSO audioDefaults = Resources.Load<AudioSettingDefaultsSO>("Settings/AudioDefaultSettings");
        if (audioDefaults == null)
        {
            Debugger.LogError("Audio settings default not found at path 'Settings/AudioDefaultSettings'");
            return;
        }

        // Load audio settings - with defaults if none are saved.
        Audio = PlayerSettingsStorage.Load<AudioPreferences>(AUDIO_STRING, audioDefaults.settings);

        ////////////////////////////
        // Get Default Developer settings
        ////////////////////////////
        DeveloperSettingDefaultsSO developerDefaults = Resources.Load<DeveloperSettingDefaultsSO>("Settings/DeveloperDefaultSettings");
        if (developerDefaults == null)
        {
            Debugger.LogError("Developer settings default not found at path 'Settings/DeveloperDefaults'");
            return;
        }

        // Load audio settings - with defaults if none are saved.
        Developer = PlayerSettingsStorage.Load<DeveloperSettings>(DEV_STRING, developerDefaults.settings);
    }

    public static void SaveSettings()
    {
        PlayerSettingsStorage.Save<AudioPreferences>(AUDIO_STRING, Audio);
        PlayerSettingsStorage.Save<DeveloperSettings>(DEV_STRING, Developer);
    }

    public static void SetAllDefaults()
    {
        // TODO Implement this function.
    }

    #endregion



    // Classes that hold settings data.
    #region Serializable Settings classes


    ////////////////////////
    // AUDIO SETTINGS
    /// ////////////////////

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


        public void DecreaseTTS_Speed()
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

        #region Audiosettings Specific Helpers
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

        public override string ToString()
        {
            return "Master Volume: " + MasterVolume + "\n" +
                "TTS Volume: " + TTS_Volume + "\n" +
                "TTS Speed: " + TTS_Speed;
        }
    }
    ////////////////////////
    // DEVELOPER SETTINGS
    /// ////////////////////
    [Serializable]
    public class DeveloperSettings
    {
        public float TouchDialogueInterval = 2f;
        public float IdentifyVODelay = 0.3f;
    }

    #endregion
}
