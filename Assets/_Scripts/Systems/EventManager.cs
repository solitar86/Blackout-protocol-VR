using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-999)]
public static class EventManager
{
    public static bool LogsEnabled = true;
    public static string PREFSKEY = "EventLogsEnabled";

    // TTS Events
    public static GameEvent<float> OnTTSVolumeChange = new("TTS Volume Change");
    public static GameEvent<float> OnTTSSPeedChange = new("TTS Speed Change");
    public static GameEvent<string> OnTTSPlay = new("TTS Play");

    // Input Events
    public static GameEvent<int> OnPlayerWantSkip = new("Skip performed");
    public static GameEvent<bool> OnPrimaryButtonPressed = new("Primary button pressed");
    public static GameEvent<bool> OnSecondaryButtonPressed = new("Secondary button pressed");
    public static GameEvent<bool> OnSecondaryButtonHeld = new("Secondary button is held");
    public static GameEvent<bool> OnSecondaryButtonReleased = new("Secondary button released");
    public static GameEvent<bool> OnTriggerPressed = new("Trigger pressed");
    public static GameEvent<bool> OnGripPressed = new("Grip button pressed");
    public static GameEvent<bool> OnGripReleased = new("Grip button released");
    public static GameEvent<int> OnPlayerStartMove = new("Player start move");
    public static GameEvent<bool> OnPlayerPushJoystick = new("Player push stick");
    public static GameEvent<bool> OnPlayerSnapTurn = new("Plauer snap turn");

    // UI Events
    public static GameEvent<int> OnRadialMenuOpen = new("Radial menu open");
    public static GameEvent<int> OnRadialMenuClose = new("Radial menu close");
    public static GameEvent<float> OnMenuItemSelect = new("UI Button select changed");
    public static GameEvent<int> OnMenuItemActivate = new("UI Button activated");

    // Player Interaction and Reaction Events
    public static GameEvent<int> OnPlayerCurse = new("Player curse");
    public static GameEvent<PickUpObject> OnPlayerTouchPickUp = new("Player touch pickup");
    public static GameEvent<int> OnAnyObjectPickUpObjectPickedUp = new("Object picked up");
    public static GameEvent<int> OnCantCarryObject = new("Player Can't carry that");
    public static GameEvent<int> OnInteractableDetectedOnSurface = new("Item on surface");
    public static GameEvent<Sound> OnPlayerObjectIDVOShouldPlay = new("ID VO");
    public static GameEvent<Sound> OnPlayerBumpIDVOShouldPlay = new("Bump ID VO");

    // Interactable object events
    public static GameEvent<PickUpObject> OnAnyPickUpObjectHitFloor = new("Object dropped on ground");
    public static GameEvent<PickUpObject> OnAnyPickUpObjectPlacedOnSurface = new("Object placed on surface");
    public static GameEvent<StaticInteractable> OnAnyInteractableActivated = new("Interactable activated");

    // Conversation & Dialogue Events
    public static GameEvent<int> OnPlayerTryStartConversation = new("Player Try Start Conversation");
    public static GameEvent<string> OnConversationStarted = new("Conversation started");
    public static GameEvent<string> OnConversationEnded = new("Conversation Ended");
    public static GameEvent<DialogueSO> OnDialogueStart_Player = new("Player start speaking");
    public static GameEvent<DialogueSO> OnDialogueStop_Player = new("Player stop speaking");
    public static GameEvent<DialogueSO> OnDialogueStart_Radio = new("Radio start speaking");
    public static GameEvent<DialogueSO> OnDialogueStop_Radio = new("Radio stop speaking");

    // Quest progression event(s)
    public static GameEvent<QuestProgressionStep> OnProgressQuest = new("Progress Quest");
    public static GameEvent<int> OnAnyQuestWasProgressed = new("Quest was progressed");
    public static GameEvent<QuestSO> OnQuestCompleted = new("Quest Completed");

    // One shot gameplay events
    public static GameEvent<int> OnBreakableMachineBreak = new("Breakable machine broke");

    // Settings events
    public static GameEvent<int> OnAccessibilitySettingsChanged = new("Accessibility Settings Changed");
    public static GameEvent<int> OnMovementSettingsChange = new("Movement Settings Changed");

    //Tutorial specific events or other extremely special case events
    public static GameEvent<bool> OnToggleRadialMenuOnOff = new("Radial menu system toggled");

    public static void DisableEventLogs()
    {
        LogsEnabled = false;
        PlayerPrefs.SetInt(PREFSKEY, 0);
    }
    public static void EnableEventLogs()
    {
        LogsEnabled = false;
        PlayerPrefs.SetInt(PREFSKEY, 1);
    }

    public static void EmptyCallEventSubsribers()
    {

    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadDebuggerSettings()
    {
        int defaultValue = 1;
        LogsEnabled = PlayerPrefs.GetInt(PREFSKEY, defaultValue) == 1 ? true : false;
    }
}

public class GameEvent<T>
{
    public GameEvent(string name)
    {
        _eventName = name;
        _listenerList = new();
    }

    private List<Action<T>> _listenerList;
    private string _eventName;
    public void AddListener(object subscriber, Action<T> listener)
    {
        if (_listenerList.Contains(listener))
        {
            Debugger.LogWarning(subscriber.ToString() + " tried to add a duplicate listener");
            return;
        }

        _listenerList.Add(listener);
    }

    public void RemoveListener(object unsubscriber, Action<T> listener)
    {
        if (_listenerList.Contains(listener))
        {
            _listenerList.Remove(listener);
        }
    }

    public void Raise(object eventCaller, T param)
    {
        if (EventManager.LogsEnabled) Debugger.Log(_eventName + " - event was called by " + eventCaller.ToString(), Debugger.TextColor.Yellow);
        for (int i = _listenerList.Count - 1; i >= 0; i--)
        {
            _listenerList[i]?.Invoke(param);
        }
    }
}