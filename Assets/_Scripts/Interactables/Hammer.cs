using UnityEngine;

public class Hammer : PickUpObject
{
    private bool _isHeld;
    public bool IsHeld => _isHeld;

    public override void Drop()
    {
        base.Drop();
        _isHeld = false;
    }

    public override void PickUp(Transform parent)
    {
        base.PickUp(parent);
        _isHeld = true;
    }
}
