using System;
using UnityEditor.Rendering;
using UnityEngine;

[CreateAssetMenu(fileName = "ConversationSO", menuName = "ConversationSO")]
public class ConversationSO : ScriptableObject
{
    [SerializeField] private string _name = "Unnamed";
    [SerializeField] private DialogueSO[] _conversation;

    public DialogueSO[] DialogueArray => _conversation;

    /// <summary>
    /// This can be used to do something at
    /// the end of a conversation as if the NPC did it etc.
    /// </summary>
    public Action OnCompleteAction = null;
}
