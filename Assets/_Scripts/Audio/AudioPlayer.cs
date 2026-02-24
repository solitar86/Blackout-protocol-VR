using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup _defaultMixerGroup;
    public static AudioPlayer Instance;
    public static AudioSource errorAudioSource;
    public AudioMixer MainMixer { get; private set; }

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);

        MainMixer = Resources.Load<AudioMixer>("MainMixer");
        _defaultMixerGroup = MainMixer.FindMatchingGroups("SFX")[0];
    }
    #endregion
    public static GameObject PlaySoundAtPoint(object sender, Sound soundToPlay, Vector3 point, bool usePitchVariation = false, bool spatialize = true)
    {
        if (soundToPlay.Clip == null)
        {
            Debugger.Log(nameof(AudioPlayer) + " : " + sender + " sent a null Sound to play");
            return null;
        }
        GameObject tempGameObject =
            CreateTempGameObjectWithAudioSource(sender, soundToPlay, point, out AudioSource audioSource, spatialize);

        if (usePitchVariation) audioSource.pitch = AddPitchVariation(soundToPlay.Pitch);
        audioSource.Play();
        Destroy(tempGameObject, audioSource.clip.length);
        return tempGameObject;

    }
    /// <returns>Sound which was picked to play</returns>
    public static Sound PlayRandomSoundFromArrayAtPoint(object sender, Sound[] soundsArray, Vector3 point, Sound previousSound = null, bool usePitchVariation = false, bool spatialize = true)
    {
        Sound randomSound = GetRandomSoundFromArray(soundsArray, previousSound);
        PlaySoundAtPoint(sender, randomSound, point, usePitchVariation, spatialize);
        return randomSound;
    }
    public static void PlaySoundAtPointWithDelay(object sender, Sound soundToPlay, Vector3 point, float delay = 0f, bool usePitchVariation = false, bool spatialize = true)
    {
        if (soundToPlay.Clip == null)
        {
            Debugger.Log(nameof(AudioPlayer) + " : " + sender + " sent a null Sound to play");
            return;
        }

        if (delay < 0)
        {
            Debugger.Log("Delay was <0 reverting to absolutelu value");
            delay = Mathf.Abs(delay);
        }

        var delayObject = new GameObject(soundToPlay.Clip.name + "with delay of " + delay);
        var mono = delayObject.AddComponent<Delay>();
        mono.CallWithDelay(() =>
        {
            PlaySoundAtPoint(sender, soundToPlay, point, usePitchVariation, spatialize);
        }, delay);

        Destroy(mono.gameObject, delay + soundToPlay.Clip.length + 1f);
    }
    public static Sound GetRandomSoundFromArray(Sound[] soundArray, Sound previousSound = null)
    {

        if (soundArray.Length == 0)
        {
            // Debugger.Log("<color=#FF0000>" + soundArray + " sound array was empty</color>");
            Debugger.Log("[Audio Player] " + soundArray + " sound array was empty");
            Sound sound = new Sound(); // An empty sound will cause an error
            return sound;
        }

        if(soundArray.Length == 1)
        {
            //Only one option. Return that
            return soundArray[0];
        }

        Sound randomClip;
        do
        {
            randomClip = soundArray[Random.Range(0, soundArray.Length)];
        } while (randomClip == previousSound);

        return randomClip;
    }
    public static void PlayHandInsideColliderError(object sender)
    {
        // The Error Audiosource is never destroyd after creation. 
        if (errorAudioSource == null)
        {
            GameObject tempGameObject = CreateTempGameObjectWithAudioSource(sender, null, Vector3.zero, out errorAudioSource);
            tempGameObject.name = "Hand Inside Collider AudioSource";
            if (tempGameObject.TryGetComponent<MetaXRAudioSource>(out var metaXRAudioSource))
                    Destroy(metaXRAudioSource); // This is added by default but we don't need it.

            errorAudioSource.spatialize = false;
            errorAudioSource.spatialBlend = 0; 
            errorAudioSource.minDistance = 1f; // Do not hardcode these
            errorAudioSource.maxDistance = 3f; // Do not hardcode these
            errorAudioSource.volume = 1f;
            errorAudioSource.loop = true;
        }
        if (errorAudioSource.clip == null)
        {
            string errorSoundPath = "Audio/SFX_ErrorHum_Loop";
            errorAudioSource.clip = Resources.Load<AudioClip>(errorSoundPath);
            if (errorAudioSource.clip == null)
            {
                Debugger.LogWarning("No file sound at " + errorSoundPath);
                return;
            }
        }

        if (errorAudioSource.isPlaying == false) errorAudioSource.Play();

    }
    public static void PauseHandInsideColliderError()
    {
        if (errorAudioSource != null)
        {
            errorAudioSource.Pause();
        }
    }
    private static float AddPitchVariation(float pitch)
    {
        float variation = 0.1f;
        return pitch += Random.Range(-variation, variation);
    }

    /// <summary>
    /// Get a looping audiosource to do something with.
    /// </summary>
    /// <param name="sender">Used for error checking and naming gameobject</param>
    /// <param name="soundToLoop">The sound to start looping</param>
    /// <returns>Audiosource with loop and Sound settings applied</returns>
    public static AudioSource CreateLoopingAudioSource(object sender, Sound soundToLoop, bool spatialize = true)
    {
        GameObject tempGameObject =
            CreateTempGameObjectWithAudioSource(sender, soundToLoop, Vector3.zero, out AudioSource audioSource, spatialize);

        audioSource.spatialBlend = soundToLoop.SpacialBlend; // TODO Make this also SPATIALIZED AUDIO
        audioSource.clip = soundToLoop.Clip;

        if (soundToLoop.Mixergroup == null && Instance == null) 
        {
            // We are being called as a static function
            var mixer = Resources.Load<AudioMixer>("MainMixer");
            soundToLoop.Mixergroup = mixer.FindMatchingGroups("SFX")[0]; // SFX is default mixergroup
        }
        else
        {
            audioSource.outputAudioMixerGroup = soundToLoop.Mixergroup;
        }
        audioSource.loop = true;
        audioSource.Play();
        return audioSource;
    }
    private static GameObject CreateTempGameObjectWithAudioSource(object sender, Sound soundToPlay,
                                                                    Vector3 point, out AudioSource audioSource, bool spatialize = true)
    {
        if (soundToPlay == null) soundToPlay = new Sound(); // Error handling.

        GameObject tempGameObject;
        tempGameObject = new GameObject("SOUND: " + sender + " : " + (soundToPlay.Clip == null ? "null" : soundToPlay.Clip));
        tempGameObject.transform.position = point;
        audioSource = (AudioSource)tempGameObject.AddComponent(typeof(AudioSource));

        audioSource.clip = soundToPlay.Clip;
        if (soundToPlay.Mixergroup == null && Instance == null)
        {
            // We are being called as a static function
            var mixer = Resources.Load<AudioMixer>("MainMixer");
            soundToPlay.Mixergroup = mixer.FindMatchingGroups("SFX")[0]; // SFX is default mixergroup
        }
        else
        {
            audioSource.outputAudioMixerGroup = soundToPlay.Mixergroup;
        }

        audioSource.spatialBlend = soundToPlay.SpacialBlend;
        audioSource.volume = soundToPlay.Volume;
        audioSource.pitch = soundToPlay.Pitch;

        if(audioSource.pitch == 0)
        {
            Debugger.Log(soundToPlay + " has pitch of 0, reverting to 1");
            audioSource.pitch = 1;
        }
        float minDistance = 0.5f;
        float maxDistance = 2f;
        audioSource.minDistance = soundToPlay.OverrideDefaultDistances ? soundToPlay.MinDistance : minDistance;
        audioSource.maxDistance = soundToPlay.OverrideDefaultDistances ? soundToPlay.MaxDistance : maxDistance;
        audioSource.rolloffMode = AudioRolloffMode.Linear; // for default;

        // Handle Spatialization
        if(spatialize == true)
        {
            audioSource.spatialize = true;
            audioSource.spatialBlend = 1; // This is an assumption for now.
            var metaXRAudio = (MetaXRAudioSource)tempGameObject.AddComponent(typeof(MetaXRAudioSource));
            metaXRAudio.EnableSpatialization = true;
            metaXRAudio.EnableAcoustics = false; // Until we have a room setup, don't use room acoustics
            metaXRAudio.GainBoostDb = 12; // Global gain add because I don't want to tweak every sound.
        }

        return tempGameObject;
    }
    public static void PlayClipAtPoint(object sender, AudioClip clipToPlay, Vector3 point, float volume = 1f, bool usePitchVariation = false, bool spatialize = false)
    {
        Sound soundToPlay = new Sound();
        soundToPlay.Pitch = 1;
        soundToPlay.Volume = volume;
        soundToPlay.Clip = clipToPlay;

        PlaySoundAtPoint(sender, soundToPlay, point, usePitchVariation, spatialize);
    }

    /// <summary>
    /// Get a mixergroup with subpath string "a.k.a." even part of a name
    /// </summary>
    /// <param name="subpath">A part of the name of the group you want to get</param>
    /// <returns>First matching mixergroup.</returns>
    public static AudioMixerGroup GetMixerGroupWithSubPathString(string subpath)
    {
        if (Instance != null)
        {
            return Instance.MainMixer.FindMatchingGroups(subpath)[0];
        }

        var mixer = Resources.Load<AudioMixer>("MainMixer");
        return mixer.FindMatchingGroups(subpath)[0];
    }
}

