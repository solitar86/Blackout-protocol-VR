using UnityEngine;

public interface Iinteractable
{
    public void Touch(PlayerHand hand);
    public void EndTouch();
    public void PickUp(Transform parent, PlayerHand hand);
    public void Activate();
    public void Release();
    public void Ping(float delay);

    /// <summary>
    /// This is meant for when player touches something with the held object.
    /// </summary>
    public void CollideWithObject();
}