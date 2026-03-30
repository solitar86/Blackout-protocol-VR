using UnityEngine;
using UnityEngine.Events;

public class WaterFaucet : StaticInteractable
{
    [Space(15), Header("Water Faucet specific settings")]
    [Space(5)]
    [SerializeField] private Sound _waterRunningLoop;
    [SerializeField] private Sound _waterDrippingLoopSound;
    [SerializeField] private Sound _waterRunningStartSound;
    [SerializeField] private Sound _waterRunningStopSound;
    [SerializeField] private Sound _cupIsFillingSound;
    [SerializeField] private Sound _cupOverFlowLoop;
    [Space(15)]
    [SerializeField] private Transform _waterOutputPoint;
    [SerializeField] private UnityEvent _onPlayerTouchRunningWater;
    [Header("Vibration settings")]
    [SerializeField] private VibrationSettingsSO _playerHandTouchWaterHapticSettings;

    private AudioSource _waterRunningLoopSource;
    private AudioSource _cupOverFlowAudioSource;
    private AudioSource _waterDrippingSource;

    private bool _hasCupUnderFaucet = false;
    private float _cupFillRequiredDuration = 0f;
    private float _cupFillTimer = 0f;

    private GameObject fillingSoundGO;

    #region Unity Callbacks

    private void Awake()
    {
        float someDefaultValue = 2f;
        _cupFillRequiredDuration = _cupIsFillingSound.Clip == null ? someDefaultValue : _cupIsFillingSound.Clip.length;
    }
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
        if (IsActivated == false) return;
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
            if (hitInfo.collider.TryGetComponent<Iinteractable>(out var interactable))
            {
                if (interactable is CoffeeCup)
                {
                    // Player has placed a cup after the running water.
                    _hasCupUnderFaucet = true;
                    HandleCupPlacedUnderRunningWaterSounds(interactable as CoffeeCup);

                    _cupFillTimer += Time.deltaTime;
                    if(_cupFillTimer > _cupFillRequiredDuration)
                    {
                        (interactable as CoffeeCup).FillCupWithWater();
                    }
                    //_onPlayerTouchRunningWater?.Invoke();
                    return;
                }
            }
            else
            {
                // There is no coffee cup under the faucet currently, but it is still running.
                _hasCupUnderFaucet = false;
                _cupFillTimer = 0f;
                HandleNoCupUnderRunningWaterSounds();
            }

            // CHECK IF PLAYER HAND IS IN RUNNING WATER
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
            AudioPlayer.PlaySoundAtPoint(this, _waterRunningStartSound, transform.position, true);
        }

        if (_waterRunningLoopSource.isPlaying == false && _hasCupUnderFaucet == false)
        {
            // Play startsound and start water running loop.
            _waterRunningLoopSource.UnPause();
            AudioPlayer.PlaySoundAtPoint(this, _waterRunningStartSound, transform.position, true);
            Debugger.WorldSpaceText("Turning running source on.", transform.position);
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

        if (_cupOverFlowAudioSource != null && _cupOverFlowAudioSource.isPlaying == true)
        {
            // Incase we turn tap off while cup is under it
            _cupOverFlowAudioSource.Stop();
        }

        if (fillingSoundGO != null) Destroy(fillingSoundGO);

        if (_waterRunningLoopSource != null && _waterRunningLoopSource.isPlaying == true)
        {
            // We were just deactivated. Stop water running sound
            // and play faucet stop sound.
            _waterRunningLoopSource.Stop();
            AudioPlayer.PlaySoundAtPoint(this, _waterRunningStopSound, transform.position, true, true);
        }
    }
    private void HandleCupPlacedUnderRunningWaterSounds(CoffeeCup cup)
    {
        // CURRENTLY THIS ONLY HANDLES THE OVERFLOW SOUNDS
        // TODO: ADD FILLING SOUND LATER
        IfOverFlowSourceNullAssignIt();

        if(_waterRunningLoopSource.isPlaying == true)
        {
            _waterRunningLoopSource.Pause();
        }

        if(cup.IsFull == true)
        {
            if (_cupOverFlowAudioSource.isPlaying == false)
            {
                _cupOverFlowAudioSource.UnPause();
            }
        }
        else
        {
            if(Mathf.Approximately(_cupFillTimer, 0f) == true)
            {
                fillingSoundGO = AudioPlayer.PlaySoundAtPoint(this, _cupIsFillingSound, transform.position);
            }
        }
    }
    private void HandleNoCupUnderRunningWaterSounds()
    {
        // CURRENTLY THIS ONLY HANDLES THE OVERFLOW SOUNDS
        IfOverFlowSourceNullAssignIt();

        if(fillingSoundGO != null)
        {
            Destroy(fillingSoundGO);
            fillingSoundGO = null;
        }

        if (_waterRunningLoopSource.isPlaying == false)
        {
            //_waterRunningLoopSource.UnPause();
            _waterRunningLoopSource.Play();
        }

        if (_cupOverFlowAudioSource.isPlaying == true)
        {
            _cupOverFlowAudioSource.Pause();
        }
    }

    #region Helpers, organization etc.
    private void IfOverFlowSourceNullAssignIt()
    {
        if (_cupOverFlowAudioSource == null)
        {
            _cupOverFlowAudioSource = AudioPlayer.CreateLoopingAudioSource(this, _cupOverFlowLoop);
            _cupOverFlowAudioSource.transform.position = transform.position;
        }
    }
    #endregion
}
