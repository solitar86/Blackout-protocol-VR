using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{

    [SerializeField] private InputActionAsset _actionAsset;


    private InputAction _rightTrigger;
    private InputAction _rightSelect;
    private InputAction _rightPrimaryButton;
    private InputAction _rightSecondaryButton;

    private InputAction _leftTrigger;
    private InputAction _leftSelect;
    private InputAction _leftPrimaryButton;
    private InputAction _leftSecondaryButton;

    private bool _isRightHand = true;

    private void Update()
    {

        // RIGHT HAND BUTTONS
        if (_rightTrigger.WasPerformedThisFrame())
        {
            Debugger.Log("Right Trigger Pressed", Debugger.TextColor.LightBlue);
        }
        if (_rightSelect.WasPerformedThisFrame())
        {
            EventManager.OnGripPressed.Raise(this, _isRightHand);
            Debugger.Log("Right Grip Pressed", Debugger.TextColor.LightBlue);
        }
        if (_rightPrimaryButton.WasPerformedThisFrame())
        {
            EventManager.OnPrimaryButtonPressed.Raise(this, _isRightHand);
            Debugger.Log("Right PrimaryButton Pressed", Debugger.TextColor.LightBlue);
        }
        if (_rightSecondaryButton.WasPerformedThisFrame())
        {
            Debugger.Log("Right Secondary Button Pressed", Debugger.TextColor.LightBlue);
        }

        // LEFTHANDBUTTONS
        if (_leftTrigger.WasPerformedThisFrame())
        {
            Debugger.Log("Left Trigger Pressed", Debugger.TextColor.LightBlue);
        }

        if (_leftSelect.WasPerformedThisFrame())
        {
            EventManager.OnGripPressed.Raise(this, !_isRightHand);
            Debugger.Log("Left Grip Pressed", Debugger.TextColor.LightBlue);
        }
        if (_leftPrimaryButton.WasPerformedThisFrame())
        {
            EventManager.OnPrimaryButtonPressed.Raise(this, !_isRightHand);
            Debugger.Log("Left Primary Button Pressed", Debugger.TextColor.LightBlue);
        }
        if (_leftSecondaryButton.WasPerformedThisFrame())
        {
            Debugger.Log("Left Secondary Button Pressed", Debugger.TextColor.LightBlue);
        }
    }

    private void OnEnable()
    {
        if (_actionAsset != null)
        {
            _actionAsset.Enable();
            PopulateActions();
        }


    }

    private void PopulateActions()
    {
        _rightTrigger = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("Activate");
        _rightSelect = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("Select");
        _rightPrimaryButton = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("Primary Button");
        _rightSecondaryButton = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("Secondary Button");

        _leftTrigger = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("Activate");
        _leftSelect = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("Select");
        _leftPrimaryButton = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("Primary Button");
        _leftSecondaryButton = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("Secondary Button");
    }

    private void OnDisable()
    {
        if (_actionAsset != null)
        {
            _actionAsset.Disable();
        }
    }
}
