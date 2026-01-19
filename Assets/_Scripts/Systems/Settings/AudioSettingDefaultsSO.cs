using UnityEngine;
using static PlayerSettings;

[CreateAssetMenu(fileName = "AudioDefaultSettings", menuName = "Default settings / New Audio Default Settings")]
public class AudioSettingDefaultsSO : ScriptableObject
{
    public AudioPreferences settings;
}
