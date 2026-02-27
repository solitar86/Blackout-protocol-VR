using UnityEngine;

public class FootstepSurface : MonoBehaviour
{
    [SerializeField] private string Name = "Unnamed";
    [SerializeField] private SoundArrayHolder _footstepSoundArrayHolder;

    public SoundArrayHolder GetFootStepSounds() => _footstepSoundArrayHolder;
}
