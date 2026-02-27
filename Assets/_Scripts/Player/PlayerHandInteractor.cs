using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHandInteractor : MonoBehaviour
{
    private PlayerHand _hand;
    public PlayerHand Hand => _hand;
    private Collider _collider;
    private List<Iinteractable> _interactablesInRange = new();
    private Iinteractable _heldInteractable;
    public GameEvent<bool> OnGrabFailed = new("Nothing to grab in range");

    #region Unity Callbacks -> Trigger Callbacks
    private void Awake()
    {
        _hand = GetComponent<PlayerHand>();
        _collider = GetComponent<Collider>();
    }
    private void OnEnable()
    {
        EventManager.OnGripPressed.AddListener(this, HandleGripPressed);
        EventManager.OnGripReleased.AddListener(this, HandleGripReleased);
        EventManager.OnTriggerPressed.AddListener(this, HandleTriggerPressed);
//
    }
    private void OnDisable()
    {
        EventManager.OnGripPressed.RemoveListener(this, HandleGripPressed);
        EventManager.OnGripReleased.RemoveListener(this, HandleGripReleased);
        EventManager.OnTriggerPressed.RemoveListener(this, HandleTriggerPressed);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Iinteractable>(out var interactable))
        {
            if (_interactablesInRange.Contains(interactable))
            {
                return;
            }
            _interactablesInRange.Add(interactable);
            interactable.Touch(_hand);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Iinteractable>(out var interactable))
        {
            interactable.EndTouch();
            if (_interactablesInRange.Contains(interactable))
                _interactablesInRange.Remove(interactable);
        }
    }
    #endregion

    #region Input Responses
    private void HandleTriggerPressed(bool isRightHand)
    {
        if (isRightHand != _hand.IsRightHand) return;
        if (_interactablesInRange.Count <= 0) return;

        if (_heldInteractable != null)
        {
            //Holding an item activate it
            _heldInteractable.Activate();
            return;
        }

        if ((_interactablesInRange.Count == 1))
        {
            // Only one option, see if it can be picked up.
            if (_interactablesInRange[0] is StaticInteractable)
            {
                // Only one item on list and it static interactable.
                _interactablesInRange[0].Activate();
                return;
            }
            else
            {
                // What would cause us to get here? Figure it out!!

                // NOTE: If we press interact near a pickup object without holding it
                // can cause us to get here (apparently) Is that a problem?
                Debugger.LogWarning("Somehow we got here this time. Pressed Activate near a pickup object");
            }
        }

        // Several interactables objects in range, just activate all of them for now.
        Iinteractable[] interactablesInRangeArray = _interactablesInRange.ToArray();

        for (int i = 0; i < interactablesInRangeArray.Length; i++)
        {
            interactablesInRangeArray[i].Activate();
        }

        Debugger.Log("Activated several interactables, rare case", Debugger.TextColor.Red);
    }
    private void HandleGripPressed(bool isRightHand)
    {
        if (isRightHand != _hand.IsRightHand) return;
        if (_interactablesInRange.Count <= 0)
        {
            OnGrabFailed.Raise(this, isRightHand);
            return;
        }

        if ((_interactablesInRange.Count == 1))
        {
            // Only one option, see if it can be picked up.
            if (_interactablesInRange[0] is PickUpObject)
            {
                // Only one item on list and it can be picked up.
                PickUpObject(_interactablesInRange[0]);
                return;
            }
            else
            {
                OnGrabFailed.Raise(this, isRightHand);
                return;
            }
        }

        // Several interactables objects in range, get pickup objects
        List<PickUpObject> pickUpObjects = _interactablesInRange.OfType<PickUpObject>().ToList();

        if (pickUpObjects.Count == 1)
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
    private void HandleGripReleased(bool isRightHand)
    {
        if (isRightHand != _hand.IsRightHand) return;

        if (_heldInteractable != null)
        {
            // TODO: Consider swapping items on drop if one is in range?
            DropThisObjectAndEmptyHand(_heldInteractable);
            return;
        }
    }

    #endregion
    private void DropThisObjectAndEmptyHand(Iinteractable interactableToDrop)
    {
        if (_interactablesInRange.Contains(interactableToDrop))
        {
            _interactablesInRange.Remove(interactableToDrop);
        }

        interactableToDrop.Drop();
        _heldInteractable = null;
    }
    private void PickUpObject(Iinteractable objectToPickUp)
    {
        objectToPickUp.PickUp(transform, _hand);
        _heldInteractable = objectToPickUp;
    }
}
