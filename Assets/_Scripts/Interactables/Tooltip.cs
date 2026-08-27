using UnityEngine;

/// <summary>
/// Used to play a TTS Tooltip by string.
/// To use activate GameObject or Call PlayToolTip with UnityEvent
/// </summary>
public class Tooltip : MonoBehaviour
{
    [SerializeField] private string _tooltipTTSPath;
    [SerializeField] private float delay;
    [SerializeField] private bool _playOnObjectEnabled;
    [SerializeField] private bool _destroyAfterUse;

    float _clipDuration = 0;
    float buffer = 0.1f;

    private void OnEnable()
    {
        if(_playOnObjectEnabled == true)
        {
            this.CallWithDelay(() =>

            {
                TTSPlayer.PlayTTSWithFilePath(_tooltipTTSPath, out _clipDuration, true);

                if (_destroyAfterUse == true)
                {
                    Destroy(gameObject, delay + _clipDuration + buffer);
                }
            }, delay);
        }
    }
    /// <summary>
    /// This can be called with a UnityEvent.
    /// </summary>
    public void PlayToolTip()
    {
        this.CallWithDelay(() =>
        {
            TTSPlayer.PlayTTSWithFilePath(_tooltipTTSPath, out _clipDuration, true);

            if (_destroyAfterUse == true)
            {
                Destroy(gameObject, delay + _clipDuration + buffer);
            }
        }, delay);

    }
}
