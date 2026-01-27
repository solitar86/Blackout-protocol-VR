using System;
using UnityEngine;
using UnityEngine.Events;

public class BreakableMachine : MonoBehaviour
{
    [SerializeField] private int _hitsRequiredToBreak = 3;
    [SerializeField] float _minVelocityToBreak = 10f;
    [SerializeField] Sound _breakSoundEffect;
    [SerializeField] private UnityEvent _onBreak;
    [SerializeField] private UnityEvent _onHitWithHammerAfterBreak;

    private int _hitpoints;
    private bool _isBroken;
    public bool IsBroken;

    private void Awake()
    {
        _hitpoints = _hitsRequiredToBreak;
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Handle sound logic
        // TODO:

        // Handle breaking logic
        if (_isBroken) return;
        if (collision.gameObject.TryGetComponent<Hammer>(out var hammer))
        {
            if (hammer.IsHeld && hammer.Velocity >= _minVelocityToBreak)
            {
                Debugger.WorldSpaceText("Hit vel: " + hammer.Velocity.ToString("F1"), collision.contacts[0].point);
                TakeHit();
            }
        }
    }

    private void TakeHit()
    {
        _hitpoints--;
        if (_hitpoints <= 0 && _isBroken == false)
        {
            Break();
        }
    }

    private void Break()
    {

        Debugger.Log("Breaking machine", Debugger.TextColor.Purple);
        _isBroken = true;
        _onBreak?.Invoke();
        AudioPlayer.PlaySoundAtPoint(this, _breakSoundEffect, transform.position);
        GetComponent<Collider>().isTrigger = true;
        EventManager.OnBreakableMachineBreak.Raise(this, -1);

#if UNITY_EDITOR
        // This is for debugging and playtesting purposes.
        GetComponent<MeshRenderer>().material.color = Color.red;
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Hammer>(out var hammer))
        {
            Debugger.Log("Hit with hammer after breaking", Debugger.TextColor.Purple);
            _onHitWithHammerAfterBreak?.Invoke();
        }
    }
}
