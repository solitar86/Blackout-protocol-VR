using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicHandler : MonoBehaviour
{
    [SerializeField] private float _fadeUpDur = 1f;
    [SerializeField] private float _crossfadeDur = 3f;
    [SerializeField] private AudioClip _titleMusic;

    private AudioSource _musicSource;

    private void Awake()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            PlayMusicTrack(_titleMusic);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += HandleSceneLoadEvent;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoadEvent;
    }

    private void HandleSceneLoadEvent(Scene arg0, LoadSceneMode arg1)
    {
       if(arg0.buildIndex != 0)
        {
            StopAllCoroutines();
            StartCoroutine(FadeDownMusic());
        }
    }

    private void PlayMusicTrack(AudioClip trackToPlay)
    {
        if (_musicSource == null)
        {
            _musicSource = GetComponent<AudioSource>();
            if (_musicSource == null) _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.outputAudioMixerGroup = AudioPlayer.GetMixerGroupWithSubPathString("Music");
            _musicSource.spatialBlend = 0f;
            _musicSource.playOnAwake = false;
        }

        if (_musicSource.isPlaying)
        {
            // Handle crossfade
            // A RETURN here caused the rest of the code no to run for some reason.
        }

        StartCoroutine(FadeUpMusicPlayer(trackToPlay));
    }

    private IEnumerator FadeUpMusicPlayer(AudioClip trackToPlay)
    {
        //_musicSource.generator = trackToPlay;
        _musicSource.clip = trackToPlay;
        _musicSource.volume = 0f;
        _musicSource.Play();

        Debugger.Log(_musicSource.clip, Debugger.TextColor.Orange);

        float timer = 0f;
        float startVolume = 0f;
        float targetVolume = 1f;

        while (timer < _fadeUpDur)
        {
            timer += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / _fadeUpDur);
            yield return null;
        }

        _musicSource.volume = targetVolume;
    }

    private IEnumerator FadeDownMusic()
    {

        float timer = 0f;
        float startVolume = _musicSource.volume;
        float targetVolume = 0f;

        while (timer < _fadeUpDur)
        {
            timer += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / _fadeUpDur);
            yield return null;
        }

        _musicSource.volume = targetVolume;
    }
}
