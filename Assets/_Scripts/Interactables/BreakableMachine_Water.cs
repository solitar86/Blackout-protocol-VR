using System;
using UnityEngine;
using UnityEngine.Events;

public class BreakableMachine_Water : MonoBehaviour
{
    [SerializeField] private int _waterRequiredToBreak = 6;
    [SerializeField] Sound _takeDamageSound;
    [SerializeField] Sound _isDamagedLoop;
    [SerializeField] Sound _breakSoundEffect;
    [SerializeField] UnityEvent _onBreak;

    private AudioSource _damageLoopSource = null;

    private bool _isBroken;
    public void ReactToWater()
    {
        _waterRequiredToBreak--;

        if(_isBroken == false)
        {
            if(_damageLoopSource == null) _damageLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _isDamagedLoop);
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

        Debugger.Log("Breaking machine", Debugger.TextColor.Purple);
        _isBroken = true;
        _onBreak?.Invoke();
        _damageLoopSource.Stop();
        AudioPlayer.PlaySoundAtPoint(this, _breakSoundEffect, transform.position);
        EventManager.OnBreakableMachineBreak.Raise(this, -1);

#if UNITY_EDITOR
        // This is for debugging and playtesting purposes.
        GetComponent<MeshRenderer>().material.color = Color.red;
#endif
    }

    #region Unity Callbacks

    #endregion
}
