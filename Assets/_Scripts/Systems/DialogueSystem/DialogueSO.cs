
using System;
using UnityEngine;
using UnityEngine.Audio;
[CreateAssetMenu(fileName = "DialogueSO", menuName = "DialogueSO")]
public class DialogueSO : ScriptableObject
{
    [SerializeField] Speaker _speaker;
    [SerializeField] Sound _dialogueAudio;
    [Tooltip("This can be negative to have overlapping dialogue")]
    [SerializeField] float _delayAfterDialogue = 0;

    public Sound DialogueAudio => _dialogueAudio;
    public Speaker GetSpeaker() => _speaker;
    /// <returns>Clip.lenght + delay (which can be negative)</returns>
    public float GetDialogueDuration() => _dialogueAudio.Clip.length + _delayAfterDialogue;
    /// <returns>Lenght of the audiofile assigned to this dialogue.</returns>
    public float GetAudioDuration() => _dialogueAudio.Clip.length;

    private void OnValidate()
    {
        var mixer = Resources.Load<AudioMixer>("MainMixer");
        AudioMixerGroup mixergroup;

        if(_speaker == Speaker.Player)
        {
            mixergroup = mixer.FindMatchingGroups("InnerMonologue")[0];
        }
        else
        {
            mixergroup = mixer.FindMatchingGroups("Radio")[0];
            _dialogueAudio.OverrideDefaultDistances = true;
            _dialogueAudio.MinDistance = .2f;
            _dialogueAudio.MaxDistance = 3f;
            _dialogueAudio.SpacialBlend = 1f;
        }

        _dialogueAudio.Mixergroup = mixergroup;
    }
}
public enum Speaker
{
    Player,
    Radio
}
