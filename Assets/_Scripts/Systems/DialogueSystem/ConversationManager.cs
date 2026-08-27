using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Responsible for queuing and playing collections of 
/// DialogueSO objects which contain audio to be played
/// in a sequence. Handles what that sound sounds like
/// and where it is played.
/// </summary>
public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance;
    private Queue<ConversationSO> _conversationQueue = new();
    private Queue<DialogueSO> _dialogueQueue = new();
    private static DialogueSO _currentPlayingDialogue;
    private static ConversationSO _currentPlayingConversation;
    private List<GameObject> _currentDialogueAudioObjects = new();
    private static bool _isPlayingConversation = false;
    private static bool _playerIsSpeaking = false;
    private static bool _NPCIsSpeaking = false;
    private static Transform _playerTransform;
    private static Transform _radioTransform;
    [Space(50)]
    [SerializeField] ConversationSO _testDialogue;

    public static bool IsPlayingConversation => _isPlayingConversation;
    public static bool PlayerIsSpeaking => _playerIsSpeaking;
    public static bool NPCIsSpeaking => _NPCIsSpeaking;


    #region Unity Callbacks
    private void Awake()
    {
        _isPlayingConversation = false;
        _playerIsSpeaking = false;
        _NPCIsSpeaking = false;
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    private void OnDisable()
    {
        _isPlayingConversation = false;
        _playerIsSpeaking = false;
        _NPCIsSpeaking = false;
    }
    #endregion

    #region Core Functions
    /// <summary>
    /// Play a conversation.
    /// </summary>
    /// <param name="conversation"></param>
    public static void PlayConversation(ConversationSO conversation)
    {
        if (Instance == null)
        {
            Debugger.Log("´Conversation Manager instance was null", Debugger.TextColor.Orange);
            return;
        }
        if (conversation == null)
        {
            Debugger.Log("Null conversation sent to conversation manager", Debugger.TextColor.LightRed);
            return;
        }
        if(conversation.DialogueArray == null || conversation.DialogueArray.Length == 0)
        {
            Debugger.Log("Conversation sent to conversation manager has no dialogues: " + conversation.Name, Debugger.TextColor.LightRed);
            return;
        }

        Instance._conversationQueue.Enqueue(conversation);

        if(_isPlayingConversation == false)
        {
            // Currently not playing conversation, start playing. 
            Instance.StartCoroutine(Instance.PlayQueudConversations());
        }
    }
    /// <summary>
    /// This is mostly meant for a situation where we want the player
    /// to be "lured" to the walkie talkie by the NPC calling for them.
    ///
    ///NOTE: This overrides the OnComplete event of the conversation to execute the loop
    /// </summary>
    /// <param name="convoToLoop"></param>
    public static void PlayConversationOnLoop(ConversationSO conversation)
    {
        if (conversation == null)
        {
            Debugger.Log("Null conversation sent to conversation manager to loop", Debugger.TextColor.LightRed);
            return;
        }
        conversation.OnCompleteAction = () =>
        {
            PlayConversation(conversation);
        };
        PlayConversation(conversation);
    }
    /// <summary>
    /// This can be used to interrupt a looping conversation
    /// t.ex from the walkie talkie to start and actual important
    /// conversation with gameplay importance.
    /// </summary>
    /// <param name="conversation"> Conversation to start after interrupt. </param>
    public static void OverrideCurrentConversationWith(ConversationSO conversation)
    {
        Debugger.Log("Overriding current conversation", Debugger.TextColor.Orange);
        ForceStopConversations();
        PlayConversation(conversation);
    }

    #endregion

    #region Conversation Coroutines
    private IEnumerator PlayQueudConversations()
    {
        _isPlayingConversation = true;
        while (_conversationQueue.Count > 0)
        {
            ConversationSO currentConvo = _conversationQueue.Peek();
            yield return StartCoroutine(Instance.PlaySingleConversation(_conversationQueue.Dequeue()));
            currentConvo.OnCompleteAction?.Invoke();
        }
        StopCurrentPlayingDialoguesAndEmptyList();
        _isPlayingConversation = false;
        _playerIsSpeaking = false;
        _NPCIsSpeaking = false;
    }
    private IEnumerator PlaySingleConversation(ConversationSO conversation)
    {

        Queue<DialogueSO> currentDialoguesQueue = new();

        for (int i = 0; i < conversation.DialogueArray.Length; i++)
        {
            currentDialoguesQueue.Enqueue(conversation.DialogueArray[i]);
        }

        _currentPlayingConversation = conversation;
        EventManager.OnConversationStarted.Raise(this, conversation.name);

        while(currentDialoguesQueue.Count > 0)
        {
            yield return StartCoroutine(PlaySingleDialogue(currentDialoguesQueue.Dequeue()));
        }

        _currentPlayingConversation = null;
        _currentPlayingDialogue = null;
        EventManager.OnConversationEnded.Raise(Instance, conversation.name);
    }
    private IEnumerator PlaySingleDialogue(DialogueSO dialogue)
    {
        _currentPlayingDialogue = dialogue;
        float fullDialogueDuration = dialogue.GetDialogueDuration();
        float audioLenght = dialogue.GetAudioDuration();
        Transform audioObjectParent = GetParentForDialogueAudio(dialogue.GetSpeaker());

        TriggerDialogueStartEvent(dialogue);

        // Parent the audioplayer to the object so it follows
        // The radio if it's playing on the radio.
        bool spatialize = dialogue.GetSpeaker() == Speaker.Radio;
        var audioObject = AudioPlayer.PlaySoundAtPoint(this, dialogue.DialogueAudio, audioObjectParent.position, false, spatialize);
        audioObject.transform.SetParent(audioObjectParent);
        _currentDialogueAudioObjects.Add(audioObject);

        // This way we trigger audio on the walkie talkie when the speaker stops even
        // if there is an assigned delay for the next dialogue line or speaker.
        // This may be unnecessarily complex but lets keep it for now.
        float buffer = 0.05f;
        this.CallWithDelay(() =>
        {
            TriggerDialogueEndEvent(dialogue);
        }, audioLenght - buffer);

        // Do not progress to next dialgue until audio + delay has elapsed.
        yield return new WaitForSeconds(fullDialogueDuration);

    }
    #endregion

    #region Helpers etc.
    private Transform GetParentForDialogueAudio(Speaker speaker)
    {
        if(speaker is Speaker.Player)
        {
            if (_playerTransform == null)
            {
                _playerTransform = FindFirstObjectByType<Player>().transform;
            }
            return _playerTransform;
        }
        else
        {
            if (_radioTransform == null)
            {
                _radioTransform = FindFirstObjectByType<WalkieTalkie>().transform;
                if(_radioTransform == null)
                {
                    Debugger.LogError("Scene has no walkie talkie, conversations won't work");
                    Debugger.Break();
                }
            }
            return _radioTransform;
        }
    }
    private void TriggerDialogueStartEvent(DialogueSO dialogueSO)
    {
        if (dialogueSO.GetSpeaker() is Speaker.Player)
        {
            _NPCIsSpeaking = false;
            _playerIsSpeaking = true;
            EventManager.OnDialogueStart_Player.Raise(this, dialogueSO);
            return;
        }
        _NPCIsSpeaking = true;
        _playerIsSpeaking = false;
        EventManager.OnDialogueStart_Radio.Raise(this, dialogueSO);
    }
    private void TriggerDialogueEndEvent(DialogueSO dialogue)
    {
        if (dialogue.GetSpeaker() is Speaker.Player)
        {
            _playerIsSpeaking = false;
            EventManager.OnDialogueStop_Player.Raise(this, dialogue);
            return;
        }
        _NPCIsSpeaking = false;
        EventManager.OnDialogueStop_Radio.Raise(this, dialogue);
    }
    private static void ForceStopConversations()
    {
        if (Instance != null)
        {
            Debugger.Log("Force stopping all conversations", Debugger.TextColor.Orange);
            Instance.StopAllCoroutines();
            Instance._conversationQueue.Clear();
            Instance._dialogueQueue.Clear();
            StopCurrentPlayingDialoguesAndEmptyList();
            EventManager.OnConversationEnded.Raise(Instance, "All");
            _isPlayingConversation = false;
            _playerIsSpeaking = false;
            _NPCIsSpeaking = false;
        }
    }
    /// <summary>
    /// This is a clean-up function for when conversations end or are force-stopped.
    /// </summary>
    private static void StopCurrentPlayingDialoguesAndEmptyList()
    {
        foreach (var dialogueAudioObject in Instance._currentDialogueAudioObjects)
        {
            if(dialogueAudioObject != null)
            {
                Destroy(dialogueAudioObject);
            }
        }

        if (_currentPlayingDialogue != null)
        {
            Instance.TriggerDialogueEndEvent(_currentPlayingDialogue);
        }

        _currentPlayingDialogue = null;
        Instance._currentDialogueAudioObjects.Clear();
    }
    public static void ResetStaticVariables()
    {
        _isPlayingConversation = false;
        _playerIsSpeaking = false;
        _NPCIsSpeaking= false;
    }
    
    /// <summary>
    /// This is currently only used for developer functions to skip conversations.
    /// </summary>
    public static void SkipCurrentConversation()
    {
        if (Instance == null) return;
        //if(_currentPlayingDialogue != null)_currentPlayingConversation.OnCompleteAction?.Invoke();

        //if (Instance._conversationQueue.Count != 0)
        //{
        //    while(Instance._conversationQueue.Count > 0)
        //    {
        //        var convo = Instance._conversationQueue.Dequeue();
        //        convo.OnCompleteAction?.Invoke();
        //    }
        //}
        ForceStopConversations();
    }
    #endregion
}
