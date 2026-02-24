
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
    /// <summary>
    /// Returns clip.lenght + delay (which can be negative)
    /// </summary>
    /// <returns>How long before next dialogue is allowed to play</returns>
    public float GetDialogueDuration() => _dialogueAudio.Clip.length + _delayAfterDialogue;
    public Speaker GetSpeaker() => _speaker;

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
