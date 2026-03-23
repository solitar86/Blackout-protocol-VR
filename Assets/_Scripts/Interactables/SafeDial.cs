using JetBrains.Annotations;
using UnityEditor.Rendering;
using UnityEngine;

public class SafeDial : StaticInteractable
{
    private Quaternion _playerHandStarRotation = Quaternion.identity;
    private int lastNumber = 0;
    private float lastDifference = 0;


    private int currentDialNumber = 0;
    private int numbersOnDial = 10;
    float _startingZangle;
    float _lastDigitsZAngle;
    float _degreesPerAngle;

    float anglePreviousFrame;

    [Header("Safe Specific Settings")]
    [SerializeField] private Sound _singleClick;

#if UNITY_EDITOR
    float debugInterval = 0.5f;
    float debugTimer = 0;
#endif

    private void Start()
    {
        _degreesPerAngle = 360f / numbersOnDial;
    }
    public void Update()
    {

        if (IsActivated && TouchingHand != null)
        {
            float handZrotation = TouchingHand.transform.eulerAngles.z;
            if (handZrotation > 180f) handZrotation -= 360;

            if (Mathf.Abs(handZrotation - _lastDigitsZAngle) > _degreesPerAngle)
            {
                // We have turned enough to change the digit.
                if(_lastDigitsZAngle > handZrotation)
                {
                    //Dialing up
                    currentDialNumber = (currentDialNumber + 1 + numbersOnDial) % numbersOnDial;
                }
                else
                {
                    // Dialing down
                    currentDialNumber = (currentDialNumber - 1 + numbersOnDial) % numbersOnDial;
                }
                //TTSPlayer.PlayNumber(currentDialNumber);
                _lastDigitsZAngle = handZrotation;
                AudioPlayer.PlaySoundAtPoint(this, _singleClick, transform.position, false, true);
            }

#if UNITY_EDITOR
            DebugWorlSpaceText(Mathf.Abs(handZrotation - _lastDigitsZAngle));
#endif
        }
        else if (TouchingHand == null && IsActivated == true)
        {
            Activate();
        }
    }

    private void DebugWorlSpaceText(float toPrint)
    {
        debugTimer += Time.deltaTime;
        if (debugTimer > debugInterval)
        {
            debugTimer -= debugInterval;
            Debugger.WorldSpaceText(toPrint.ToString("F1"), transform.position);
        }
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

    public override void Activate()
    {
        if (IsActivated == false)
        {
            if (TouchingHand != null)
            {
                _playerHandStarRotation = TouchingHand.transform.rotation;
                _startingZangle = TouchingHand.transform.eulerAngles.z;
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
}
