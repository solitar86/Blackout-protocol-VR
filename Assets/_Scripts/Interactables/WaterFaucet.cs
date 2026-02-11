using UnityEngine;

public class WaterFaucet : StaticInteractable
{
    [Space(15), Header("Water Faucet specific settings")]
    [SerializeField] private Sound _waterDrippingLoopSound;
    [SerializeField] private Sound _faucetStartSound;
    [SerializeField] private Sound _faucetStopSound;
    [SerializeField] private Sound _waterRunningLoop;
    private AudioSource _waterRunningLoopSource;
    private AudioSource _waterDrippingSource;
    [SerializeField] private Transform waterObj;

    #region Unity Callbacks
    private void Update()
    {
        if (IsActivated == true)
        {
            HandleFaucet_On_Audio();
            HandleCoffeeCupUnderRunningWater();
        }

        if (IsActivated == false)
        {
            HandleFaucet_Off_Audio();
        }
    }
    #endregion
    private void HandleFaucet_Off_Audio()
    {
        if (_waterDrippingSource == null)
        {
            // First time deactivated (could be start of scene)
            // Create dripping sound source and start loop.
            _waterDrippingSource = AudioPlayer.CreateLoopingAudioSource(this, _waterDrippingLoopSound);
            _waterDrippingSource.transform.position = transform.position;
        }

        if (_waterDrippingSource.isPlaying == false)
        {
            // We arent playing dripping loop yet
            // but we shuold be. Begin playing. 
            _waterDrippingSource.Play();
        }


        if (_waterRunningLoopSource != null && _waterRunningLoopSource.isPlaying == true)
        {
            // We were just deactivated. Stop water running sound
            // and play faucet stop sound.
            _waterRunningLoopSource.Stop();
            AudioPlayer.PlaySoundAtPoint(this, _faucetStopSound, transform.position, true);
        }
    }
    private void HandleFaucet_On_Audio()
    {
        if (_waterRunningLoopSource == null)
        {
            // Handle audio when faucet starts running for first time in game.
            // Play start sound and start water running loop.
            _waterRunningLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _waterRunningLoop);
            _waterRunningLoopSource.transform.position = transform.position;
            AudioPlayer.PlaySoundAtPoint(this, _faucetStartSound, transform.position, true);
        }

        if (_waterRunningLoopSource.isPlaying == false)
        {
            // We were just turned on.
            // Play startsound and start water running loop.

            _waterRunningLoopSource.Play();
            AudioPlayer.PlaySoundAtPoint(this, _faucetStartSound, transform.position, true);
        }

        if (_waterDrippingSource != null && _waterDrippingSource.isPlaying == true)
        {
            // Stop drip sound because water is running.
            _waterDrippingSource.Stop();
        }
    }
    private void HandleCoffeeCupUnderRunningWater()
    {
        // Handle coffee cup placed under "running water"
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue))
        {
            if (hitInfo.collider.TryGetComponent<Iinteractable>(out var component))
            {
                _waterRunningLoopSource.Pause();
                if (component is CoffeeCup)
                {
                    (component as CoffeeCup).FillCupWithWater();
                }
            }
            else
            {
                _waterRunningLoopSource.UnPause();
            }
        }
    }
}