/// <summary>
/// Class <c>Sound</c> is a holder for volume and pitch data for SFX Clips.
/// </summary>
[Serializable]
public class Sound
{
    [SerializeField] public AudioClip Clip;
    [SerializeField] public AudioMixerGroup Mixergroup;
    [SerializeField][Range(0f, 1f)] public float Volume = 1f;
    [SerializeField][Range(-3, 3f)] public float Pitch = 1;
    [SerializeField] public float SpacialBlend;
    [Space(15)]
    [SerializeField] public bool OverrideDefaultDistances;
    [SerializeField] public float MinDistance = 0.5f;
    [SerializeField] public float MaxDistance = 2f;

    public Sound()
    {
        Volume = 1f;
        Pitch = 1;
        SpacialBlend = 0f;
    }
    public Sound(Sound soundToCopyFrom)
    {
        Volume = soundToCopyFrom.Volume;
        Volume = soundToCopyFrom.Pitch;
        Pitch = soundToCopyFrom.Pitch;
        Clip = soundToCopyFrom.Clip;
        Mixergroup = soundToCopyFrom.Mixergroup;
        SpacialBlend = soundToCopyFrom.SpacialBlend;
        MinDistance = soundToCopyFrom.MinDistance;
        MaxDistance = soundToCopyFrom.MaxDistance;
    }

    public override string ToString()
    {
        return "Clip: " + Clip.ToString() + " Volume: " + Volume + ", Pitch: " + Pitch;
    }
}

