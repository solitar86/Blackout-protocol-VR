using UnityEditor.Rendering;
using UnityEngine;

[CreateAssetMenu(fileName = "ConversationSO", menuName = "ConversationSO")]
public class ConversationSO : ScriptableObject
{
    [SerializeField] private string _name = "Unnamed";
    [SerializeField] private DialogueSO[] _conversation;

    public DialogueSO[] DialogueArray => _conversation;
}
