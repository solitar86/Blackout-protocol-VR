using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
/// <summary>
/// Many VO's are held by objects themselves, this class deals with non-specific
/// VO reactioons such as "Ouch!" and curse words etc.
/// </summary>
public class PlayerVOReactionHandler : MonoBehaviour
{
    [SerializeField] private SoundArrayHolder _leftTurnVO, _rightTurnVO;
    [SerializeField] private SoundArrayHolder _curseWordsVO;

    void Start()
    {
        EventManager.OnPlayerCurse.AddListener(this, PlayerSayCurseWord);
        SnapTurnProvider.OnPlayerSnapTurn += HandlePlayerTurn;
    }
    private void OnDisable()
    {
        EventManager.OnPlayerCurse.RemoveListener(this, PlayerSayCurseWord);
        SnapTurnProvider.OnPlayerSnapTurn -= HandlePlayerTurn;
    }

    private void HandlePlayerTurn(bool isRightTurn)
    {
        if(isRightTurn == false)
        {
            if (_leftTurnVO != null && _leftTurnVO.SoundArray != null && _leftTurnVO.SoundArray.Length > 0)
            {
                AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                            _leftTurnVO.SoundArray,
                                                            transform.position,
                                                            _curseWordsVO.LastPlayedSound);
            }
            return;
        }

        if (_rightTurnVO != null && _rightTurnVO.SoundArray != null && _rightTurnVO.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _rightTurnVO.SoundArray,
                                                        transform.position,
                                                        _rightTurnVO.LastPlayedSound);
        }
    }

    private void PlayerSayCurseWord(int severity)
    {
        if(_curseWordsVO != null && _curseWordsVO.SoundArray != null && _curseWordsVO.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _curseWordsVO.SoundArray,
                                                        transform.position,
                                                        _curseWordsVO.LastPlayedSound);
        }
    }
}
