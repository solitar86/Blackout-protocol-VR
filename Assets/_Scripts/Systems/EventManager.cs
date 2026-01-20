using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-999)]
public static class EventManager
{
    public static GameEvent<float> OnTTSVolumeChange = new("TTS Volume Change");
    public static GameEvent<float> OnTTSSPeedChange = new("TTS Speed Change");
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
    public void Addlistener(object subscriber, Action<T> listener)
    {
        if (_listenerList.Contains(listener))
        {
            Debugger.LogWarning(subscriber.ToString() + " tried to add a duplicate listener");
            return;
        }

        _listenerList.Add(listener);
    }

    public void Removelistener(object unsubscriber, Action<T> listener)
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
