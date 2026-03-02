using System;
using UnityEngine;

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
        if(soundToCheck.Clip.name.ToLower().Contains("foot"))
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
