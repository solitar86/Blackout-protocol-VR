using UnityEngine;

public class WaterFaucet : StaticInteractable
{
    [Space(15), Header("Water Faucet specific settings")]
    [SerializeField] private Sound _faucetStartSound;
    [SerializeField] private Sound _faucetStopSound;
    [SerializeField] private Sound _waterRunningLoop;
    private AudioSource _waterLoopSource;
    [SerializeField] private Transform waterObj;

    private void Update()
    {
        if (IsActivated == true)
        {
            if (_waterLoopSource == null)
            {
                _waterLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _waterRunningLoop);
                AudioPlayer.PlaySoundAtPoint(this, _faucetStartSound, transform.position, true);
            }

            if (_waterLoopSource.isPlaying == false)
            {
                _waterLoopSource.Play();
                AudioPlayer.PlaySoundAtPoint(this, _faucetStartSound, transform.position, true);
            }

            if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue))
            {
                if(hitInfo.collider.TryGetComponent<Iinteractable>(out var component))
                {
                    _waterLoopSource.Pause();
                    if(component is CoffeeCup)
                    {
                        (component as CoffeeCup).FillCupWithWater();
                    }
                }
                else
                {
                    _waterLoopSource.UnPause();
                }
            }
        }
        if (IsActivated == false)
        {
            if (_waterLoopSource == null) return;
            if (_waterLoopSource.isPlaying == true)
            {
                _waterLoopSource.Stop();
                AudioPlayer.PlaySoundAtPoint(this, _faucetStopSound, transform.position, true);
            }
        }
    }
}
