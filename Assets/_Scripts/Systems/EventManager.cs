using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-999)]
public static class EventManager
{
    public static bool _logEnabled = true;

    // TTS Events
    public static GameEvent<float> OnTTSVolumeChange = new("TTS Volume Change");
    public static GameEvent<float> OnTTSSPeedChange = new("TTS Speed Change");

    // Input Events
    public static GameEvent<bool> OnPrimaryButtonPressed = new("Primary button pressed");
    public static GameEvent<bool> OnSecondaryButtonPressed = new("Secondary button pressed");
    public static GameEvent<bool> OnGripPressed = new("Secondary button pressed");
    public static void DisableEventLogs() => _logEnabled = false;
    public static void EnableEventLogs() => _logEnabled = true;
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
        Debugger.Log(_eventName + "-event was called by " + eventCaller.ToString(), Debugger.TextColor.Yellow);
        for (int i = _listenerList.Count - 1; i >= 0; i--)
        {
            _listenerList[i]?.Invoke(param);
        }
    }
}
