using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "new QuestSO")]
public class QuestSO : ScriptableObject
{
    public string Name;
    [SerializeField] private QuestState _state = QuestState.Unknown;
    public QuestState State => _state;

    [Tooltip("TIP conversation for when player has discovered the quest")]
    [SerializeField] private ConversationSO _discoveredStateHintConvo;
    [Tooltip("TIP conversation for when player has attempted the quest but not completed")]
    [SerializeField] private ConversationSO _startedStateHintConvo;


    #region Helpers
    public void ChangeStateTo(QuestState newState)
    {
        if (_state >= newState)
        {
            Debugger.Log($"Can't progress quest '{Name}' state to previous or same state of {newState}", Debugger.TextColor.Orange);
            return;
        }
        _state = newState;

        if(_state == QuestState.Completed)
        {
            EventManager.OnQuestCompleted.Raise(this, this);
        }
    }

    public void ResetState()
    {
        _state = QuestState.Unknown;
    }

    #endregion
}
