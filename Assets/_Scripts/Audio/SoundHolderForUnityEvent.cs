using System.Drawing;
using UnityEngine;

public class SoundHolderForUnityEvent : MonoBehaviour
{
    [Header("Triggering music overrides most sound settings.")]
    [SerializeField] private bool _isMusic = false;
    [Space(15)]
    [SerializeField] private Sound _soundToPlay;
    [SerializeField] private bool _spatialize = true;
    [SerializeField] private bool _pitchVariation = true;
    [SerializeField] private float _delay = 0f;
    [Tooltip("If this field is assigned, sound will play here instead of transform.position")]
    [SerializeField] private Transform _optionalPositionToPlayAt;
    [Tooltip("Prevent new sound from playing until previous has finished")]
    [SerializeField] private bool _preventOverlappingPlayes = false;
    [Tooltip("How long a pause until sound is allowed to play again (Requires PreventOverlap)")]
    [SerializeField] private float _postDelay = 0f;

    private float nextTimeAllowPlayback = 0f;
    public void PlaySound()
    {
        if(_isMusic == false)
        {
            Debugger.Log("Playing Sound", Debugger.TextColor.Green);
            if (_preventOverlappingPlayes == true && nextTimeAllowPlayback > Time.time) return;

            var pos = transform.position;
            if (_optionalPositionToPlayAt != null) pos = _optionalPositionToPlayAt.position;

            var delayObject = new GameObject(_soundToPlay.Clip.name + "with delay of " + _delay);
            var mono = delayObject.AddComponent<Delay>();
            mono.CallWithDelay(() =>
            {
                AudioPlayer.PlaySoundAtPoint(this, _soundToPlay, pos, _pitchVariation, _spatialize);
            }, _delay);
            Destroy(mono.gameObject, _delay + _soundToPlay.Clip.length + 1f);
            nextTimeAllowPlayback = Time.time + _soundToPlay.Clip.length + _postDelay;
        }
        else
        {
            var delayObject = new GameObject("Play Music: " + _soundToPlay.Clip.name + "with delay of " + _delay);
            var mono = delayObject.AddComponent<Delay>();
            mono.CallWithDelay(() =>
            {
                FindFirstObjectByType<MusicHandler>().RequestPlayMusicTrack(_soundToPlay.Clip);
            }, _delay);
        }

    }

}
