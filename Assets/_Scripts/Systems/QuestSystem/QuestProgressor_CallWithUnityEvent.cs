using UnityEngine;

public class QuestProgressor_CallWithUnityEvent : MonoBehaviour
{
    //[SerializeField] private string Name;
    [SerializeField] private QuestProgressionStep _questProgressionStep;
    private bool _hasBeenTriggered = false;
    public void ProgressQuest()
    {
        if (_hasBeenTriggered == true) return;
        _hasBeenTriggered = true;

        EventManager.OnProgressQuest.Raise(this, _questProgressionStep);
    }
}