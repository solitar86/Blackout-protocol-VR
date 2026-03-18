using System;
using UnityEngine;

public class QuestProgressor_OnTouchTouchableSurface : MonoBehaviour
{
    [Tooltip("When this object is touched it will set this queststate to the defined state")]
    [SerializeField] private QuestProgressionStep _questProgressionStep;

    private bool _hasBeenTriggered = false;

    private void OnEnable()
    {
        GetComponent<TouchableSurface>().OnHandTouchStart.AddListener(this, OnTouched);
    }


    private void OnDisable()
    {
        GetComponent<TouchableSurface>().OnHandTouchStart.RemoveListener(this, OnTouched);
    }

    private void OnTouched(Vector3 vector)
    {
        if (_hasBeenTriggered == true) return;
        _hasBeenTriggered = true;
        EventManager.OnProgressQuest.Raise(this, _questProgressionStep); 
    }

}
