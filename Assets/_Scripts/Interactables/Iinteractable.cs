using UnityEngine;
using static UnityEngine.LowLevelPhysics2D.PhysicsComposer;

public interface Iinteractable
{
    public void Touch(PlayerHand hand);
    public void EndTouch();
    public void PickUp(Transform parent, PlayerHand hand);
    public void Activate();
    public void Drop();

    /// <summary>
    /// This is meant for when player touches something with the held object.
    /// </summary>
    public void CollideWithObject();
}