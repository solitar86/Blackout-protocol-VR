using UnityEngine;
using UnityEngine.Audio;
using ContextMenu = UnityEngine.ContextMenu;

[RequireComponent(typeof(AudioSource))]
public class TTS_SpeedControl : MonoBehaviour
{

    // References to the AudioMixer and AudioSources
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioSource _TTS_source;
    private AudioMixerGroup TTS_bus;

    public AudioSource TTSSource => _TTS_source;

    #region Unity Callbacks
    private void Awake()
    {
        // Find the AudioMixerGroups and set the output of the AudioSources to them

        InitAudioMixer();
        InitAudioSource();

    }
    private void Start()
    {
        SetSpeedAndPitch(); // This doesn't work if it's in Awake.
        EventManager.OnTTSSPeedChange.AddListener(this, OnTTSSpeedChanged);
    }
    private void OnDisable()
    {
        EventManager.OnTTSSPeedChange.RemoveListener(this, OnTTSSpeedChanged);
    }
    #endregion

    private void SetSpeedAndPitch()
    {
        if (_audioMixer == null) InitAudioMixer();
        if (_TTS_source == null) InitAudioSource();
        _TTS_source.pitch = PlayerSettings.Audio.TTS_Speed;
        _audioMixer.SetFloat("TTSPitch", 1 / PlayerSettings.Audio.TTS_Speed); // Exposed params in MainMixer must match string exactly.
        _audioMixer.SetFloat("FFTSize", 512 / PlayerSettings.Audio.TTS_Speed); // Exposed params in MainMixer must match string exactly.
        Debugger.Log("TTS Settings set Pitch set to :" + 1 / PlayerSettings.Audio.TTS_Speed);
    }
    private void InitAudioSource()
    {
        _TTS_source = GetComponent<AudioSource>();

        if (_TTS_source == null)
        {
            _TTS_source = gameObject.AddComponent<AudioSource>();
        }

        // Init TTS BUS (Has to be created in MainMixer)
        TTS_bus = _audioMixer.FindMatchingGroups("TTS")[0];
        _TTS_source.outputAudioMixerGroup = TTS_bus;

        _TTS_source.loop = false;
        _TTS_source.playOnAwake = false;
        _TTS_source.bypassEffects = true;
        _TTS_source.bypassReverbZones = true;
        _TTS_source.spatialBlend = 0; // Does not need to be spatialized in any way.
        _TTS_source.bypassEffects = true;
        _TTS_source.Stop(); // Sanity check.
    }
    private void InitAudioMixer()
    {
        _audioMixer = Resources.Load("MainMixer") as AudioMixer;
    }
    private void OnTTSSpeedChanged(float value)
    {
        SetSpeedAndPitch();
    }

#if UNITY_EDITOR
    [UnityEngine.ContextMenu("Play On Loop")]
    public void PlayOnLoop()
    {
        SetSpeedAndPitch();
        _TTS_source.clip = Resources.Load("TTS/speech_TTS_Test") as AudioClip;
        _TTS_source.loop = true;
        _TTS_source.Play();
    }
    [UnityEngine.ContextMenu("Stop Playbackloop")]
    public void StopPlaybackLoop()
    {
        _TTS_source.Stop();
        _TTS_source.loop = false;
    }

#endif

}
