using System;
using UnityEngine;

public class PlayerFingerSnapHandler : MonoBehaviour
{
    [SerializeField] private bool _spatializeFingerSnaps = false;
    [SerializeField, Range(0f,1f)] float _echoDelayMultiplier = 0.1f;
    [SerializeField] SoundArrayHolder _fingerSnapSounds;
    [SerializeField] LayerMask _layersToPlayEchoFrom;
    [SerializeField] LayerMask _interactableLayerMask;
    #region UnityCallbacks
    private void OnEnable()
    {
        EventManager.OnTriggerPressed.AddListener(this, OnPlayerPressTrigger);
    }
    private void OnDisable()
    {
        EventManager.OnTriggerPressed.RemoveListener(this, OnPlayerPressTrigger);
    }
    private void OnPlayerPressTrigger(bool isRightHand)
    {
        if (enabled == false) return;
        if (RadialMenuManager.Instance.MenuIsOpen) return;
        if (InteractableInRange(isRightHand) == true) return;

        var hand = isRightHand ? Player.Instance.GetRightHand() : Player.Instance.GetLeftHand();
        if ((hand.IsHoldingObject)) return;

        if(_fingerSnapSounds != null && _fingerSnapSounds.SoundArray != null && _fingerSnapSounds.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                            _fingerSnapSounds.SoundArray,
                                                            hand.transform.position,
                                                            _fingerSnapSounds.LastPlayedSound,
                                                            true, true);
        }

        RaycastToWallsAndPlaySound(hand.transform.position, _fingerSnapSounds.LastPlayedSound);
    }
    private bool InteractableInRange(bool rightHand)
    {
        var position = rightHand ?
                Player.Instance.GetRightHand().transform.position :
                Player.Instance.GetLeftHand().transform.position;
        float radius = Player.Instance.GetRightHand().GetColliderRadius();
        return Physics.OverlapSphere(position, radius, _interactableLayerMask).Length > 0;
    }
    private void RaycastToWallsAndPlaySound(Vector3 startPosition, Sound soundToPlay)
    {
        Vector3[] directions = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        RaycastHit[] hits = new RaycastHit[4];

        for (int i = 0; i < directions.Length; i++)
        {
            Physics.Raycast(startPosition, directions[i],out hits[i], float.MaxValue, _layersToPlayEchoFrom);
        }

        foreach (var hit in hits)
        {
            var distance = Vector3.Distance(startPosition, hit.point);

            var soundWithLoweredVolume = new Sound(soundToPlay);
            soundWithLoweredVolume.Volume *= 0.9f; // Make excho sligtly quieter.

            AudioPlayer.PlaySoundAtPointWithDelay(this,
                                                soundToPlay,
                                                hit.point,
                                                distance * _echoDelayMultiplier,
                                                usePitchVariation: false,
                                                spatialize: _spatializeFingerSnaps);
        }
    }
    #endregion'

    public void Enable() => enabled = true;
    public void Disable() => enabled = false;
    
}
