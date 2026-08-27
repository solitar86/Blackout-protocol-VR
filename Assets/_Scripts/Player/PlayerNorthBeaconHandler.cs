using System;
using System.Collections;
using UnityEngine;

public class PlayerNorthBeaconHandler : MonoBehaviour
{
    [Tooltip("The height is determined by player head height")]
    [SerializeField] private float _volumeFadeDur = 0.25f;
    [SerializeField] private Vector2 _beaconPosition;
    [SerializeField] private Sound _northBeaconSound;
    private AudioSource _northBeaconSource;
    private bool _enabled = true;
    public bool Enabled => _enabled;
    #region  Unity Callbacks
    private void OnEnable()
    {
        EventManager.OnSecondaryButtonPressed.AddListener(this, StartNorthBeaconSoundLoop);
        EventManager.OnSecondaryButtonHeld.AddListener(this, UpdateNorthBeaconPosition);
        EventManager.OnSecondaryButtonReleased.AddListener(this, StopNorthBeaconSoundLoop);
    }
    private void OnDisable()
    {
        EventManager.OnSecondaryButtonPressed.RemoveListener(this, StartNorthBeaconSoundLoop);
        EventManager.OnSecondaryButtonHeld.RemoveListener(this, UpdateNorthBeaconPosition);
        EventManager.OnSecondaryButtonReleased.RemoveListener(this, StopNorthBeaconSoundLoop);
    }

    #endregion
    private void StartNorthBeaconSoundLoop(bool isRightHand)
    {
        if (_enabled == false) return;
        if(_northBeaconSource == null)
        {
            _northBeaconSource = AudioPlayer.CreateLoopingAudioSource(this,
                                                                    _northBeaconSound,
                                                                    spatialize: false);
            _northBeaconSource.gameObject.AddComponent<BeaconLPFController>();
        }

        if(_northBeaconSource.isPlaying == false) _northBeaconSource.Play();

        StopAllCoroutines();
        StartCoroutine(FadeSourceVolumeTo(_northBeaconSound.Volume));
    }
    private void UpdateNorthBeaconPosition(bool isRightHand)
    {
        if (_enabled == false) return;
        _northBeaconSource.transform.position =
                            transform.position +
                            new Vector3(_beaconPosition.x,
                            Player.Instance.GetPlayerHeadTransform().position.y,
                            _beaconPosition.y);

        Vector3 directionToPlayerHead = // This Vector is backwards but it works, see what happens if you fix it.
                            (Player.Instance.GetPlayerHeadTransform().position - transform.position);
                            directionToPlayerHead.y = 0f;
                            directionToPlayerHead.Normalize();

        var playerLookDirection = Player.Instance.GetPlayerLookingDirection();
        playerLookDirection.y = 0;
        playerLookDirection.Normalize();

        float dotProduct = Vector3.Dot(directionToPlayerHead,
                                        playerLookDirection);

        var lerpValue = Mathf.InverseLerp(1f, -1f, dotProduct);
        _northBeaconSource.panStereo = Mathf.Lerp(-1, 1, lerpValue);
    }
    private void StopNorthBeaconSoundLoop(bool obj)
    {
        if (_northBeaconSource == null) return;

        StopAllCoroutines();
        StartCoroutine(FadeSourceVolumeTo(0f));

    }
    private IEnumerator FadeSourceVolumeTo(float targetVolume)
    {
        float timer = 0f;
        float startVolume = _northBeaconSource.volume;

        while(timer < _volumeFadeDur)
        {
            timer += Time.deltaTime;
            var lerpAmount = _volumeFadeDur / timer;
            _northBeaconSource.volume = Mathf.Lerp(startVolume, targetVolume, lerpAmount);
            yield return null;
        }
        _northBeaconSource.volume = targetVolume;
        if (targetVolume == 0 && _northBeaconSource.isPlaying == true) _northBeaconSource.Stop();
    }
    public void Enable() => _enabled = true;
    public void Disable() => _enabled = false;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        var playerHeadPos = Player.Instance == null ? 
                                                transform.position : 
                                                Player.Instance.GetPlayerHeadTransform().position;

        Vector3 position = transform.position + new Vector3(_beaconPosition.x, playerHeadPos.y, _beaconPosition.y);
        Gizmos.DrawCube(position, Vector3.one * 0.1f);
    }
    
}
