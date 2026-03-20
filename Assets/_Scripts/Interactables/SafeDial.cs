using JetBrains.Annotations;
using UnityEngine;

public class SafeDial : StaticInteractable
{
    private Quaternion _playerHandStarRotation = Quaternion.identity;
    private int lastNumber = 0;

    private float lastDifference = 0;
    public void Update()
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
