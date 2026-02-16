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
    private InputAction _rightMove;
    private InputAction _rightSkip;

    private InputAction _leftTrigger;
    private InputAction _leftSelect;
    private InputAction _leftPrimaryButton;
    private InputAction _leftSecondaryButton;
    private InputAction _leftMove;
    private InputAction _leftSkip;

    private bool _isRightHand = true;
    private static bool _playerIsMoving = false;

    private Vector2 _leftMoveVector;
    private Vector2 _rightMoveVector;

    public static bool PlayerIsMoving => _playerIsMoving;


    private void Update()
    {

        ////////////////////////////////
        // RIGHT HAND BUTTONS
        ////////////////////////////////
        if (_rightTrigger.WasPerformedThisFrame())
        {
            Debugger.Log("Right Trigger Pressed", Debugger.TextColor.LightBlue);
            EventManager.OnTriggerPressed.Raise(this, _isRightHand);
        }
        // Grip press
        if (_rightSelect.WasPerformedThisFrame())
        {
            Debugger.Log("Right Grip Pressed", Debugger.TextColor.LightBlue);
            EventManager.OnGripPressed.Raise(this, _isRightHand);
        }
        // Grip Release
        if (_rightSelect.WasReleasedThisFrame())
        {
            Debugger.Log("Right Grip Released", Debugger.TextColor.LightBlue);
            EventManager.OnGripReleased.Raise(this, _isRightHand);
        }
        if (_rightPrimaryButton.WasPerformedThisFrame())
        {
            Debugger.Log("Right PrimaryButton Pressed", Debugger.TextColor.LightBlue);
            EventManager.OnPrimaryButtonPressed.Raise(this, _isRightHand);
        }
        if (_rightSecondaryButton.WasPerformedThisFrame())
        {
            Debugger.Log("Right Secondary Button Pressed", Debugger.TextColor.LightBlue);
            EventManager.OnSecondaryButtonPressed.Raise(this, _isRightHand);
        }

        ////////////////////////////////
        // LEFT HAND BUTTONS
        ////////////////////////////////
        if (_leftTrigger.WasPerformedThisFrame())
        {
            Debugger.Log("Left Trigger Pressed", Debugger.TextColor.LightBlue);
            EventManager.OnTriggerPressed.Raise(this, !_isRightHand);
        }
        // Grip press
        if (_leftSelect.WasPerformedThisFrame())
        {
            Debugger.Log("Left Grip Pressed", Debugger.TextColor.LightBlue);
            EventManager.OnGripPressed.Raise(this, !_isRightHand);
        }
        // Grip Release
        if (_leftSelect.WasReleasedThisFrame())
        {
            Debugger.Log("Left Grip Released", Debugger.TextColor.LightBlue);
            EventManager.OnGripReleased.Raise(this, !_isRightHand);
        }
        if (_leftPrimaryButton.WasPerformedThisFrame())
        {
            Debugger.Log("Left Primary Button Pressed", Debugger.TextColor.LightBlue);
            EventManager.OnPrimaryButtonPressed.Raise(this, !_isRightHand);
        }
        if (_leftSecondaryButton.WasPerformedThisFrame())
        {
            Debugger.Log("Left Secondary Button Pressed", Debugger.TextColor.LightBlue);
            EventManager.OnSecondaryButtonPressed.Raise(this, !_isRightHand);
        }


        ////////////////////////////////
        ///SKIP ACTIONS
        ////////////////////////////////
        if (_leftSkip.WasPerformedThisFrame() || _rightSkip.WasPerformedThisFrame())
        {
            Debugger.Log("Skip performed", Debugger.TextColor.LightBlue);
            EventManager.OnPlayerWantSkip.Raise(this, -1);
        }


        ////////////////////////////////
        // PLAYER MOVEMENT
        ////////////////////////////////

        _playerIsMoving = _leftMoveVector != Vector2.zero ||
                            _rightMoveVector != Vector2.zero;
    }
    private void PopulateActions()
    {
        _rightTrigger = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("Activate");
        _rightSelect = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("Select");
        _rightPrimaryButton = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("Primary Button");
        _rightSecondaryButton = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("Secondary Button");
        _rightMove = _actionAsset.FindActionMap("XRI Right Locomotion").FindAction("Move");

        _rightSkip = _actionAsset.FindActionMap("XRI Right Interaction").FindAction("SkipTutorial");

        _leftTrigger = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("Activate");
        _leftSelect = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("Select");
        _leftPrimaryButton = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("Primary Button");
        _leftSecondaryButton = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("Secondary Button");
        _leftMove = _actionAsset.FindActionMap("XRI Left Locomotion").FindAction("Move");

        _leftSkip = _actionAsset.FindActionMap("XRI Left Interaction").FindAction("SkipTutorial");
    }
    private void OnRightMove(InputAction.CallbackContext context)
    {
        _rightMoveVector = context.ReadValue<Vector2>();
        //Debugger.Log("Right Move: " _rightMoveVector);

    }
    private void OnLeftMove(InputAction.CallbackContext context)
    {
        _leftMoveVector = context.ReadValue<Vector2>();
        //Debugger.Log("Left Move: " + _leftMoveVector);
    }
    private void OnPlayerStartMove(InputAction.CallbackContext context)
    {
        if(Player.Instance.PlayerCanMove)
        {
            EventManager.OnPlayerStartMove.Raise(this, -1);
        }
    }
    private void SubscribeToEvents()
    {
        _leftMove.started += OnPlayerStartMove;
        _leftMove.performed += OnLeftMove;
        _leftMove.canceled += OnLeftMove;

        _rightMove.started += OnPlayerStartMove;
        _rightMove.performed += OnRightMove;
        _rightMove.canceled += OnRightMove;

    }
    private void UnsubscribeFromEvents()
    {
        _leftMove.performed -= OnLeftMove;
        _leftMove.canceled -= OnLeftMove;
        _rightMove.performed -= OnRightMove;
        _rightMove.canceled -= OnRightMove;
    }
    private void OnEnable()
    {
        if (_actionAsset != null)
        {
            _actionAsset.Enable();
            PopulateActions();
            SubscribeToEvents();
        }
    }
    private void OnDisable()
    {
        if (_actionAsset != null)
        {
            UnsubscribeFromEvents();
            _actionAsset.Disable();
        }
    }
}
