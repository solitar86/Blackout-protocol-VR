using System;
using UnityEngine;
using UnityEngine.Events;

public class BreakableMachine : MonoBehaviour
{
    [SerializeField] private UnityEvent _onBreak;
    [SerializeField] private UnityEvent _onHitWithHammerAfterBreak;
    private bool _isBroken;
    public bool IsBroken;
    private void OnCollisionEnter(Collision collision)
    {
        Debugger.Log("OnCollisionEnter called on Breakable Machine", Debugger.TextColor.Purple);
        // Handle sound logic
        // TODO:

        // Handle breaking logic
        if (_isBroken) return;
        if (collision.gameObject.TryGetComponent<Hammer>(out var hammer))
        {
            Debugger.Log("It was hammer", Debugger.TextColor.Purple);
            if (hammer.IsHeld)
            {
                Break();
            }
        }
    }

    private void Break()
    {
        Debugger.Log("Breaking machine", Debugger.TextColor.Purple);
        _isBroken = true;
        _onBreak?.Invoke();
        GetComponent<Collider>().isTrigger = true;
        EventManager.OnBreakableMachineBreak.Raise(this, -1);
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
