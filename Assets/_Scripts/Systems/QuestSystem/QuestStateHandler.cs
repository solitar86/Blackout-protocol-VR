using System.Collections.Generic;
using UnityEngine;

public class QuestStateHandler : MonoBehaviour
{
    [SerializeField] private List<QuestSO> questList;

    #region UnityCallbacks
    private void OnEnable()
    {
        EventManager.OnProgressQuest.AddListener(this, ProggressQuestTate);
        ResetAllQuestsToStartingState();
    }
    private void OnDisable()
    {
        EventManager.OnProgressQuest.RemoveListener(this, ProggressQuestTate);
        ResetAllQuestsToStartingState();
    }
    #endregion

    public void ProggressQuestTate(QuestProgressionStep progressioInfo)
    {
        QuestSO quest = questList.Find(quest => quest.name == progressioInfo.Quest.name);
        if (quest == null)
        {
            Debugger.LogWarning($"Quest with name {name} could not be found");
            return;
        }
        quest.ChangeStateTo(progressioInfo.ProgressionState);
        EventManager.OnAnyQuestWasProgressed.Raise(this, -1);
    }

    /// <summary>
    /// This is a failtsafe that is mostly needed in the editor
    /// This should not be a problem during normal gameplay.
    /// Unless the player restarts the game (TODO FIX)
    /// </summary>
    private void ResetAllQuestsToStartingState()
    {
        foreach (var quest in questList)
        {
            quest.ResetState();
        }
    }
}

public enum QuestState
{
    Unknown = 0,
    Discovered = 1,
    Started = 2,
    Completed = 3,
}

[System.Serializable]
public class QuestProgressionStep
{
    public QuestSO Quest;
    public QuestState ProgressionState;
}
