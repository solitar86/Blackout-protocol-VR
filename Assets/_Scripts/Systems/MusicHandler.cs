using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicHandler : MonoBehaviour
{
    [SerializeField] private float _fadeUpDur = 1f;
    [SerializeField] private float _crossfadeDur = 3f;
    [SerializeField] private AudioClip _titleMusic;

    private AudioSource _musicSource;
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

        Debugger.Log("Playing track: " + trackToPlay.name, Debugger.TextColor.LightGreen);
        if (_musicSource.isPlaying)
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

        AudioPlayer.SetMusicVolumeTo(lerpedDecibelVolume);
    }
    private IEnumerator FadeDownMusic()
    {

        float timer = 0f;
        AudioPlayer.Instance.MainMixer.GetFloat(PlayerSettings.MUSIC_VOLUME_STRING, out float startVolume);
        float targetVolume = AudioPlayer.GetDecibelsWithNormalizedFloat(0f);
        float lerpvalue = 0f;

        while (timer < _fadeUpDur)
        {
            timer += Time.deltaTime;
            lerpvalue = Mathf.Lerp(startVolume, targetVolume, timer / _fadeUpDur);
            AudioPlayer.SetMusicVolumeTo(AudioPlayer.GetDecibelsWithNormalizedFloat(lerpvalue));
            yield return null;
        }

        _musicSource.volume = targetVolume;
    }
    private void HandleSceneLoadEvent(Scene arg0, LoadSceneMode arg1)
    {
        if (arg0.buildIndex != 0)
        {
            StopAllCoroutines();
            StartCoroutine(FadeDownMusic());
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
