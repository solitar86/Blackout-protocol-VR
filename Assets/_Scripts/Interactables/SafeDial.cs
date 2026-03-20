using UnityEngine;

public class SafeDial : StaticInteractable
{
    private Quaternion _playerHandStarRotation = Quaternion.identity;

    [SerializeField] private float _degreesPerNumber = 180f / 12f;

    private float lastDifference = 0;
    public void Update()
    {
        if(IsActivated && TouchingHand != null)
        {
            var difference = Quaternion.Angle(_playerHandStarRotation, TouchingHand.transform.rotation);

            if(difference > _degreesPerNumber)
            {
                lastDifference = difference;
            }
        }
        else if (TouchingHand == null && IsActivated == true)
        {
            Activate();
        }

        
    }

    public override void Activate()
    {
        if(IsActivated == false)
        {
            if(TouchingHand != null)
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
