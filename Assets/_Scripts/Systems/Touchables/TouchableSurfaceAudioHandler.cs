using System;
using UnityEngine;

public class TouchableSurfaceAudioHandler : MonoBehaviour
{
    private TouchableSurface _surface;

    [SerializeField] private Sound _firstTouchSound;
    [SerializeField] private Sound _touchSlideSound;
    [SerializeField] private Sound _touchEndSound;
    private void Start()
    {
        _surface = GetComponent<TouchableSurface>();
        _surface.OnFirstTouch.Addlistener(this, PlayFirstTouchSound);
        _surface.OnTouchSlide.Addlistener(this, HandleHandSlideSound);
        _surface.OnTouchEnd.Addlistener(this, PlayTouchEndSound);
    }

    private void PlayFirstTouchSound(Vector3 position)
    {
        
    }
    private void HandleHandSlideSound(float obj)
    {
        
    }
    private void PlayTouchEndSound(Vector3 position)
    {
        
    }
    private void OnDisable()
    {
        _surface.OnFirstTouch.Removelistener(this, PlayFirstTouchSound);
        _surface.OnTouchSlide.Removelistener(this, HandleHandSlideSound);
        _surface.OnTouchEnd.Removelistener(this, PlayTouchEndSound);
    }
}
