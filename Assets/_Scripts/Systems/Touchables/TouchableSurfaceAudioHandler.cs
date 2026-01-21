using System;
using UnityEngine;

[RequireComponent(typeof(TouchableSurface))]
public class TouchableSurfaceAudioHandler : MonoBehaviour
{
    [SerializeField] private TouchSoundHolderSO _touchSoundHolder;
    [Tooltip("This is doubled for volume going down")]
    [SerializeField] private float _slideAudioVolumeChangeSpeed = 0.0125f;

    private TouchableSurface _surface;
    private AudioSource _slideAudioSource;
    private float _audioSmoothDampVelocity = 0f;
    private bool _audioWasIncreasedThisFrame = false;

    private void Start()
    {
        _surface = GetComponent<TouchableSurface>();
        _surface.OnTouchStart.Addlistener(this, PlayFirstTouchSound);
        _surface.OnTouchSlide.Addlistener(this, HandleHandSlideSound);
        _surface.OnTouchEnd.Addlistener(this, PlayTouchEndSound);
    }

    private void PlayFirstTouchSound(Vector3 position)
    {
        AudioPlayer.PlaySoundAtPoint(this, _touchSoundHolder.FirstTouchSound, position, true);
    }

    private void HandleHandSlideSound((float distance, Vector3 position) tupleData)
    {
        if (_slideAudioSource == null)
        {
            _slideAudioSource = AudioPlayer.CreateLoopingAudioSource(this, _touchSoundHolder.SlideTouchSound);
            _slideAudioSource.maxDistance = 0.6f;
            _slideAudioSource.minDistance = 0.3f;
            _slideAudioSource.volume = 0f; // Don't play slide sound on first touch.
        }
        _slideAudioSource.transform.position = tupleData.position;


        _audioWasIncreasedThisFrame = true;
        _slideAudioSource.volume = Mathf.SmoothDamp(_slideAudioSource.volume,
                                                    _touchSoundHolder.SlideTouchSound.Volume,
                                                    ref _audioSmoothDampVelocity,
                                                    _slideAudioVolumeChangeSpeed);
        return;

    }

    private void Update()
    {
        if (_audioWasIncreasedThisFrame == false)
        {
            if (_slideAudioSource == null) return;
            _slideAudioSource.volume = Mathf.SmoothDamp(_slideAudioSource.volume,
                                                0f,
                                                ref _audioSmoothDampVelocity,
                                                _slideAudioVolumeChangeSpeed * 2);
        }
        _audioWasIncreasedThisFrame = false;
    }
    private void PlayTouchEndSound(Vector3 position)
    {
        AudioPlayer.PlaySoundAtPoint(this, _touchSoundHolder.EndTouchSound, position, true);

        if (_slideAudioSource != null)
        {
            Destroy(_slideAudioSource.gameObject);
        }
    }
    private void OnDisable()
    {
        _surface.OnTouchStart.Removelistener(this, PlayFirstTouchSound);
        _surface.OnTouchSlide.Removelistener(this, HandleHandSlideSound);
        _surface.OnTouchEnd.Removelistener(this, PlayTouchEndSound);
    }
}
