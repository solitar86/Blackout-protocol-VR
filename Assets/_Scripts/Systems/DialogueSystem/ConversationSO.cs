using System;
using UnityEditor.Rendering;
using UnityEngine;

[CreateAssetMenu(fileName = "ConversationSO", menuName = "ConversationSO")]
public class ConversationSO : ScriptableObject
{
    [SerializeField] private string _name = "Unnamed";
    [SerializeField] private DialogueSO[] _conversation;

    public string Name => _name;
    public DialogueSO[] DialogueArray => _conversation;

    /// <summary>
    /// This can be used to do something at
    /// the end of a conversation as if the NPC did it etc.
    /// </summary>
    public Action OnCompleteAction = null;

    /// <returns>The sum of all dialogue audios and their post delays</returns>
    public float GetConversationDuration()
    {
        float sum = 0f;
        for (int i = 0; i < _conversation.Length; i++)
        {
            sum += _conversation[i].GetDialogueDuration();
        }
        return sum;
    }
}
