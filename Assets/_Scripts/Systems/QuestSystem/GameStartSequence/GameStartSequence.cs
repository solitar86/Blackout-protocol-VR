using System.Collections;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

public class GameStartSequence : MonoBehaviour
{
    [SerializeField] private float _startDelay = 1.5f;
    [SerializeField] private Sound _gameStartSFXSequence;
    [SerializeField] private Transform _startSoundPosition;
    [Tooltip("This can be negative as well.")]
    [SerializeField] private float _delayBeforeFirstConversationStart = 0;
    [SerializeField] private ConversationSO _levelStartConversation;
    [Tooltip("If player doesn't find radio how long until it starts calling player.")]
    [SerializeField] private float _delayBeforeRadioCallPlayer = 5f;
    [SerializeField] private ConversationSO _radioCallPlayerLoopConversation;

    public bool _playerHasActivatedRadio = false;

    public void SetPlayerHasActivatedRadio()
    {
        _playerHasActivatedRadio = true;
    }
    private IEnumerator Start()
    {
        TTSPlayer.AddRepeatableTTS(PlayerSettings.CONTROLS_LIST_TTS_PATH);
        yield return new WaitForSeconds(_startDelay);
        AudioPlayer.PlaySoundAtPoint(this, _gameStartSFXSequence, _startSoundPosition.position, false, true);

        if (_playerHasActivatedRadio == false)
        {
            yield return new WaitForSeconds(_gameStartSFXSequence.Clip.length + _delayBeforeFirstConversationStart);

            ConversationManager.PlayConversation(_levelStartConversation);
            yield return new WaitForSeconds(_levelStartConversation.GetConversationDuration());

            yield return new WaitForSeconds(_delayBeforeRadioCallPlayer);
            // The player might reach the radio during the delay
            // Therefore we check again here if they have done so.
            if (_playerHasActivatedRadio == false)
            {
                ConversationManager.PlayConversationOnLoop(_radioCallPlayerLoopConversation);
            }
        }
    }
}
