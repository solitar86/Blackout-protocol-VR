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

    private float _turnVODelay = 0.25f;
    private float _curseDelay = 1.5f;

    #region Unity Callbacks
    private void Start()
    {
        EventManager.OnPlayerCurse.AddListener(this, PlayerSayCurseWord);
        EventManager.OnPlayerObjectIDVOShouldPlay.AddListener(this, PlayTouchIDVoiceLine);
        SnapTurnProvider.OnPlayerSnapTurn += HandlePlayerTurn;
    }
    private void OnDisable()
    {
        EventManager.OnPlayerCurse.RemoveListener(this, PlayerSayCurseWord);
        EventManager.OnPlayerObjectIDVOShouldPlay.RemoveListener(this, PlayTouchIDVoiceLine);
        SnapTurnProvider.OnPlayerSnapTurn -= HandlePlayerTurn;
    }

    #endregion
    
    private void HandlePlayerTurn(bool isRightTurn)
    {
        if (isRightTurn == false)
        {
            if (_leftTurnVO != null && _leftTurnVO.SoundArray != null && _leftTurnVO.SoundArray.Length > 0)
            {
                PlayPlayerInnerMonologueWithDelay(_leftTurnVO, _turnVODelay);
            }
            return;
        }

        if (_rightTurnVO != null && _rightTurnVO.SoundArray != null && _rightTurnVO.SoundArray.Length > 0)
        {
            PlayPlayerInnerMonologueWithDelay(_rightTurnVO, _turnVODelay);
        }

    }
    private void PlayerSayCurseWord(int severity)
    {
        if (_curseWordsVO != null && _curseWordsVO.SoundArray != null && _curseWordsVO.SoundArray.Length > 0)
        {
            PlayPlayerInnerMonologueWithDelay(_curseWordsVO, _curseDelay);
        }
    }
    private void PlayTouchIDVoiceLine(Sound IDVOSound)
    {
        PlayPlayerInnerMonologueWithDelay(IDVOSound, PlayerSettings.Developer.IdentifyVODelay);
    }
    private void PlayPlayerInnerMonologueWithDelay(SoundArrayHolder soundHolder, float delay)
    {
        this.CallWithDelay(() =>
        {
            bool spatialize = false;
            bool pitchVary = false;
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        soundHolder.SoundArray,
                                                        transform.position,
                                                        soundHolder.LastPlayedSound,
                                                        pitchVary,
                                                        spatialize);
        }, delay);
    }
    private void PlayPlayerInnerMonologueWithDelay(Sound soundToPlay, float delay)
    {
        this.CallWithDelay(() =>
        {
            bool spatialize = false;
            bool pitchVary = false;
            AudioPlayer.PlaySoundAtPoint(this,
                                        soundToPlay,
                                        transform.position,
                                        pitchVary,
                                        spatialize);
        }, delay);
    }
}
