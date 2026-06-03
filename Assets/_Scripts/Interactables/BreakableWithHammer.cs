using System;
using UnityEngine;
using UnityEngine.Events;

public class BreakableWithHammer : MonoBehaviour
{
    [SerializeField] private float _minVelocityToBreak;
    [SerializeField] private UnityEvent _onHitHardEnoughToTriggerBreak;
    [SerializeField] private UnityEvent _onHitTooSoftly;
    [SerializeField] private UnityEvent _onHitWithHand;
    private bool _isBroken;

    [SerializeField] private Sound _HintVoiceline;
    [SerializeField] private Sound _onBreakSound;

    public void HandlePlayerTryHitWithHand()
    {
        _onHitWithHand?.Invoke();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isBroken) return;
        if (collision.gameObject.TryGetComponent<Hammer>(out var hammer))
        {
            if(PlayerInputHandler.PlayerIsMoving) return; // Don't break door by walking into it holding a hammer.

            if (hammer.IsHeld && hammer.Velocity >= _minVelocityToBreak)
            {
                TakeHitAndGetDamaged();
                return;
            }

            _onHitTooSoftly?.Invoke();
        }
    }

    private void TakeHitAndGetDamaged()
    {
        if( _isBroken == true) return;
        _isBroken = true;
        Debugger.Log("Player broke door with hammer", Debugger.TextColor.Orange);
        _onHitHardEnoughToTriggerBreak?.Invoke();
    }
}
