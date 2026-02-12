using System.Collections.Generic;
using UnityEngine;

public class PlayerBodyCollisionHandler : MonoBehaviour
{

    [SerializeField] private LayerMask _layersToCollideWith;
    [SerializeField] private Transform _hitMarker;
    [SerializeField] private Transform _playerHead;
    [Space(15), Header("Raycast settings")]
    [SerializeField] private float _raycastDistance = 0.3f;
    [SerializeField] private float[] _raycastHeightIntervals = { 0.3f, 0.5f };
    [SerializeField] private int _numberOfRaycasts = 5;
    [Space(15), Header("Sounds")]
    [SerializeField] private SoundArrayHolder _playerCollisionSoundHolder;
    [SerializeField] private Sound _playerScrapeObstacleSound;

    private AudioSource _scrapeWallAudioLoopSource;
    private Vector3 _currentTouchingPoint;
    private Vector3 _previousTouchingPoint;
    private bool _isTouchingObstacle = false;
    private bool _wasTouchingLastFrame = false;
    private bool _audioWasIncreasedThisFrame = false;
    private float _audioSmoothDampVelocity = 0f;

    #region Unity Callbacks
    private void Awake()
    {
        if (_playerHead == null)
        {
            _playerHead = Camera.main.transform;
        }
#if !UNITY_EDITOR
        _hitMarker.gameObject.SetActive(false):
#endif

    }
    void Update()
    {
        HandleScrapingAudioSourceVolumeDecrease();

        Vector3 groundOrigin = _playerHead.position;
        groundOrigin.y = transform.position.y; // base reference level

        _isTouchingObstacle = false;

        Vector3 baseDirection = _playerHead.forward;
        baseDirection.y = 0f;
        baseDirection.Normalize();

        float angleStep = 360f / _numberOfRaycasts;

        for (int h = 0; h < _raycastHeightIntervals.Length; h++)
        {
            float heightOffset = _playerHead.position.y * _raycastHeightIntervals[h];
            Vector3 origin = groundOrigin + Vector3.up * heightOffset;

            for (int i = 0; i < _numberOfRaycasts; i++)
            {
                float angle = angleStep * i;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * baseDirection;

                if (Physics.Raycast(origin, direction, out RaycastHit hit,
                                    _raycastDistance, _layersToCollideWith))
                {
                    _isTouchingObstacle = true;
                    _currentTouchingPoint = hit.point;
                    if (_wasTouchingLastFrame == false) HandlePlayerObstacleCollisionStart();
                    
                    break;
                }
            }
        }

        if (_isTouchingObstacle == false && _wasTouchingLastFrame == true)
        {
            HandlePlayerObstacleCollisionStop();
        }

        if (_isTouchingObstacle == true && _hitMarker != null)
        {
            HandlePlayerObstacleCollisionStay();
        }


    }
    private void HandleScrapingAudioSourceVolumeDecrease()
    {
        if (_audioWasIncreasedThisFrame == false)
        {
            if (_scrapeWallAudioLoopSource != null)
            {
                _scrapeWallAudioLoopSource.volume = Mathf.SmoothDamp(_scrapeWallAudioLoopSource.volume,
                                                    0f,
                                                    ref _audioSmoothDampVelocity,
                                                    PlayerSettings.Developer.SlideAudioChangeSpeed * 2);
            }
        }
        _audioWasIncreasedThisFrame = false;
    }
    #endregion

    private void HandlePlayerObstacleCollisionStart()
    {
        _wasTouchingLastFrame = true;
        AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                    _playerCollisionSoundHolder.SoundArray,
                                                    _currentTouchingPoint,
                                                    _playerCollisionSoundHolder.LastPlayedSound,
                                                    true,
                                                    true);
    }
    private void HandlePlayerObstacleCollisionStay()
    {
        float distance = Vector3.Distance(_previousTouchingPoint, _currentTouchingPoint);

        if (distance > 0.02f)
        {
            if (_scrapeWallAudioLoopSource == null)
            {
                _scrapeWallAudioLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _playerScrapeObstacleSound);
                _scrapeWallAudioLoopSource.volume = 0f; // Don't play slide sound on first touch.
            }

            if (_scrapeWallAudioLoopSource.isPlaying == false) _scrapeWallAudioLoopSource.Play();

            _scrapeWallAudioLoopSource.transform.position = _currentTouchingPoint;
            _audioWasIncreasedThisFrame = true;
            _scrapeWallAudioLoopSource.volume = Mathf.SmoothDamp(_scrapeWallAudioLoopSource.volume,
                                                        _playerScrapeObstacleSound.Volume,
                                                        ref _audioSmoothDampVelocity,
                                                        PlayerSettings.Developer.SlideAudioChangeSpeed);

            _previousTouchingPoint = _currentTouchingPoint;
        }
#if UNITY_EDITOR
        _hitMarker.position = _currentTouchingPoint;
#endif
    }
    private void HandlePlayerObstacleCollisionStop()
    {
        if (_scrapeWallAudioLoopSource != null)
        {
            _scrapeWallAudioLoopSource.Stop();
        }

        //TODO: Play collision end sound
        _wasTouchingLastFrame = false;

    }
    private void OnDrawGizmosSelected()
    {
        if (_playerHead == null || _raycastHeightIntervals == null)
            return;

        Vector3 groundOrigin = _playerHead.position;
        groundOrigin.y = transform.position.y;

        Vector3 facingDirection = _playerHead.forward;
        facingDirection.y = 0f;
        facingDirection.Normalize();

        float angleStep = 360f / _numberOfRaycasts;

        for (int h = 0; h < _raycastHeightIntervals.Length; h++)
        {
            float heightOffset = _playerHead.position.y * _raycastHeightIntervals[h];
            Vector3 origin = groundOrigin + Vector3.up * heightOffset;

            for (int i = 0; i < _numberOfRaycasts; i++)
            {
                float angle = angleStep * i;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * facingDirection;

                Vector3 end = origin + direction * _raycastDistance;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, end);
            }
        }
    }
}
