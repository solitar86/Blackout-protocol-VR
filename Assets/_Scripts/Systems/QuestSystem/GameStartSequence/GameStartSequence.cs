using System.Collections;
using UnityEngine;

public class GameStartSequence : MonoBehaviour
{
    [SerializeField] private float _startDelay = 1.5f;
    [SerializeField] private Sound _gameStartSFXSequence;
    [SerializeField] private Transform _startSoundPosition;
    [SerializeField] private ConversationSO _levelStartConversation;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(_startDelay);
        AudioPlayer.PlaySoundAtPoint(this, _gameStartSFXSequence, _startSoundPosition.position, false, true);
        yield return new WaitForSeconds(_gameStartSFXSequence.Clip.length);
        ConversationManager.PlayConversation(_levelStartConversation);
    }
}
