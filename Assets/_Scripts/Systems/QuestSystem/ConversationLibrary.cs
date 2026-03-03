using UnityEngine;

public class ConversationLibrary : MonoBehaviour
{
    [SerializeField] public ConversationLibraryEntry[] _conversations;

    public void GetConversationByStringName(string name)
    {
     //TODO: Implement.
    }
}

[System.Serializable]
public class ConversationLibraryEntry
{
    public string Name;
    public string ConversationSO;
}
