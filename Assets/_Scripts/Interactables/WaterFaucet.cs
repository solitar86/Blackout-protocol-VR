using System;
using UnityEngine;
using UnityEngine.Events;

public class WaterFaucet : StaticInteractable
{
    [Space(15), Header("Water Faucet specific settings")]
    [SerializeField] private Sound _waterRunningLoop;
    [SerializeField] private Sound _waterDrippingLoopSound;
    [SerializeField] private Sound _faucetStartSound;
    [SerializeField] private Sound _faucetStopSound;
    [SerializeField] private Sound _cupOverFlowLoop;
    [Space(15)]
    [SerializeField] private Transform _waterOutputPoint;
    [SerializeField] private UnityEvent _onPlayerTouchRunningWater;
    [Header("Vibration settings")]
    [SerializeField] private VibrationSettingsSO _playerHandTouchWaterHapticSettings;

    private AudioSource _waterRunningLoopSource;
    private AudioSource _cupOverFlowAudioSource;
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

    #region Core Functions
    public override void TouchStay(PlayerHand hand)
    {
        // This check is so we can find the tap, but
        // so that it wouldn't get confused with 
        // touching the running water.
        if (IsActivated == true) return;
        HandlPlayerHandUnderRunningWater(hand);
    }
    private void HandleFaucet_Off_Audio()
    {
        HandleRunningWaterSoundWhenDeactivated();
    }
    private void HandleFaucet_On_Audio()
    {
        HandleRunningWaterSoundWhenActivated();
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
                    // Player has placed a cup after the running water.
                    HandleCupPlacedUnderRunningWaterSounds();
                    _waterRunningLoopSource.Pause();
                    (component as CoffeeCup).FillCupWithWater();
                    _onPlayerTouchRunningWater?.Invoke();
                    return;
                }
            }
            else
            {
                // There is no coffee cup under the faucet currently.
                HandleNoCupUnderRunningWaterSounds();
                _waterRunningLoopSource.UnPause();
            }

            if(hitInfo.collider.TryGetComponent<PlayerHand>(out var playerHand))
            {
                // Players hand is in the raycast of the running water.
                HandlPlayerHandUnderRunningWater(playerHand);
                _onPlayerTouchRunningWater?.Invoke();
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
    #endregion

    private void HandleRunningWaterSoundWhenActivated()
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
    private void HandleRunningWaterSoundWhenDeactivated()
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
    private void HandleCupPlacedUnderRunningWaterSounds()
    {
        if(_cupOverFlowAudioSource == null)
        {
            _cupOverFlowAudioSource = AudioPlayer.CreateLoopingAudioSource(this, _cupOverFlowLoop);
            _cupOverFlowAudioSource.transform.position = transform.position;
        }

        if (_cupOverFlowAudioSource.isPlaying == false)
        {
            // We arent playing dripping loop yet
            // but we shuold be. Begin playing. 
            _cupOverFlowAudioSource.Play();
        }
    }
    private void HandleNoCupUnderRunningWaterSounds()
    {
        if (_cupOverFlowAudioSource == null)
        {
            _cupOverFlowAudioSource = AudioPlayer.CreateLoopingAudioSource(this, _cupOverFlowLoop);
            _cupOverFlowAudioSource.transform.position = transform.position;
        }

        if (_cupOverFlowAudioSource.isPlaying == true)
        {
            // We had a cup underneath the faucet and it was removed. 
            _cupOverFlowAudioSource.Stop();
        }
    }
}
