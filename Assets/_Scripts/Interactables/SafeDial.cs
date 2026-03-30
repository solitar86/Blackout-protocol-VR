using UnityEngine;
using UnityEngine.Events;

public class SafeDial : StaticInteractable
{

    #region Fields
    private Quaternion _playerHandStarRotation = Quaternion.identity;
    private int lastNumber = 0;
    [SerializeField] private string combinationString = "5,2,7";
    [Header("Safe Specific Settings")]
    [SerializeField] private Sound _singleClick;
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

    #endregion

    #region Unity Callbacks
    private void Start()
    {
        _degreesPerAngle = 360f / _numbersOnDial;
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
            if (_lastDigitsZAngle > handZrotation)
            {
                //Dialing up
                _currentDialNumber = (_currentDialNumber + 1 + _numbersOnDial) % _numbersOnDial;
                if (_currentDialNumber == _combination[_combinationIndex] && (_combinationIndex % 2 == 0))
                {
                    PlayCorrectNumberClickSound();
                    _combinationIndex++;
                    CheckIfSafeShouldOpen();
                }
                else
                {
                    PlayNormalClickSound();
                }
            }
            else
            {
                // Dialing down
                _currentDialNumber = (_currentDialNumber - 1 + _numbersOnDial) % _numbersOnDial;
                if (_currentDialNumber == _combination[_combinationIndex] && _combinationIndex % 2 != 0)
                {
                    PlayCorrectNumberClickSound();
                    _combinationIndex++;
                    CheckIfSafeShouldOpen();
                }
                else
                {
                    PlayNormalClickSound();
                }
            }

            _lastDigitsZAngle = handZrotation;
#if UNITY_EDITOR
            DebugWorlSpaceText(_currentDialNumber);
#endif


        }
        else if (TouchingHand == null && IsActivated == true)
        {
            Activate();
        }
    }

    #endregion

    #region Core Funtions
    private void CheckIfSafeShouldOpen()
    {
        if(_combinationIndex == _combination.Length)
        {
            OpenSafe();
        }
    }
    private void OpenSafe()
    {
        Debugger.WorldSpaceText("OPENED", transform.position);
        Activate();
        _isOpen = true;

        _closedSafe.gameObject.SetActive(false);
        _openedSafe.gameObject.SetActive(true);
        OnSafeOpened?.Invoke();
    }
    private void PlayNormalClickSound()
    {
        AudioPlayer.PlaySoundAtPoint(this, _singleClick, transform.position, false, true);
        OnPlayerTurnDial?.Invoke();
    }
    private void PlayCorrectNumberClickSound()
    {
        Sound correctSound = new Sound(_singleClick);
        correctSound.Pitch -= 0.4f;
        correctSound.Volume += 0.3f;
        Debugger.PlayBlipSound();
        AudioPlayer.PlaySoundAtPoint(this, correctSound, transform.position, false, true);
    }
    #endregion

    #region Interaction Functions
    public override void Activate()
    {
        if (_isOpen) return;
        if (IsActivated == false)
        {
            if (TouchingHand != null)
            {
                _startingZangle = TouchingHand.transform.eulerAngles.z;
                if (_startingZangle > 180f) _startingZangle -= 360;
                _lastDigitsZAngle = _startingZangle;
            }
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
        if (IsActivated == true) Activate();
    }
    
    #endregion

    #region Helpers etc.
    private void InitSafeCombination()
    {

        string[] combinationNumbers = combinationString.Split(',');
        int[] combinationAsIntArray = new int[combinationNumbers.Length];

        for (int i = 0; i < combinationNumbers.Length; i++)
        {
            combinationAsIntArray[i] = int.Parse(combinationNumbers[i]);
        }

        _combination = combinationAsIntArray;
    }
    private void DebugWorlSpaceText(int toPrint)
    {
        Debugger.WorldSpaceText(toPrint.ToString("F1"), transform.position);
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
    #endregion
}
