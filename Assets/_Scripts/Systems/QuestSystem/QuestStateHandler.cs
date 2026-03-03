using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles updating Quest states when calls from QuestProgressor objects call events
/// Also responsible for starting an appropriate conversation when the player
/// uses the walkie talkie object based on gamestate.
/// </summary>
public class QuestStateHandler : MonoBehaviour
{
    [SerializeField] private List<QuestSO> questList = new();

    [Space(15)]
    [Header("Oneshot or repeating conversations")]
    [SerializeField] ConversationSO _firstRadioConversation;

    //Used to avoid reptition in hint conversations
    private ConversationSO _lastPlaydConversation;

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
        if (ConversationManager.IsPlayingConversation == true) return;

        if (_isFirstRadioConversation)
        {
            StartFirstRadioConversation();
            _lastPlaydConversation = _firstRadioConversation;
            return;
        }

        ConversationSO hintConversation = TryGetQuestStateConversation();
        if (hintConversation != null)
        {
            PlayThisConversation(hintConversation);
            return;
        }

        // Here we would figure out what conversation to call based on quest states.
    }

    private ConversationSO TryGetQuestStateConversation()
    {
        var startedQuests = questList.FindAll(q => q.State == QuestState.Started);

        if (startedQuests.Count == 1)
        {
            // Player has attempted a quest but hasn't finished it.
            // This is a priority hint to give

            return null;
        }
        else if (startedQuests.Count > 1)
        {
            // Player has several started quests. Pick on and play hint conversation

            return null;
        }


        var discoveredQuests = questList.FindAll(q => q.State == QuestState.Discovered);

        if (discoveredQuests.Count == 1)
        {
            // Player has attempted a quest but hasn't finished it.
            // This is a priority hint to give

            return null;
        }
        else if (discoveredQuests.Count > 1)
        {
            // Player has several started quests. Pick on and play hint conversation

            return null;
        }

        // Player has no started or discovered quests but is requesting a hint
        // or maybe just wants to talk to the NPC. Handle that situation

        // DO SOMETHING:
        return null;
    }

    private void PlayThisConversation(ConversationSO conversation)
    {
        ConversationManager.PlayConversation(conversation);
        _lastPlaydConversation = conversation;
    }


    #region Quest sprecific functions

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
            if (quest != null)
            {
                quest.ResetState();
            }
        }
    }
    #endregion

    #endregion

    #region Helpers and One shots

    private void StartFirstRadioConversation()
    {
        _isFirstRadioConversation = false;
        ConversationManager.OverrideCurrentConversationWith(_firstRadioConversation);
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
