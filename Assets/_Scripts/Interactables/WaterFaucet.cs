using System;
using UnityEngine;

public class WaterFaucet : StaticInteractable
{
    [Space(15), Header("Water Faucet specific settings")]
    [SerializeField] private Sound _waterRunningLoop;
    [SerializeField] private Sound _waterDrippingLoopSound;
    [SerializeField] private Sound _faucetStartSound;
    [SerializeField] private Sound _faucetStopSound;
    [Space(15)]
    [SerializeField] private Transform _waterOutputPoint;
    [Header("Vibration settings")]
    [SerializeField] private VibrationSettingsSO _playerHandTouchWaterHapticSettings;

    private AudioSource _waterRunningLoopSource;
    private AudioSource _waterDrippingSource;

    #region Unity Callbacks
    private void Update()
    {
        if (IsActivated == true)
        {
            HandleFaucet_On_Audio();
            HandleDetectCoffeeCupUnderTap(); // Condisider optimizing this a bit. Interval maybe?
        }

        if (IsActivated == false)
        {
            HandleFaucet_Off_Audio();
        }
    }

    #endregion
    private void HandleFaucet_Off_Audio()
    {
        if (_waterDrippingSource == null)
        {
            // First time deactivated (could be start of scene)
            // Create dripping sound source and start loop.
            _waterDrippingSource = AudioPlayer.CreateLoopingAudioSource(this, _waterDrippingLoopSound);
            _waterDrippingSource.transform.position = transform.position;
        }

        if (_waterDrippingSource.isPlaying == false)
        {
            // We arent playing dripping loop yet
            // but we shuold be. Begin playing. 
            _waterDrippingSource.Play();
        }


        if (_waterRunningLoopSource != null && _waterRunningLoopSource.isPlaying == true)
        {
            // We were just deactivated. Stop water running sound
            // and play faucet stop sound.
            _waterRunningLoopSource.Stop();
            AudioPlayer.PlaySoundAtPoint(this, _faucetStopSound, transform.position, true);
        }
    }
    private void HandleFaucet_On_Audio()
    {
        if (_waterRunningLoopSource == null)
        {
            // Handle audio when faucet starts running for first time in game.
            // Play start sound and start water running loop.
            _waterRunningLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _waterRunningLoop);
            _waterRunningLoopSource.transform.position = transform.position;
            AudioPlayer.PlaySoundAtPoint(this, _faucetStartSound, transform.position, true);
        }

        if (_waterRunningLoopSource.isPlaying == false)
        {
            // We were just turned on.
            // Play startsound and start water running loop.

            _waterRunningLoopSource.Play();
            AudioPlayer.PlaySoundAtPoint(this, _faucetStartSound, transform.position, true);
        }

        if (_waterDrippingSource != null && _waterDrippingSource.isPlaying == true)
        {
            // Stop drip sound because water is running.
            _waterDrippingSource.Stop();
        }
    }
    private void HandleDetectCoffeeCupUnderTap()
    {
        if (_waterOutputPoint == null) _waterOutputPoint = transform.Find("WaterOutPutPoint");

        if (Physics.Raycast(_waterOutputPoint.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue))
        {
            if (hitInfo.collider.TryGetComponent<Iinteractable>(out var component))
            {
                if (component is CoffeeCup)
                {
                    _waterRunningLoopSource.Pause();
                    (component as CoffeeCup).FillCupWithWater();
                    return;
                }
            }
            else
            {
                _waterRunningLoopSource.UnPause();
            }

            if(hitInfo.collider.TryGetComponent<PlayerHand>(out var playerHand))
            {
                HandlPlayerHandUnderRunningWater(playerHand);
            }
        }
    }
    private void HandlPlayerHandUnderRunningWater(PlayerHand hand)
    {
        hand.HandleTouchSlide(_playerHandTouchWaterHapticSettings);
    }
    public void Deactivate()
    {
        if (IsActivated) Activate();
    }
}
