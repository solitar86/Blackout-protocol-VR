using System.Collections.Generic;
using UnityEngine;

public class QuestStateHandler : MonoBehaviour
{
    [SerializeField] private List<QuestSO> questList = new();

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
        if (questList == null || questList.Count == 0)
        {
            Debugger.Log("No Quests in Quest list", Debugger.TextColor.LightRed);
            return;
        }

        if (progressioInfo?.Quest == null)
        {
            Debugger.LogWarning("Progression info or Quest is null.");
            return;
        }

        var questName = progressioInfo.Quest.name;
        QuestSO quest = questList.Find(q => q.name == questName);

        if (quest == null)
        {
            Debugger.LogWarning($"Quest with name {questName} could not be found");
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
            if(quest != null)
            {
                quest.ResetState();
            }
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
