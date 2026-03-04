using System.Drawing;
using UnityEngine;

public class SoundHolderForUnityEvent : MonoBehaviour
{
    [SerializeField] private Sound _soundToPlay;
    [SerializeField] private bool _spatialize = true;
    [SerializeField] private bool _pitchVariation = true;
    [SerializeField] private float _delay = 0f;
    [Tooltip("If this field is assigned, sound will play here instead of transform.position")]
    [SerializeField] private Transform _optionalPositionToPlayAt;

    public void PlaySound()
    {
        var pos = transform.position;
        if (_optionalPositionToPlayAt != null) pos = _optionalPositionToPlayAt.position;

        var delayObject = new GameObject(_soundToPlay.Clip.name + "with delay of " + _delay);
        var mono = delayObject.AddComponent<Delay>();
        mono.CallWithDelay(() =>
        {
            AudioPlayer.PlaySoundAtPoint(this, _soundToPlay, pos, _pitchVariation, _spatialize);
        }, _delay);
        Destroy(mono.gameObject, _delay + _soundToPlay.Clip.length + 1f);
    }

}
