using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Project.SFX
{
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup _defaultMixerGroup;
        public static AudioPlayer Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else Destroy(gameObject);

            _defaultMixerGroup = Resources.Load<AudioMixer>("MainMixer").FindMatchingGroups("SFX")[0];
            _defaultMixerGroup.Logthis();
        }

        public static void PlaySoundAtPoint(object sender, Sound soundToPlay, Vector3 point, bool usePitchVariation = false)
        {

            if (soundToPlay.Clip == null)
            {
#if UNITY_EDITOR
                Debugger.Log("[Audio Player] " + sender + " sent a null Sound to play");
#endif
                return;
            }

            GameObject tempGameObject = new GameObject(sender + " : " + soundToPlay.Clip);
            tempGameObject.transform.position = point;
            AudioSource audioSource = (AudioSource)tempGameObject.AddComponent(typeof(AudioSource));
            audioSource.clip = soundToPlay.Clip;
            if (soundToPlay.Mixergroup == null && Instance == null) // We are being called as a static function
            {
                var mixer = Resources.Load<AudioMixer>("MainMixer");
                soundToPlay.Mixergroup = mixer.FindMatchingGroups("SFX")[0]; // SFX is default mixergroup
            }
            else
            {
                audioSource.outputAudioMixerGroup = soundToPlay.Mixergroup;
            }

            audioSource.spatialBlend = 0f;
            audioSource.volume = soundToPlay.Volume;
            audioSource.pitch = soundToPlay.Pitch;

            if (usePitchVariation) audioSource.pitch = AddPitchVariation(soundToPlay.Pitch);

            audioSource.Play();
            Destroy(tempGameObject,
                audioSource.clip.length);
        }

        /// <returns>Sound which was picked to play</returns>
        public static Sound PlayRandomSoundFromArrayAtPoint(object sender, Sound[] soundsArray, Vector3 point, Sound previousSound = null, bool usePitchVariation = false)
        {

            Sound randomSound = GetRandomSoundFromArray(soundsArray, previousSound);
            PlaySoundAtPoint(sender, randomSound, point, usePitchVariation);
            return randomSound;
        }

        public static void PlayClipAtPoint(object sender, AudioClip clipToPlay, Vector3 point, float volume = 1f)
        {
            Sound soundToPlay = new Sound();
            soundToPlay.Pitch = 1;
            soundToPlay.Volume = volume;
            soundToPlay.Clip = clipToPlay;

            PlaySoundAtPoint(sender, soundToPlay, point);
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

            Sound randomClip;
            do
            {
                randomClip = soundArray[Random.Range(0, soundArray.Length)];
            } while (randomClip == previousSound);

            return randomClip;
        }

        //public static AudioClip GetRandomClipFromArray(AudioClip[] _clipArray, AudioClip _previousClip = null)
        //{
        //    AudioClip randomClip;
        //    do
        //    {
        //        randomClip = _clipArray[Random.Range(0, _clipArray.Length)];
        //    } while (randomClip == _previousClip);

        //    return randomClip;
        //}
        private static float AddPitchVariation(float pitch)
        {
            float variation = 0.1f;
            return pitch += Random.Range(-variation, variation);
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
        [SerializeField] [Range(0f, 1f)] public float Volume = 1f;
        [SerializeField] [Range(-3, 3f)] public float Pitch = 1;
        [SerializeField] public float SpacialBlend;

        public Sound()
        {
            Volume = 1f;
            Pitch = 1;
            SpacialBlend = 0f;
        }

        public override string ToString()
        {
            return "Clip: " + Clip.ToString() + " Volume: " + Volume + ", Pitch: " + Pitch;
        }
    }
}
