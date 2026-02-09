using JetBrains.Annotations;
using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using static Debugger;
using static PlayerSettings.AudioPreferences;

public class PlayerSettings
{
    // Classes that hold data and string for saving them to PlayerPrefs.

    // Audio settings
    public static AudioPreferences Audio;
    public static string AUDIO_STRING = "audio";

    // Movement settings
    public static MovementSettings Movement;
    public static string MOVE_STRING = "move";

    // Dev settings
    public static DeveloperSettings Developer;
    public static string DEV_STRING = "dev";

    // Accessibility settings
    public static AccessibilitySettings Accessibility;
    public static string ACCESS_STRING = "access";

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
        // Get Default Movement
        ////////////////////////////
        MovementSettingsDefaultsSO moveDefault = Resources.Load<MovementSettingsDefaultsSO>("Settings/MovementDefaultSettings");
        if (moveDefault == null)
        {
            Debugger.LogError("Audio settings default not found at path 'Settings/MovementDefaultSettings'");
            return;
        }

        // Load movement settings - with defaults if none are saved.
        Movement = PlayerSettingsStorage.Load<MovementSettings>(MOVE_STRING, moveDefault.settings);


        ////////////////////////////
        // Get Default Developer settings
        ////////////////////////////
        DeveloperSettingDefaultsSO developerDefaults = Resources.Load<DeveloperSettingDefaultsSO>("Settings/DeveloperDefaultSettings");
        if (developerDefaults == null)
        {
            Debugger.LogError("Developer settings default not found at path 'Settings/DeveloperDefaults'");
            return;
        }

        // Load dev settings - with defaults if none are saved.
        Developer = PlayerSettingsStorage.Load<DeveloperSettings>(DEV_STRING, developerDefaults.settings);


        ////////////////////////////
        // Get Default Accessibility settings
        ////////////////////////////
       AccessibilitySettingsDefaultsSO accessDefaults = Resources.Load<AccessibilitySettingsDefaultsSO>("Settings/AccessibilityDefaultSettings");
        if (accessDefaults == null)
        {
            Debugger.LogError("Accessibility settings default not found at path 'Settings/AccessibilityDefaultSettings'");
            return;
        }

        // Load accessibility settings - with defaults if none are saved.
        Accessibility = PlayerSettingsStorage.Load<AccessibilitySettings>(ACCESS_STRING, accessDefaults.settings);
    }

    public static void SaveSettings()
    {
        PlayerSettingsStorage.Save<AudioPreferences>(AUDIO_STRING, Audio);
        PlayerSettingsStorage.Save<DeveloperSettings>(DEV_STRING, Developer);
        PlayerSettingsStorage.Save<AccessibilitySettings>(ACCESS_STRING, Accessibility);
        PlayerSettingsStorage.Save<MovementSettings>(MOVE_STRING, Movement);
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
        public float SlideAudioChangeSpeed = 0.0125f;
    }

    ////////////////////////
    // MOVEMENT SETTINGS
    /// ////////////////////
    [Serializable]
    public class MovementSettings
    {
        [Tooltip("This has to be 22.5, 45, 90 05 180 or stuff will break")]
        public float SnapTurnAngle;

        public bool TryIncreaseSnapTurnAngle()
        {
            switch (SnapTurnAngle)
            {
                case 22.5f:
                    SnapTurnAngle = 45f;
                    break;
                case 45f:
                    SnapTurnAngle = 90f;
                    break;
                case 90f:
                    SnapTurnAngle = 180f;
                    break;
                case 180f:
                    SnapTurnAngle = 180f;
                    break;
            }

            return TryUpdateSnapTurnAngle();
        }

        public bool TryDecreaseSnapTurnAngle()
        {
            switch (SnapTurnAngle)
            {
                case 22.5f:
                    SnapTurnAngle = 22.5f;
                    break;
                case 45f:
                    SnapTurnAngle = 22.5f;
                    break;
                case 90f:
                    SnapTurnAngle = 45f;
                    break;
                case 180f:
                    SnapTurnAngle = 90f;
                    break;
            }

            return TryUpdateSnapTurnAngle();
        }

        private bool TryUpdateSnapTurnAngle()
        {
            var turnProvider = GameObject.FindFirstObjectByType<SnapTurnProvider>();
            if (turnProvider != null)
            {
                turnProvider.ChangeTurnAmountToAngle(SnapTurnAngle);
                EventManager.OnMovementSettingsChange.Raise(this, -1);
                return true;
            }
            return false;
        }
    }

    ////////////////////////
    // ACCESSIBILITY SETTINGS
    /// ////////////////////
    [Serializable]
    public class AccessibilitySettings
    {
        private bool _hands = false;
        private bool _particles = false;
        private bool _touchRipple = false;
        private bool _debugLight = false;

        public bool Hands => _hands;
        public bool Particles => _particles;
        public bool TouchRipple => _touchRipple;
        public bool DebugLight => _debugLight;

        public void ToggleAll()
        {
            if(_hands == false || _particles == false || _touchRipple == false || _debugLight == false)
            {
                _hands = true;
                _particles = true;
                _touchRipple = true;
                _debugLight = true;
            }
            else
            {
                _hands = false;
                _particles = false;
                _touchRipple = false;
                _debugLight = false;
            }

            EventManager.OnAccessibilitySettingsChanged.Raise(this, -1);
        }

        public bool ToggleHands()
        {
            _hands = !_hands;
            EventManager.OnAccessibilitySettingsChanged.Raise(this, -1);
            return _hands;
        }
        public bool ToggleParticles()
        {
            _particles = !_particles;
            EventManager.OnAccessibilitySettingsChanged.Raise(this, -1);
            return _particles;
        }
        public bool ToggleTouchRipple()
        {
            _touchRipple = !_touchRipple;
            EventManager.OnAccessibilitySettingsChanged.Raise(this, -1);
            return _touchRipple;
        }

        public bool ToggleDebugLight()
        {
            _debugLight = !_debugLight;
            EventManager.OnAccessibilitySettingsChanged.Raise(this, -1);
            return _touchRipple;
        }
    }

    #endregion
}
