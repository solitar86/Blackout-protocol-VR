using UnityEngine;

public interface Iinteractable
{
    public void Touch();
    public void EndTouch();
    public void PickUp(Transform parent);
    public void Activate();
    public void Drop();

    /// <summary>
    /// This is meant for when player touches something with the held object.
    /// </summary>
    public void CollideWithObject();
}