using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles updating queststates when calls from QuestProgressor objects call events
/// Also responsible for starting an appropriate conversation when the player
/// uses the walkie talkie object based on gamestate.
/// </summary>
public class QuestStateHandler : MonoBehaviour
{
    [SerializeField] private List<QuestSO> questList = new();

    private bool _isFirstRadioConversation = true;
    #region UnityCallbacks
    private void OnEnable()
    {
        EventManager.OnProgressQuest.AddListener(this, ProggressQuestTate);
        EventManager.OnPlayerTryStartConversation.AddListener(this, HandlePlayerTryStartConversation);
        ResetAllQuestsToStartingState();
    }
    private void OnDisable()
    {
        EventManager.OnProgressQuest.RemoveListener(this, ProggressQuestTate);
        EventManager.OnPlayerTryStartConversation.RemoveListener(this, HandlePlayerTryStartConversation);
        ResetAllQuestsToStartingState();
    }

    #endregion


    #region Core functions
    private void HandlePlayerTryStartConversation(int value)
    {
        if (_isFirstRadioConversation)
        {
            StartFirstRadioConversation();
            return;
        }

        // Here we would figure out what conversation to call based on quest states.
    }

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
    #endregion

    #region Helpers and One shots

    private void StartFirstRadioConversation()
    {
        _isFirstRadioConversation = false;
        //ConversationManager.PlayConversation();
    }

    #endregion
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
