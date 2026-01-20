using System;
using UnityEngine;

public class TouchableSurfaceAudioHandler : MonoBehaviour
{
    [SerializeField] private TouchSoundHolderSO _touchSoundHolder;

    private TouchableSurface _surface;
    private Transform _slideAudioSourceTransform;

    private void Start()
    {
        _surface = GetComponent<TouchableSurface>();
        _surface.OnTouchStart.Addlistener(this, PlayFirstTouchSound);
        _surface.OnTouchSlide.Addlistener(this, HandleHandSlideSound);
        _surface.OnTouchEnd.Addlistener(this, PlayTouchEndSound);
    }

    private void HandleHandSlideSound((float distance, Vector3 position) tuple)
    {
        if (_slideAudioSourceTransform == null)
        {
            _slideAudioSourceTransform = AudioPlayer.CreateLoopingAudioSource(this, _touchSoundHolder.SlideTouchSound).transform;
        }

        _slideAudioSourceTransform.position = tuple.position;
    }

    private void PlayFirstTouchSound(Vector3 position)
    {
        AudioPlayer.PlaySoundAtPoint(this, _touchSoundHolder.FirstTouchSound, position, true);
    }
    private void PlayTouchEndSound(Vector3 position)
    {
        AudioPlayer.PlaySoundAtPoint(this, _touchSoundHolder.EndTouchSound, position, true);

        if( _slideAudioSourceTransform != null)
        {
            Destroy(_slideAudioSourceTransform.gameObject);
        }
    }
    private void OnDisable()
    {
        _surface.OnTouchStart.Removelistener(this, PlayFirstTouchSound);
        _surface.OnTouchSlide.Removelistener(this, HandleHandSlideSound);
        _surface.OnTouchEnd.Removelistener(this, PlayTouchEndSound);
    }
}
