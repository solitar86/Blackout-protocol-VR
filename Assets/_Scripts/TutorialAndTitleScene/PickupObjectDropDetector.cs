using System;
using UnityEngine;
using UnityEngine.Events;

public class PickupObjectDropDetector : MonoBehaviour
{
    public UnityEvent OnThisObjectDroppedOnFloor;

    private void OnEnable()
    {
        EventManager.OnAnyPickUpObjectHitFloor.AddListener(this, OnHitFloor);
    }
    private void OnDisable()
    {
        EventManager.OnAnyPickUpObjectHitFloor.RemoveListener(this, OnHitFloor);
    }

    private void OnHitFloor(PickUpObject droppedItem)
    {
        if(droppedItem.Equals(GetComponent<PickUpObject>()))
        {
            //This object was dropped on floor.
            OnThisObjectDroppedOnFloor?.Invoke();
            Debugger.Log($"{droppedItem} was <color=#00FF00>dropped on floor</color>");
        }
    }

}
