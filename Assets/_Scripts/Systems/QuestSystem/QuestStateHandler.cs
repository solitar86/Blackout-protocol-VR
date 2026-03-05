using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
    [SerializeField] ConversationSO[] _genericStartConversations;
    [SerializeField] ConversationSO _nothingToSayConversation;
    [SerializeField] ConversationSO _firstRadioConversation;
    [SerializeField] ConversationSO _noQuestStartedHint;

    //Used to avoid reptition in hint conversations
    private ConversationSO _lastPlaydConversation;

    private bool _hasNotSpokenOnRadioYet = true;

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
        // Handle the case that player is speaking for the first time.
        if (_hasNotSpokenOnRadioYet == true)
        {
            StartFirstRadioConversation();
            _lastPlaydConversation = _firstRadioConversation;
            return;
        }

        //TODO Handle situatioon where during gameplay
        // NPC is calling player to pick-up radio.
        // That currently can't be interrupted with this event response.

        if (ConversationManager.IsPlayingConversation == true) return;

        // Simple player call NPC conversation to start off with.
        PlayThisConversation(_genericStartConversations[Random.Range(0, _genericStartConversations.Length)], false);

        ConversationSO hintConversation = TryGetQuestStateConversationOrDefault();
        if (hintConversation != null)
        {
            PlayThisConversation(hintConversation);
            return;
        }

        Debugger.Log("Player Try Start Conversation was null", Debugger.TextColor.Orange);
       
    }

    private ConversationSO TryGetQuestStateConversationOrDefault()
    {
        // Started quests - handle here. Important as it means
        // Player has tried the right thing, but stopped or given up.
        var startedQuests = questList.FindAll(q => q.State == QuestState.Started);

        if (startedQuests.Count == 1)
        {
            // Player has attempted a quest but hasn't finished it.
            // This is a priority hint to give
            return startedQuests[0].GetStartedHintConversation();
        }
        else if (startedQuests.Count > 1)
        {
            // Player has several started quests. Pick on and play hint conversation
            QuestSO selectedQuest;
            do
            {
                selectedQuest = startedQuests[Random.Range(0, startedQuests.Count)];
            } while (selectedQuest.GetStartedHintConversation() == _lastPlaydConversation);
            return selectedQuest.GetStartedHintConversation();
        }

        // Discovered quests - handle here. These are less priority if 
        // Player has started quests.
        var discoveredQuests = questList.FindAll(q => q.State == QuestState.Discovered);

        if (discoveredQuests.Count == 1)
        {
            // Player has discovered a quest. 
            return discoveredQuests[0].GetDiscoveredHintConversation();
        }
        else if (discoveredQuests.Count > 1)
        {
            // Player has several started quests. Pick on and play hint conversation
            QuestSO selectedQuest;
            do
            {
                selectedQuest = discoveredQuests[Random.Range(0, discoveredQuests.Count)];
            } while (selectedQuest.GetDiscoveredHintConversation() == _lastPlaydConversation);
            return selectedQuest.GetDiscoveredHintConversation();
        }

        // Player has no started or discovered quests but is requesting a hint
        // or maybe just wants to talk to the NPC. Handle that situation
        if(_lastPlaydConversation != _noQuestStartedHint)
        {
            return _noQuestStartedHint;
        }

        return _nothingToSayConversation;
    }
    /// <summary>
    /// Tells Conversation manager to play conversation.
    /// </summary>
    /// <param name="conversation">Conversation to play</param>
    /// <param name="overWriteLastPlayedConversation">So Generic Start conversations don't overwrite previously played convo</param>
    private void PlayThisConversation(ConversationSO conversation, bool overWriteLastPlayedConversation = true)
    {
        ConversationManager.PlayConversation(conversation);
        if(overWriteLastPlayedConversation) _lastPlaydConversation = conversation;
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
        _hasNotSpokenOnRadioYet = false;
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
