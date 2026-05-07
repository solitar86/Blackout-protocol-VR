using UnityEngine;
using UnityEngine.Audio;
public class LocationVOTriggerVolume : MonoBehaviour
{
    [SerializeField] private Sound _voLocationSound;

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerSettings.Accessibility.LocationVOEnabled == false) return;
        if (other.TryGetComponent<Player>(out _))
        {
            EventManager.OnPlayerLocationIDShouldPlay.Raise(this, _voLocationSound);
        }

    }


    private void OnValidate()
    {
        if (_voLocationSound != null)
        {
            var mixer = Resources.Load<AudioMixer>("MainMixer");
            _voLocationSound.Mixergroup = mixer.FindMatchingGroups("InnerMonologue")[0];
            _voLocationSound.SpacialBlend = 0;
        }

    }
}
