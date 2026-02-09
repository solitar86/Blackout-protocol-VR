using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class PlayerFoleyHandler : MonoBehaviour
{
    [SerializeField] Transform _foleySoundPosition;
    [SerializeField] SoundArrayHolder _foleySounds;

    private PlayerFootStepHandler _footstepHandler;

    #region Unity Callbacks
    private void OnEnable()
    {
        if (_footstepHandler == null)
            _footstepHandler = GetComponent<PlayerFootStepHandler>();
        _footstepHandler.OnPlayerTakeFootstep.AddListener(this, HandlePlayerTakeFootStep);

        EventManager.OnPlayerStartMove.AddListener(this, HandlePlayerStartMove);
        SnapTurnProvider.OnPlayerSnapTurn += HandlePlayerMakeSnapTurn;
    }

    private void HandlePlayerMakeSnapTurn(bool wasRightTurn)
    {
        AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                            _foleySounds.SoundArray,
                                            _foleySoundPosition.position,
                                            _foleySounds.LastPlayedSound, true);
    }

    private void OnDisable()
    {
        if (_footstepHandler == null)
            _footstepHandler = GetComponent<PlayerFootStepHandler>();
        _footstepHandler.OnPlayerTakeFootstep.RemoveListener(this, HandlePlayerTakeFootStep);

        EventManager.OnPlayerStartMove.RemoveListener(this, HandlePlayerStartMove);
    }
    #endregion


    private void HandlePlayerTakeFootStep(int value)
    {
        AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                    _foleySounds.SoundArray,
                                                    _foleySoundPosition.position,
                                                    _foleySounds.LastPlayedSound, true);
    }

    private void HandlePlayerStartMove(int value)
    {
        AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                            _foleySounds.SoundArray,
                                            _foleySoundPosition.position,
                                            _foleySounds.LastPlayedSound, true);
    }

}
