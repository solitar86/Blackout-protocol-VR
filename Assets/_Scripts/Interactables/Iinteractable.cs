using UnityEngine;

public interface Iinteractable
{
    public void Touch();
    public void EndTouch();
    public void PickUp(Transform parent);
    public void Activate();
    public void Drop();
    public void HitObject();
}