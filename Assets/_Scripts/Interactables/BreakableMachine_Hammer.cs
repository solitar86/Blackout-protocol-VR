using System;
using UnityEngine;
using UnityEngine.Events;

public class BreakableMachine_Hammer : MonoBehaviour
{
    [SerializeField] private int _hitsRequiredToBreak = 3;
    [SerializeField] float _minVelocityToBreak = 10f;
    [SerializeField] Sound _airConHummLoop;
    [SerializeField] Sound _breakSoundEffect;
    [SerializeField] private UnityEvent _onDamaged;
    [SerializeField] private UnityEvent _onBreak;
    [SerializeField] private UnityEvent _onHitWithHammerAfterBreak;

    private AudioSource _airConditionerHummLoopSource;
    private int _hitpoints;
    private bool _isBroken;
    public bool IsBroken;

    #region Unity Callbacks
    
    private void Awake()
    {
        _hitpoints = _hitsRequiredToBreak;
    }
    private void Start()
    {
        PlayRadioStaticBeaconLoop();
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Hammer>(out var hammer))
        {
            Debugger.Log("Hit with hammer after breaking", Debugger.TextColor.Purple);
            _onHitWithHammerAfterBreak?.Invoke();
        }
    }
    
    #endregion
    private void TakeHit()
    {
        _hitpoints--;
        if (_hitpoints <= 0 && _isBroken == false)
        {
            Break();
        }

        _onDamaged?.Invoke();
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
    private void PlayRadioStaticBeaconLoop()
    {
        if (_airConditionerHummLoopSource == null)
        {
            InitStaticLoopAudioSource();
            return;
        }

        _airConditionerHummLoopSource.Play();
    }
    private void InitStaticLoopAudioSource()
    {
        _airConditionerHummLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _airConHummLoop, true);
        _airConditionerHummLoopSource.transform.position = transform.position;
        _airConditionerHummLoopSource.transform.SetParent(transform);
        _airConditionerHummLoopSource.gameObject.AddComponent<BeaconLPFController>();
    }
    public void StopPlayingHummLoop()
    {
        _airConditionerHummLoopSource.Stop();
    }

}
