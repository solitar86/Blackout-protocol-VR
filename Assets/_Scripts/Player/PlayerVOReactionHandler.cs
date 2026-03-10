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
    [SerializeField] private SoundArrayHolder _somethingHereVO;
    [SerializeField] private SoundArrayHolder _bumpIDUnknownObstacleVO;
    [SerializeField] private SoundArrayHolder _cantCarryVO;

    private float _turnVODelay = 0.25f;
    private float _curseDelay = 1.5f;
    private float _itemDetectedDelay = 1f;
    private float _cantCarryDelay = 0.25f;


    #region Unity Callbacks
    private void OnEnable()
    {
        EventManager.OnPlayerCurse.AddListener(this, PlayerSayCurseWord);
        EventManager.OnCantCarryObject.AddListener(this, PlayerSayCantCarry);
        EventManager.OnPlayerObjectIDVOShouldPlay.AddListener(this, PlayTouchIDVoiceLine);
        EventManager.OnPlayerBumpIDVOShouldPlay.AddListener(this, PlayBumpIDVoiceLine);
        EventManager.OnInteractableDetectedOnSurface.AddListener(this, PlayItemDetectedVoiceLine);
        //SnapTurnProvider.OnPlayerSnapTurn += HandlePlayerTurn;
        CustomSnapTurnProviderWrapper.OnPlayerSnapTurn += HandlePlayerTurn;
    }


    private void OnDisable()
    {
        EventManager.OnPlayerCurse.RemoveListener(this, PlayerSayCurseWord);
        EventManager.OnCantCarryObject.RemoveListener(this, PlayerSayCantCarry);
        EventManager.OnPlayerObjectIDVOShouldPlay.RemoveListener(this, PlayTouchIDVoiceLine);
        EventManager.OnPlayerBumpIDVOShouldPlay.RemoveListener(this, PlayBumpIDVoiceLine);
        EventManager.OnInteractableDetectedOnSurface.RemoveListener(this, PlayItemDetectedVoiceLine);
        // SnapTurnProvider.OnPlayerSnapTurn -= HandlePlayerTurn;
        CustomSnapTurnProviderWrapper.OnPlayerSnapTurn -= HandlePlayerTurn;

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
    private void PlayerSayCantCarry(int value)
    {
        PlayPlayerInnerMonologueWithDelay(_cantCarryVO, _cantCarryDelay);
    }

    private void PlayTouchIDVoiceLine(Sound IDVOSound)
    {

        PlayPlayerInnerMonologueWithDelay(IDVOSound, PlayerSettings.Developer.IdentifyVODelay);
    }
    private void PlayBumpIDVoiceLine(Sound sound)
    {
        // This null check is here because the Sound comes from another object.
        if(sound != null && sound.Clip != null)
        {
            PlayPlayerInnerMonologueWithDelay(sound, PlayerSettings.Developer.IdentifyVODelay);
            return;
        }

        PlayPlayerInnerMonologueWithDelay(_bumpIDUnknownObstacleVO, PlayerSettings.Developer.IdentifyVODelay);
    }
    private void PlayItemDetectedVoiceLine(int obj)
    {
        PlayPlayerInnerMonologueWithDelay(_somethingHereVO, _itemDetectedDelay);
    }
    private void PlayPlayerInnerMonologueWithDelay(SoundArrayHolder soundHolder, float delay)
    {
        if (ConversationManager.IsPlayingConversation == true) return; // CHANGE THIS: playtest notes

        //if(soundHolder?.SoundArray?.Length > 0)
        if (soundHolder != null && soundHolder.SoundArray != null && soundHolder.SoundArray.Length > 0)
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
            return;
        }
        else
        {
            Debugger.Log("Inner monologue with delay was called with null or empty sound");
        }
    }
    private void PlayPlayerInnerMonologueWithDelay(Sound soundToPlay, float delay)
    {
        if (ConversationManager.IsPlayingConversation == true) return; // CHANGE THIS: playtest notes

        if(soundToPlay != null && soundToPlay.Clip != null)
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
        else
        {
            Debugger.Log("Inner monologue was called with null or empty sound");
        }
    }
}
