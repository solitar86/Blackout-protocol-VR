using UnityEngine;
using UnityEngine.Audio;
[CreateAssetMenu(fileName = "DialogueSO", menuName = "new DialogueSO")]
public class DialogueSO : ScriptableObject
{
    [SerializeField] private Speaker _speaker;
    [SerializeField] private Sound _dialogueAudio;
    [Tooltip("This can be negative to have overlapping dialogue")]
    [SerializeField] private float _delayAfterDialogue = 0;
    [Tooltip("If checked the walkietalkie will not respond to player speech")]
    [SerializeField] private bool _isMonologue = false;

    public bool IsMonologue => _isMonologue;
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
