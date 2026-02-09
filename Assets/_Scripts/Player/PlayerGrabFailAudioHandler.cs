using System;
using UnityEngine;

public class PlayerGrabFailAudioHandler : MonoBehaviour
{
    private PlayerHandInteractor _interactor;
    [SerializeField] Sound _onGrabFailSound;

    #region Unity Callbacks
    private void OnEnable()
    {
        if (_interactor == null)
            _interactor = GetComponent<PlayerHandInteractor>();
        _interactor.OnGrabFailed.AddListener(this, HandleOnGrabFailed);
    }

    private void OnDisable()
    {
        if (_interactor == null)
            _interactor = GetComponent<PlayerHandInteractor>();
        _interactor.OnGrabFailed.AddListener(this, HandleOnGrabFailed);
    }
    #endregion
    private void HandleOnGrabFailed(bool isRightHand)
    {
       if(isRightHand == _interactor.Hand.IsRightHand)
        {
            AudioPlayer.PlaySoundAtPoint(this, _onGrabFailSound, transform.position, true);
            return;
        }
    }
}
