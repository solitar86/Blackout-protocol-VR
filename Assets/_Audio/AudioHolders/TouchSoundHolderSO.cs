using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(fileName = "New SoundHolder", menuName = "Audio Holders/New Touch SoundHolder")]
public class TouchSoundHolderSO : ScriptableObject
{
    //TODO Refactor this to use soundholders instead.
    public Sound FirstTouchSound;
    public Sound SlideTouchSound;
    public Sound EndTouchSound;

    private void OnValidate()
    {
        CorrectPitchFromZero(FirstTouchSound);
        CorrectPitchFromZero(SlideTouchSound);
        CorrectPitchFromZero(EndTouchSound);

        CorrectVolumeFromZero(FirstTouchSound);
        CorrectVolumeFromZero(SlideTouchSound);
        CorrectVolumeFromZero(EndTouchSound);

        SetSpacialBlendingToOne(FirstTouchSound);
        SetSpacialBlendingToOne(SlideTouchSound);
        SetSpacialBlendingToOne(EndTouchSound);
    }

    private void CorrectPitchFromZero(Sound sound)
    {
        if (sound != null && sound.Pitch == 0) sound.Pitch = 1;
    }
    private void CorrectVolumeFromZero(Sound sound)
    {
        if (sound != null && sound.Volume == 0) sound.Volume = 0.5f;
    }

    private void SetSpacialBlendingToOne(Sound sound)
    {
        if (sound != null && sound.SpacialBlend != 1) sound.SpacialBlend = 1f;
    }
}

