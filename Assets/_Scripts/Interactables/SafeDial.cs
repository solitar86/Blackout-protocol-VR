using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class SafeDial : StaticInteractable
{

    #region Fields
    private Quaternion _playerHandStarRotation = Quaternion.identity;
    private int lastNumber = 0;
    [Tooltip("Leave blank for random, separate with ',' (comma)")]
    [SerializeField] private string combinationString = "";
    [Header("Safe Specific Settings")]
    [SerializeField] private Sound _singleClick;
    [SerializeField] private Sound _resetDial;
    [SerializeField] private Sound _turnDialIDVO;
    [SerializeField] private SoundArrayHolder _wentOVerDigitVOSoundHolder;
    [SerializeField] private Sound _releaseDialIDVO;
    [SerializeField] private Sound _resetDialVO;
    [SerializeField] private Sound _safeOpenVO;
    [SerializeField] private VibrationSettingsSO _safeClickVibrationSettings;
    [Tooltip("This gameobject will disable when the safe is opened")]
    [SerializeField] private GameObject _closedSafe;
    [Tooltip("This gameobject will enable when the safe is opened")]
    [SerializeField] private GameObject _openedSafe;
    [SerializeField] private UnityEvent OnPlayerTurnDial;
    [SerializeField] private UnityEvent OnSafeOpened;

    private int _combinationIndex = 0;
    private int[] _combination;
    private int _currentDialNumber = 0;
    private int _numbersOnDial = 10;
    float _startingZangle;
    float _lastDigitsZAngle;
    float _degreesPerAngle;
    private bool _isOpen = false;

    private bool _isAtCorrectDigit = false;
    private bool _isDialingDown = false;
    private bool _needsReset = false;

    private float _VONumberTimer = 0;
    private float _VONumberDelay = 0.1f;
    private bool _hasSaidNumber = false;

    #endregion

    #region Unity Callbacks
    private void Start()
    {
        _degreesPerAngle = 180f / _numbersOnDial;
        InitSafeCombination();

        _openedSafe.SetActive(false);
        _closedSafe.SetActive(true);
    }
    public void Update()
    {
        if (_isOpen == true) return;
        if (IsActivated == false || TouchingHand == null) return;

        float handZrotation = TouchingHand.transform.eulerAngles.z;
        if (handZrotation > 180f) handZrotation -= 360;

        if (Mathf.Abs(handZrotation - _lastDigitsZAngle) > _degreesPerAngle)
        {
            // We have turned enough to change the digit.
            ResetHasSaidNumberBoolAndTimer();


            if (_lastDigitsZAngle > handZrotation)
            {
                // Player went over correct digit
                if (!_isDialingDown && _isAtCorrectDigit && _needsReset == false)
                {
                    HandlePlayerTurnedOverCorrectDigit();
                }
                //Dialing up
                _isDialingDown = false;
                _currentDialNumber = (_currentDialNumber + 1 + _numbersOnDial) % _numbersOnDial;

                // ODD numbers in sequence have to dialed DOWN to. Ignore progress if player has gone over correct Digit.
                if (_currentDialNumber == _combination[_combinationIndex]
                                            && (_combinationIndex % 2 != 0
                                            && _needsReset == false))
                {
                    _isAtCorrectDigit = true;
                    PlayCorrectNumberClickSound();
                    _combinationIndex++;
                    CheckIfSafeShouldOpen();
                }
                else
                {
                    _isAtCorrectDigit = false;
                    PlayNormalClickSound();
                }
            }
            else
            {
                // Player went over correct digit
                if (_isDialingDown && _isAtCorrectDigit && _needsReset == false)
                {
                    HandlePlayerTurnedOverCorrectDigit();
                }
                // Dialing down
                _isDialingDown = true;
                _currentDialNumber = (_currentDialNumber - 1 + _numbersOnDial) % _numbersOnDial;

                // EVEN numbers in sequence have to dialed UP to. Ignore progress if player has gone over correct Digit.
                if (_currentDialNumber == _combination[_combinationIndex]
                                        && _combinationIndex % 2 == 0
                                        && _needsReset == false)
                {
                    // We are at correct digit.
                    _isAtCorrectDigit = true;
                    PlayCorrectNumberClickSound();
                    _combinationIndex++;
                    CheckIfSafeShouldOpen();
                }
                else
                {
                    _isAtCorrectDigit = false;
                    PlayNormalClickSound();
                }
            }

            _lastDigitsZAngle = handZrotation;
        }
        else if (TouchingHand == null && IsActivated == true)
        {
            Activate();
        }

        HandlePlayerDigitVOTimer(Time.deltaTime);

    }

    #endregion

    #region Core Funtions
    private void CheckIfSafeShouldOpen()
    {
        if (_combinationIndex == _combination.Length)
        {
            OpenSafe();
        }
    }
    private void OpenSafe()
    {
        float safeOpenDelay = 1.5f;

        this.CallWithDelay(() =>
        {
            EventManager.OnGeneralVOShouldPlay.Raise(this, _safeOpenVO);
            base.Activate();
            _isOpen = true;

            _closedSafe.gameObject.SetActive(false);
            _openedSafe.transform.SetParent(null); // We unparent from the _closed Safe before enabling the object.
            _openedSafe.gameObject.SetActive(true);
            OnSafeOpened?.Invoke();
        }, safeOpenDelay);
    }
    private void PlayNormalClickSound()
    {
        AudioPlayer.PlaySoundAtPoint(this, _singleClick, transform.position, false, true);
        TouchingHand.HandleSingleVibration(_safeClickVibrationSettings);
        OnPlayerTurnDial?.Invoke();
    }
    private void PlayCorrectNumberClickSound()
    {
        Sound correctSound = new Sound(_singleClick);
        correctSound.Pitch -= 0.4f;
        correctSound.Volume += 0.5f;
        //Debugger.PlayBlipSound();
        AudioPlayer.PlaySoundAtPoint(this, correctSound, transform.position, false, true);
    }
    private void HandlePlayerDigitVOTimer(float deltaTime)
    {
        if (_hasSaidNumber == false)
        {
            _VONumberTimer += deltaTime;
            if (_VONumberTimer >= _VONumberDelay)
            {
                PlaySafeDialCurrentDigitVO();
            }
        }
    }
    private void PlaySafeDialCurrentDigitVO()
    {
        if (_needsReset == false)
        {
            EventManager.OnPlayerShouldSayNumber.Raise(this, _currentDialNumber);
        }
        _hasSaidNumber = true;
    }
    private void ResetHasSaidNumberBoolAndTimer()
    {
        _VONumberTimer = 0f; // Reset timer to have player say dial number.
        _hasSaidNumber = false;
    }
    private void ResetDial()
    {
        _currentDialNumber = 0;
        _combinationIndex = 0;
        _needsReset = false;
        AudioPlayer.PlaySoundAtPoint(this, _resetDial, transform.position, false, true);

        this.CallWithDelay(() =>
        {
            EventManager.OnGeneralVOShouldPlay.Raise(this, _resetDialVO);
        }, _resetDial.Clip.length);
    }
    private void HandlePlayerTurnedOverCorrectDigit()
    {
        _needsReset = true;
        var voiceOver = AudioPlayer.GetRandomSoundFromArray(_wentOVerDigitVOSoundHolder,
                                                            _wentOVerDigitVOSoundHolder.LastPlayedSound);
        _wentOVerDigitVOSoundHolder.LastPlayedSound = voiceOver;
        EventManager.OnGeneralVOShouldPlay.Raise(this, voiceOver);

    }
    #endregion

    #region Interaction Functions
    public override void Activate()
    {
        if (_isOpen) return;
        ResetDial();

    }
    public override void PickUp(Transform parent, PlayerHand hand)
    {
        if (_isOpen) return;
        if (IsActivated == false)
        {
            if (TouchingHand != null)
            {
                _startingZangle = TouchingHand.transform.eulerAngles.z;
                if (_startingZangle > 180f) _startingZangle -= 360;
                _lastDigitsZAngle = _startingZangle;
                EventManager.OnGeneralVOShouldPlay.Raise(this, _turnDialIDVO);
                PlaySafeDialCurrentDigitVO();
            }
        }
        else
        {
            EventManager.OnGeneralVOShouldPlay.Raise(this, _releaseDialIDVO);
        }
            base.Activate();
    }
    public override void TouchStay(PlayerHand hand)
    {
        if ((IsActivated)) return;
        base.TouchStay(hand);
    }
    public override void EndTouch()
    {
        base.EndTouch();
        if (IsActivated == true)
        {
            EventManager.OnGeneralVOShouldPlay.Raise(this, _releaseDialIDVO);
            base.Activate();
        }
    }

    #endregion

    #region Helpers etc.
    private void InitSafeCombination()
    {
        Debugger.Log("Init safe", Debugger.TextColor.Orange);
        string debugCombinationString = string.Empty;
        bool parseFailed = false;

        if (string.IsNullOrEmpty(combinationString) == false)
        {
            // We have an assigned combination in the inspector
            string[] combinationNumbersAsString = this.combinationString.Split(',');
            int[] combinationAsIntArray = new int[combinationNumbersAsString.Length];

            for (int i = 0; i < combinationNumbersAsString.Length; i++)
            {
                if (int.TryParse(combinationNumbersAsString[i], out combinationAsIntArray[i]) == false)
                {
                    Debugger.LogError("Safe Combination couldn't be parsed. Defaulting to random");
                    parseFailed = true;
                    break;
                }
            }
            if(parseFailed == false) Debugger.Log($"Safe Combination is now: {combinationString}");
            _combination = combinationAsIntArray;
        }
        else parseFailed = true;

        if (parseFailed == true)
        {
            // No supplied combination or parse failure, generating a random with parameters.
            int[] randomCombinationIntArray = new int[3];

            for (int i = 0; i < randomCombinationIntArray.Length; i++)
            {
                if (i % 2 == 0)
                {
                    randomCombinationIntArray[i] = Random.Range(4, 8);
                }
                else
                {
                    randomCombinationIntArray[i] = Random.Range(1, 6);
                }

                debugCombinationString += randomCombinationIntArray[i].ToString() + ',';
            }

            _combination = randomCombinationIntArray;
            Debugger.Log($"Generated random Safe Combination: {debugCombinationString}");
        }
    }
    private void DebugWorlSpaceText(int toPrint)
    {
        // Debugger.WorldSpaceText(toPrint.ToString("F1"), transform.position);
    }
    private void AngleDifferenceWithQuaternionsMethod()
    {
        if (IsActivated && TouchingHand != null)
        {
            var angleDifference = Quaternion.Angle(_playerHandStarRotation, TouchingHand.transform.rotation);
            var normalized = angleDifference / 180f;
            int dialNumber = Mathf.FloorToInt(normalized * 11f);

            if (dialNumber != lastNumber)
            {
                lastNumber = dialNumber;
                TTSPlayer.PlayNumber(dialNumber);
            }

            Debugger.Log($"Number: {dialNumber} /" + " Difference: " + angleDifference.ToString("F2"));
        }
        else if (TouchingHand == null && IsActivated == true)
        {
            Activate();
        }
    }

    [ContextMenu("Open safe")]
    public void DebugOpenSafe()
    {
        OpenSafe();
    }
    #endregion
}
