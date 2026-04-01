using System;
using UnityEngine;
using UnityEngine.Events;

public class BreakableMachine_Water : MonoBehaviour
{
    [SerializeField] private int _waterRequiredToBreak = 6;
    [SerializeField] private Sound _fuseBoxHummLoop;
    [SerializeField] private Sound _takeDamageSound;
    [SerializeField] private Sound _isDamagedLoop;
    [SerializeField] private Sound _breakSoundEffect;
    [SerializeField] private UnityEvent _onDamaged;
    [SerializeField] private UnityEvent _onBreak;
    [SerializeField] private UnityEvent _onTouchedWithHammer;

    private AudioSource _fuseBoxHummSource;
    private AudioSource _damageLoopSource = null;
    private bool _isBroken;

    #region Unity Callbacks
    private void Start()
    {
        PlayRadioStaticBeaconLoop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isBroken) return;
        if (collision.gameObject.TryGetComponent<Hammer>(out var hammer))
        {
            _onTouchedWithHammer?.Invoke();
        }
    }

    #endregion
    public void ReactToWater()
    {
        _waterRequiredToBreak--;

        if(_isBroken == false)
        {
            _onDamaged.Invoke();
            if(_damageLoopSource == null) _damageLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _isDamagedLoop);
            _damageLoopSource.transform.position = transform.position;
            AudioPlayer.PlaySoundAtPoint(this, _takeDamageSound, transform.position, true);
            if(_waterRequiredToBreak > 0)
            {
                _damageLoopSource.volume = (1f / _waterRequiredToBreak) + 0.1f; // a small minimum volume.
            }
        }

        if( _waterRequiredToBreak <= 0  && _isBroken == false)
        {
            Break();
        }
    }
    private void Break()
    {
        _isBroken = true;
        _onBreak?.Invoke();
        _damageLoopSource.Stop();
        _fuseBoxHummSource.Stop();
        AudioPlayer.PlaySoundAtPoint(this, _breakSoundEffect, transform.position);
        EventManager.OnBreakableMachineBreak.Raise(this, -1);

#if UNITY_EDITOR
        // This is for debugging and playtesting purposes.
        GetComponent<MeshRenderer>().material.color = Color.red;
#endif
    }
    private void PlayRadioStaticBeaconLoop()
    {
        if (_fuseBoxHummSource == null)
        {
            InitStaticLoopAudioSource();
            return;
        }

        _fuseBoxHummSource.Play();
    }
    private void InitStaticLoopAudioSource()
    {
        _fuseBoxHummSource = AudioPlayer.CreateLoopingAudioSource(this, _fuseBoxHummLoop, true);
        _fuseBoxHummSource.transform.position = transform.position;
        _fuseBoxHummSource.transform.SetParent(transform);
        _fuseBoxHummSource.gameObject.AddComponent<BeaconLPFController>();
    }

    private void OnDrawGizmosSelected()
    {
        if (_fuseBoxHummLoop != null)
        {
            Gizmos.color = Color.lightBlue;
            Gizmos.DrawWireSphere(transform.position, _fuseBoxHummLoop.MinDistance);
            Gizmos.DrawWireSphere(transform.position, _fuseBoxHummLoop.MaxDistance);
        }
    }
}
