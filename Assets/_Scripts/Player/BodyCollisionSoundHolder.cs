using UnityEngine;

public class BodyCollisionSoundHolder: MonoBehaviour
{
    [SerializeField] SoundArrayHolder _playerBodyCollisionSoundHolder;
    [SerializeField] Sound _ID_voiceline;
    [Tooltip("If this is assigned it will override the single ID voiceline and return a random one from the array.")]
    [SerializeField] private SoundArrayHolder _IDVoicelineHolder;

    public SoundArrayHolder GetSoundArrayHolder() => _playerBodyCollisionSoundHolder;
    public Sound GetIdVoiceLine()
    {
        if(_IDVoicelineHolder == null || _IDVoicelineHolder.SoundArray == null || _IDVoicelineHolder.SoundArray.Length == 0)
        {
            return _ID_voiceline;
        }

        return AudioPlayer.GetRandomSoundFromArray(_IDVoicelineHolder.SoundArray, _IDVoicelineHolder.LastPlayedSound);
    }
}