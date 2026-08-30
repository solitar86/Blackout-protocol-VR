using UnityEngine;

public class TTS_From_UnityEvent : MonoBehaviour
{
    [Tooltip("Start path from ..TTS/ ->")]
    [SerializeField] private string _tts_Path = "";
    [SerializeField] private float _delay = 0f;
    [Tooltip("If this field is assigned, sound will play here instead of transform.position")]
    [SerializeField] private Transform _optionalPositionToPlayAt;
    [Tooltip("Prevent new sound from playing until previous has finished")]
    [SerializeField] private bool _preventOverlappingPlayes = false;
    [SerializeField] private bool _onlyPlayOnce = false;
    [Tooltip("How long a pause until sound is allowed to play again (Requires PreventOverlap)")]
    [SerializeField] private float _postDelay = 0f;

    private bool _hasPlayed = false;
    private float nextTimeAllowPlayback = 0f;
    public void Play_TTS()
    {
        if (_onlyPlayOnce == true && _hasPlayed == true) return;
        if (_preventOverlappingPlayes == true && nextTimeAllowPlayback > Time.time) return;
        _hasPlayed = true;

        var delayObject = new GameObject("TTS Call with Unity Event with delay of " + _delay);
        var mono = delayObject.AddComponent<Delay>();

        float ttsDuration = TTSPlayer.GetDurationOfTTSClipWithPath(_tts_Path);
        mono.CallWithDelay(() =>
        {

            TTSPlayer.PlayTTSWithFilePath(_tts_Path, true);
        }, _delay);
        Destroy(mono.gameObject, _delay + ttsDuration);
        nextTimeAllowPlayback = Time.time + ttsDuration + _postDelay;
    }

    private void OnValidate()
    {
        if (_tts_Path.Contains(".wav")) _tts_Path = _tts_Path.Replace(".wav", string.Empty);
        if (_tts_Path.Contains("Assets/Resources/")) _tts_Path = _tts_Path.Replace("Assets/Resources/", string.Empty);
    }
}
