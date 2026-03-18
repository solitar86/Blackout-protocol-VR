using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

[RequireComponent(typeof(TouchableSurface))]
public class TouchableSurfaceAudioHandler : MonoBehaviour
{
    [SerializeField] private Sound _touchIdentifyVO;
    [SerializeField] private TouchSoundHolderSO _touchSoundHolder;

    private TouchableSurface _surface;
    private AudioSource _slideAudioSource;
    private float _audioSmoothDampVelocity = 0f;
    private bool _audioWasIncreasedThisFrame = false;
    private float _nextTimeAllowTouchVO = 0f;

    #region Unity Callbacks
    private void OnEnable()
    {
        if(_surface == null) _surface = GetComponent<TouchableSurface>();
        _surface.OnHandTouchStart.AddListener(this, PlayFirstTouchSound);
        _surface.OnHandTouchSlide.AddListener(this, HandleHandSlideSound);
        _surface.OnHandTouchEnd.AddListener(this, PlayTouchEndSound);
        _surface.OnTouchedWithObject.AddListener(this, HandleTouchedWithObjectSound);
    }



    private void Update()
    {
        if (_audioWasIncreasedThisFrame == false)
        {
            if (_slideAudioSource == null) return;
            _slideAudioSource.volume = Mathf.SmoothDamp(_slideAudioSource.volume,
                                                0f,
                                                ref _audioSmoothDampVelocity,
                                                PlayerSettings.Developer.SlideAudioChangeTime);
        }
        _audioWasIncreasedThisFrame = false;
    }
    private void OnDisable()
    {
        _surface.OnHandTouchStart.RemoveListener(this, PlayFirstTouchSound);
        _surface.OnHandTouchSlide.RemoveListener(this, HandleHandSlideSound);
        _surface.OnHandTouchEnd.RemoveListener(this, PlayTouchEndSound);
        _surface.OnTouchedWithObject.RemoveListener(this, HandleTouchedWithObjectSound);
    }
    #endregion
    private void PlayFirstTouchSound(Vector3 position)
    {
        AudioPlayer.PlaySoundAtPoint(this, _touchSoundHolder.FirstTouchSound, position, true);

        if(_nextTimeAllowTouchVO < Time.time)
        {
            EventManager.OnPlayerObjectIDVOShouldPlay.Raise(this, _touchIdentifyVO);
            Debugger.Log(this.ToString() + "CALLED ID VO", Debugger.TextColor.Purple);
            _nextTimeAllowTouchVO = Time.time + PlayerSettings.Developer.TouchDialogueInterval;
        }
    }
    private void HandleHandSlideSound((float distance, Vector3 position) tupleData)
    {
        if (_slideAudioSource == null)
        {
            _slideAudioSource = AudioPlayer.CreateLoopingAudioSource(this, _touchSoundHolder.SlideTouchSound);
            _slideAudioSource.volume = 0f; // Don't play slide sound on first touch.
        }
        _slideAudioSource.transform.position = tupleData.position;


        _audioWasIncreasedThisFrame = true;
        _slideAudioSource.volume = Mathf.SmoothDamp(_slideAudioSource.volume,
                                                    _touchSoundHolder.SlideTouchSound.Volume,
                                                    ref _audioSmoothDampVelocity,
                                                    PlayerSettings.Developer.SlideAudioChangeTime);
        return;
    }
    private void PlayTouchEndSound(Vector3 position)
    {
        AudioPlayer.PlaySoundAtPoint(this, _touchSoundHolder.EndTouchSound, position, true);

        if (_slideAudioSource != null)
        {
            Destroy(_slideAudioSource.gameObject);
        }
    }
    private void HandleTouchedWithObjectSound(Iinteractable iinteractable)
    {
        var position = (iinteractable as MonoBehaviour).transform.position;
        AudioPlayer.PlaySoundAtPoint(this, _touchSoundHolder.FirstTouchSound, position, true);
    }

    private void OnValidate()
    {
        var innerMonologueMixerGroup = AudioPlayer.GetMixerGroupWithSubPathString(PlayerSettings.INNER_MONOLOGUE_MIXERGROUP_STRING);
        if (_touchIdentifyVO != null &&
            innerMonologueMixerGroup != null &&
            _touchIdentifyVO.Mixergroup != innerMonologueMixerGroup)
        {
            _touchIdentifyVO.Mixergroup = innerMonologueMixerGroup;
            _touchIdentifyVO.SpacialBlend = 0; // Inner monologue should not be spatilized.
        }
    }
}
