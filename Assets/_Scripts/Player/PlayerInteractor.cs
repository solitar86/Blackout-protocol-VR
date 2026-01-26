using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    private PlayerHand _hand;
    private Collider _collider;
    private List<Iinteractable> _interactablesInRange = new();
    private Iinteractable _heldInteractable;

    #region Unity Life Cycle and Trigger functions
    private void Awake()
    {
        _hand = GetComponent<PlayerHand>();
        _collider = GetComponent<Collider>();
    }
    private void Start()
    {
        EventManager.OnGripPressed.AddListener(this, HandleGripPressed);
    }
    private void OnDisable()
    {
        EventManager.OnGripPressed.RemoveListener(this, HandleGripPressed);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<Iinteractable>(out var interactable))
        {
            if (_interactablesInRange.Contains(interactable)) return;

            _interactablesInRange.Add(interactable);
            interactable.Touch();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Iinteractable>(out var interactable))
        {
            interactable.EndTouch();
            if(_interactablesInRange.Contains(interactable))
                    _interactablesInRange.Remove(interactable);
        }
    }
    #endregion

    private void HandleGripPressed(bool isRightHand)
    {
        if (_interactablesInRange.Count <= 0) return;
        Debugger.Log("Trying pickup");
        if (_heldInteractable != null)
        {
            //Holding an item drop it first.
            // TODO: Consider swapping items on drop if one is in range?
            DropThisObject(_heldInteractable);
            return;
        }

        if ((_interactablesInRange.Count == 1))
        {
            // Only one option, see if can be picked up.
            if (_interactablesInRange[0] is PickUpObject)
            {
                // Only one item on list and it can be picked up.
                Debugger.Log("Only one option, pickup it up");
                PickUpObject(_interactablesInRange[0]);
                return;
            }
            else
            {
                // What to do when an interactable can't be picked up?
            }

        }

        // Several interactables objects in range
        List<PickUpObject> pickUpObjects = _interactablesInRange.OfType<PickUpObject>().ToList();

        if(pickUpObjects.Count == 1)
        {
            // Only one of these can be picked up
            Debugger.Log("Several Interactables in range, but only one PickUpObjects - picking up");
            PickUpObject(pickUpObjects[0]);
            return;
        }

        // Pickup closes PickUpObject
        Iinteractable closest = null;
        float closesDistance = float.MaxValue;

        foreach (var item in pickUpObjects)
        {
            float distance = Vector3.SqrMagnitude(transform.position - item.transform.position);
            if (distance < closesDistance)
            {
                closesDistance = distance;
                closest = item;
            }
        }

        // This should never be null at this point.
        PickUpObject(closest);
        Debugger.Log("Picked Up Item With Closest Distance, Rare case", Debugger.TextColor.Red);


    }

    private void DropThisObject(Iinteractable heldInteractable)
    {
        _heldInteractable.Drop();
        _heldInteractable = null;
    }

    private void PickUpObject(Iinteractable objectToPickUp)
    {
        objectToPickUp.PickUp(transform);
        _heldInteractable = objectToPickUp;
    }
}
