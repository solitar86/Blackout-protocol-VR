using UnityEngine;

public class PlayConversationFromUnityEvent : MonoBehaviour
{
    [SerializeField] private ConversationSO _conversationToPlay;
    [SerializeField] private bool _allowRepeat = false;
    [SerializeField] private float _preDelay = 0f;
    [SerializeField] private float _minDelayBetweenRepeats = 0f;
    private bool _hasPlayed = false;

    private float _nextTimeAllowPlay = 0f;


    public void PlayConversation()
    {
        if (_nextTimeAllowPlay > Time.time) return;
        if (_hasPlayed == true && _allowRepeat == false) return;

        this.CallWithDelay(() =>
        {
            ConversationManager.PlayConversation(_conversationToPlay);

        }, _preDelay);

        _hasPlayed = true;
        _nextTimeAllowPlay = Time.time + _minDelayBetweenRepeats;
    }
}
