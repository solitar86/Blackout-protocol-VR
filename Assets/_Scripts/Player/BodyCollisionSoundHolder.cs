using UnityEngine;

public class BodyCollisionSoundHolder: MonoBehaviour
{
    [SerializeField] SoundArrayHolder _playerBodyCollisionSoundHolder;
    [SerializeField] Sound _ID_voiceline;

    public SoundArrayHolder GetSoundArrayHolder() => _playerBodyCollisionSoundHolder;
    public Sound GetIdVoiceLine() => _ID_voiceline;
}