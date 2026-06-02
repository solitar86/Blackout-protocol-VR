using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicHandler : MonoBehaviour
{
    [SerializeField] private float _fadeUpDur = 1f;
    [SerializeField] private float _crossfadeDur = 3f;
    [SerializeField] private AudioClip _titleMusic;
    [SerializeField] private float _musicLowVolumeInDecibels = -20f;

    private AudioSource _musicSource;
    [Tooltip("This is read from the Main Mixer Music mixergroup setting on startup.")]
    private float _musicDefaultVolumeInDecibels = 0f;

    #region Unity Callbacks

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoadEvent;
    }
    private void Awake()
    {
        AudioPlayer.Instance.MainMixer.GetFloat(PlayerSettings.MUSIC_VOLUME_STRING, out _musicDefaultVolumeInDecibels);

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            //Start playing title track if we start in the boot-up scene.
            PlayMusicTrack(_titleMusic);
        }
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoadEvent;
    }
    #endregion
    private void PlayMusicTrack(AudioClip trackToPlay)
    {
        InitMusicSource();
        if (_musicSource.isPlaying && _musicSource.clip != trackToPlay)
        {
            // Handle crossfade between track that is currently playing.
            // MAYBE MOVE THIS TO IT'S OWN COROUTINE
        }
        StartCoroutine(FadeUpMusicPlayer(trackToPlay));
    }
    private IEnumerator FadeUpMusicPlayer(AudioClip trackToPlay)
    {
        _musicSource.clip = trackToPlay;
        _musicSource.Play();

        float startVolume = AudioPlayer.GetDecibelsWithNormalizedFloat(0f);
        float targetVolume = _musicDefaultVolumeInDecibels;
        float timer = 0f;
        float lerpvalue = 0f;
        float lerpedDecibelVolume = 0f;

        while (timer < _fadeUpDur)
        {
            timer += Time.deltaTime;
            lerpvalue = timer / _fadeUpDur;
            lerpedDecibelVolume = Mathf.Lerp(startVolume, targetVolume, lerpvalue);
            AudioPlayer.SetMusicVolumeTo(lerpedDecibelVolume);
            yield return null;
        }

        AudioPlayer.SetMusicVolumeTo(targetVolume);
    }
    private IEnumerator FadeMusicToVolumeInDecibels(float decibels)
    {
        float timer = 0f;
        AudioPlayer.Instance.MainMixer.GetFloat(PlayerSettings.MUSIC_VOLUME_STRING, out float startVolume);
        float targetVolume = decibels;
        float lerpedVolumeInDecibels = 0f;
        while (timer < _fadeUpDur)
        {
            timer += Time.deltaTime;
            lerpedVolumeInDecibels = Mathf.Lerp(startVolume, targetVolume, timer / _fadeUpDur);
            AudioPlayer.SetMusicVolumeTo(lerpedVolumeInDecibels);
            yield return null;
        }

        AudioPlayer.SetMusicVolumeTo(targetVolume);
    }
    private void HandleSceneLoadEvent(Scene sceneFile, LoadSceneMode loadSceneMode)
    {
        if (sceneFile.buildIndex == 1)
        {
            StopAllCoroutines();
            StartCoroutine(FadeMusicToVolumeInDecibels(_musicLowVolumeInDecibels));
        }

        if (sceneFile.buildIndex == 2)
        {
            StopAllCoroutines();
            StartCoroutine(FadeMusicToVolumeInDecibels(-80f));
        }

        if(sceneFile.buildIndex == 0)
        {
            // we reloaded the bootupscene
            PlayMusicTrack(_titleMusic);
        }
    }
    
    #region Helpers, organization etc.
    private void InitMusicSource()
    {
        if (_musicSource == null)
        {
            _musicSource = GetComponent<AudioSource>();
            if (_musicSource == null) _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.outputAudioMixerGroup = AudioPlayer.GetMixerGroupWithSubPathString("Music");
            _musicSource.spatialBlend = 0f;
            _musicSource.playOnAwake = false;
        }
    }

    #endregion
}
