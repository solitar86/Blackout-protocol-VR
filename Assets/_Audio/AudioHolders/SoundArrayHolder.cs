using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "New SoundArrayHolder", menuName = "Audio Holders/New SoundArrayHolder")]
public class SoundArrayHolder : ScriptableObject
{
    [Tooltip("Drag audioclips here to add them to sounds array")]
    [SerializeField] private List<AudioClip> _importerArrayList;
    public Sound[] SoundArray;

    [HideInInspector]
    public Sound LastPlayedSound;

    private void OnValidate()
    {

        CreateSoundsFromImportArray();

        for (int i = 0; i < SoundArray.Length; i++)
        {
            CorrectPitchFromZero(SoundArray[i]);
            CorrectVolumeFromZero(SoundArray[i]);
            ApplyDefaultSpatializationBasedOnFileName(SoundArray[i]);
        }
    }

    private void CreateSoundsFromImportArray()
    {
        if (_importerArrayList == null) return;
        if (_importerArrayList.Count == 0) return;

        for (int i = _importerArrayList.Count - 1; i >= 0; i--)
        {
            if (ThisClipHasSoundInSoundArray(_importerArrayList[i]) == true)
            {
                _importerArrayList.Remove(_importerArrayList[i]);
                continue;
            }
            Sound s = new Sound(_importerArrayList[i]);
            Sound[] oneLongerArray = new Sound[SoundArray.Length + 1];
            oneLongerArray[^1] = s;
            Array.Copy(SoundArray, oneLongerArray, SoundArray.Length);
            SoundArray = oneLongerArray;
            _importerArrayList.Remove(_importerArrayList[i]);
        }
    }
    private bool ThisClipHasSoundInSoundArray(AudioClip clipToCheck)
    {
       foreach (Sound s in SoundArray)
        {
            if (s == null) continue;
            if (s.Clip == null) continue;
            if (s.Clip == clipToCheck)
            {
                Debugger.Log($"Clip {clipToCheck.name} is already in SoundHolder array");
                return true;
            }
        }
        return false;
    }

    private void ApplyDefaultSpatializationBasedOnFileName(Sound soundToCheck)
    {
        if (soundToCheck == null || soundToCheck.Clip == null) return;

        var mixer = Resources.Load<AudioMixer>("MainMixer");
        var footstepBus = mixer.FindMatchingGroups("Footsteps")[0];
        var SFXBus = mixer.FindMatchingGroups("SFX")[0];
        var monologueBus =  mixer.FindMatchingGroups("Inner")[0];
        var bodyCollisionBus = mixer.FindMatchingGroups("BodyCollision")[0];

        var fileNamePart = soundToCheck.Clip.name.ToLower();

        if (fileNamePart.Contains("foot"))
        {
            if(soundToCheck.SpacialBlend != 1)
            {
                soundToCheck.SpacialBlend = 1f;
                Debugger.Log("Forcing spatialblend to 1 for file: " + soundToCheck.Clip.name);
            }
            Debugger.Log("Changing mixergroup to 'Footsteps' for file: " + soundToCheck.Clip.name + " based on filename.");
            soundToCheck.Mixergroup = footstepBus;
        }

        if (fileNamePart.Contains("bump") && fileNamePart.Contains("vo") == false)
        {
            if (soundToCheck.SpacialBlend != 1)
            {
                soundToCheck.SpacialBlend = 1f;
                Debugger.Log("Forcing spatialblend to 1 for file: " + soundToCheck.Clip.name);
            }
            Debugger.Log("Changing mixergroup to 'BodyCollision' for file: " + soundToCheck.Clip.name + " based on filename.");
            soundToCheck.Mixergroup = bodyCollisionBus;
        }

        if (fileNamePart.Contains("foley") && soundToCheck.SpacialBlend != 1)
        {
            soundToCheck.SpacialBlend = 1f;
            Debugger.Log("Forcing spatialblend to 1 for file: " + soundToCheck.Clip.name);
        }

        if (fileNamePart.Contains("vo"))
        {
            if (soundToCheck.SpacialBlend != 0)
            {
                soundToCheck.SpacialBlend = 0f;
                Debugger.Log("Forcing spatialblend to 0 for file: " + soundToCheck.Clip.name);
            }
            Debugger.Log("Changing mixergroup to 'InnerMonologue' for file: " + soundToCheck.Clip.name + " based on filename.");
            soundToCheck.Mixergroup = monologueBus;
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
