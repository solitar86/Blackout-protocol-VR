using System;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New SoundArrayHolder", menuName = "Audio Holders/New SoundArrayHolder")]
public class SoundArrayHolder : ScriptableObject
{
    public Sound[] SoundArray;

    [HideInInspector]
    public Sound LastPlayedSound;

    private void OnValidate()
    {
        for (int i = 0; i < SoundArray.Length; i++)
        {
            CorrectPitchFromZero(SoundArray[i]);
            CorrectVolumeFromZero(SoundArray[i]);
            ApplyDefaultSpatializationBasedOnFileName(SoundArray[i]);
        }
    }

    private void ApplyDefaultSpatializationBasedOnFileName(Sound soundToCheck)
    {
        if (soundToCheck == null || soundToCheck.Clip == null) return;
        if(soundToCheck.Clip.name.ToLower().Contains("foot"))
        {
            if(soundToCheck.SpacialBlend != 1)
            {
                soundToCheck.SpacialBlend = 1f;
                Debugger.Log("Forcing spatialblend to 1 for file: " + soundToCheck.Clip.name);
            }
            Debugger.Log("Changing mixergroup to 'Footsteps' for file: " + soundToCheck.Clip.name + " based on filename.");
            var mixer = Resources.Load<AudioMixer>("MainMixer");
            soundToCheck.Mixergroup = mixer.FindMatchingGroups("Footsteps")[0];
        }

        if (soundToCheck.Clip.name.ToLower().Contains("foley") && soundToCheck.SpacialBlend != 1)
        {
            soundToCheck.SpacialBlend = 1f;
            Debugger.Log("Forcing spatialblend to 1 for file: " + soundToCheck.Clip.name);
        }

        if (soundToCheck.Clip.name.ToLower().Contains("bump") && soundToCheck.SpacialBlend != 1)
        {
            soundToCheck.SpacialBlend = 1f;
            Debugger.Log("Forcing spatialblend to 1 for file: " + soundToCheck.Clip.name);
        }
    }

    private void CorrectVolumeFromZero(Sound sound)
    {
        if (sound != null && sound.Volume == 0) sound.Volume = 0.5f;
    }

    private void CorrectPitchFromZero(Sound sound)
    {
        if (sound != null && sound.Pitch == 0) sound.Pitch = 1;
    }
}
