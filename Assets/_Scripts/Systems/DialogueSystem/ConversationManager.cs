using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance;
    public Queue<ConversationSO> _conversationQueue = new();
    public Queue<DialogueSO> _dialogueQueue = new();
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

    public static void StartConversation(ConversationSO conversation)
    {
        if (Instance == null) return;
        if(conversation == null) return;
        if(conversation.DialogueArray == null || conversation.DialogueArray.Length == 0)
        {
            Debugger.LogWarning("Null dialogue sent to Conversation manager");
            return;
        }
  
        Instance._conversationQueue.Enqueue(conversation);

        if(_isPlayingConversation == false)
        {
            // Currently not playing conversation, start playing. 
           Instance.StartCoroutine(Instance.RunConversations());
        }
    }

    public static void OverrideCurrentConversationWith(ConversationSO conversation)
    {
        if(Instance != null)
        {
            Instance.StopAllCoroutines();
            Instance._conversationQueue.Clear();
            Instance._dialogueQueue.Clear();
        }

        StartConversation(conversation);
    }

    #region Conversation Coroutines
    private IEnumerator RunConversations()
    {
        _isPlayingConversation = true;


        while (_conversationQueue.Count > 0)
        {
            ConversationSO currentConvo = _conversationQueue.Peek();
            yield return StartCoroutine(Instance.PlayConversation(_conversationQueue.Dequeue()));
            currentConvo.OnCompleteAction?.Invoke();
        }


        _isPlayingConversation = false;
    }
    private IEnumerator PlayConversation(ConversationSO conversation)
    {
        Queue<DialogueSO> currentDialogue = new();

        for (int i = 0; i < conversation.DialogueArray.Length; i++)
        {
            currentDialogue.Enqueue(conversation.DialogueArray[i]);
        }

        while(currentDialogue.Count > 0)
        {
            yield return StartCoroutine(PlaySingleDialogue(currentDialogue.Dequeue()));
        }
    }
    private IEnumerator PlaySingleDialogue(DialogueSO dialogue)
    {
        float fullDialogueDuration = dialogue.GetDialogueDuration();
        float audioLenght = dialogue.GetAudioDuration();
        Transform audioObjectParent = GetParentForDialogueAudio(dialogue.GetSpeaker());

        TriggerDialogueStartEvent(dialogue.GetSpeaker(), dialogue.GetDialogueDuration());

        // Parent the audioplayer to the object so it follows
        // The radio if it's playing on the radio.
        var audioObject = AudioPlayer.PlaySoundAtPoint(this, dialogue.DialogueAudio, audioObjectParent.position, false, true);
        audioObject.transform.SetParent(audioObjectParent);

        // This way we trigger audio on the walkie talkie when the speaker stops even
        // if there is an assigned delay for the next dialogue line or speaker.
        // This may be unnecessarily complex but lets keep it for now.
        this.CallWithDelay(() =>
        {
            TriggerDialogueEndEvent(dialogue.GetSpeaker());
        }, audioLenght - 0.05f);

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

    public void PlayTestDialogue()
    {
        StartConversation(_testDialogue);
    }
    #endregion
}
