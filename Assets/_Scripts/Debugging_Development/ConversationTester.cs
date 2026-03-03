using UnityEngine;

public class ConversationTester : MonoBehaviour
{
    [SerializeField] ConversationSO _conversationToTest;

    [ContextMenu("Play test conversation")]
    public void TestConversation()
    {
        if(Application.isPlaying == false)
        {
            Debugger.LogWarning("Can't Test Conversation in Edit mode");
            return;
        }
        ConversationManager.PlayConversation(_conversationToTest);
    }
}
