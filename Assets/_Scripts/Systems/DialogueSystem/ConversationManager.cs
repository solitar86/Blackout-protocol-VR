using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance;
    private Queue<ConversationSO> _conversationQueue = new();
    private Queue<DialogueSO> _dialogueQueue = new();
    private static DialogueSO _currentPlayingDialogue;
    private List<GameObject> _currentDialogueAudioObjects = new();
    private static bool _isPlayingConversation = false;
    private static Transform _playerTransform;
    private static Transform _radioTransform;
    [Space(50)]
    [SerializeField] ConversationSO _testDialogue;

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }
    #endregion
    
    /// <summary>
    /// Play a conversation.
    /// </summary>
    /// <param name="conversation"></param>
    public static void PlayConversation(ConversationSO conversation)
    {
        if (Instance == null) return;
        if (conversation == null)
        {
            Debugger.Log("Null conversation sent to conversation manager", Debugger.TextColor.LightRed);
            return;
        }
        if(conversation.DialogueArray == null || conversation.DialogueArray.Length == 0)
        {
            Debugger.Log("Null dialogue sent to Conversation manager", Debugger.TextColor.LightRed);
            return;
        }

        Debugger.Log("Start Conversation is running", Debugger.TextColor.Orange);

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
        conversation.OnCompleteAction = () =>
        {
            PlayConversationOnLoop(conversation);
        };
        PlayConversation(conversation);
    }
    /// <summary>
    /// This can be used to interrupt a looping conversation
    /// t.ex from the walkie talkie to start and actual important
    /// conversation with gameplay importance.
    /// </summary>
    /// <param name="conversation"></param>
    public static void OverrideCurrentConversationWith(ConversationSO conversation)
    {
        ForceStopConversations();
        PlayConversation(conversation);
    }
    private static void ForceStopConversations()
    {
        if (Instance != null)
        {
            Instance.StopAllCoroutines();
            Instance._conversationQueue.Clear();
            Instance._dialogueQueue.Clear();
            StopCurrentPlayingDialoguesAndEmptyList();
            EventManager.OnConversationEnded.Raise(Instance, "All");
            _isPlayingConversation = false;
        }
    }

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
    }
    private IEnumerator PlaySingleConversation(ConversationSO conversation)
    {
        Queue<DialogueSO> currentDialoguesQueue = new();

        for (int i = 0; i < conversation.DialogueArray.Length; i++)
        {
            currentDialoguesQueue.Enqueue(conversation.DialogueArray[i]);
        }

        EventManager.OnConversationStarted.Raise(this, conversation.name);

        while(currentDialoguesQueue.Count > 0)
        {
            yield return StartCoroutine(PlaySingleDialogue(currentDialoguesQueue.Dequeue()));
        }

        _currentPlayingDialogue = null;
        EventManager.OnConversationEnded.Raise(Instance, conversation.name);
    }
    private IEnumerator PlaySingleDialogue(DialogueSO dialogue)
    {
        _currentPlayingDialogue = dialogue;
        float fullDialogueDuration = dialogue.GetDialogueDuration();
        float audioLenght = dialogue.GetAudioDuration();
        Transform audioObjectParent = GetParentForDialogueAudio(dialogue.GetSpeaker());

        TriggerDialogueStartEvent(dialogue.GetSpeaker(), dialogue.GetDialogueDuration());

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
            TriggerDialogueEndEvent(dialogue.GetSpeaker());
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
            }
            return _radioTransform;
        }
    }
    private void TriggerDialogueStartEvent(Speaker speaker, float dialogDuration)
    {
        if (speaker is Speaker.Player)
        {
            EventManager.OnDialogueStart_Player.Raise(this, dialogDuration);
            return;
        }
        EventManager.OnDialogueStart_Radio.Raise(this, -1);
    }
    private void TriggerDialogueEndEvent(Speaker speaker)
    {
        if (speaker is Speaker.Player)
        {
            EventManager.OnDialogueStop_Player.Raise(this, -1);
            return;
        }
        EventManager.OnDialogueStop_Radio.Raise(this, -1);
    }
    private static void StopCurrentPlayingDialoguesAndEmptyList()
    {
        foreach (var item in Instance._currentDialogueAudioObjects)
        {
            if(item != null)
            {
                Destroy(item);
            }
        }

        if (_currentPlayingDialogue != null)
        {
            Instance.TriggerDialogueEndEvent(_currentPlayingDialogue.GetSpeaker());
        }

        _currentPlayingDialogue = null;
        Instance._currentDialogueAudioObjects.Clear();
    }
    #endregion
}
