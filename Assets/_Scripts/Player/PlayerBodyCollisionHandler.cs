using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerBodyCollisionHandler : MonoBehaviour
{

    [SerializeField] private LayerMask _layersToCollideWith;
    [SerializeField] private Transform _hitMarker;
    [SerializeField] private Transform _playerHead;
    [Space(15), Header("Raycast settings")]
    [SerializeField] private float _raycastDistance = 0.3f;
    [SerializeField] private float[] _raycastHeightIntervals = { 0.5f, 0.3f };
    [SerializeField] private int _numberOfRaycasts = 5;
    [Space(15), Header("Sounds")]
    [SerializeField] private Sound _playerCollisionSound;

    private float _nextTimeAllowCollisionSound = 0f;
    private bool[] _directionIsColliding;
    private bool[] _hitThisFrame;
    private Vector3[] _directions;


    private void Awake()
    {
        if (_playerHead == null)
        {
            _playerHead = Camera.main.transform;
        }

        _directionIsColliding = new bool[_numberOfRaycasts];
        _directions = new Vector3[_numberOfRaycasts];
        _hitThisFrame = new bool[_numberOfRaycasts];

    }
    void Update()
    {
        Vector3 playerPosOnGround = new Vector3(_playerHead.position.x,
                                                transform.position.y,
                                                _playerHead.position.z);

        // Clear frame state from previous frame.
        for (int i = 0; i < _hitThisFrame.Length; i++) _hitThisFrame[i] = false;

        // Set directions based on player head direction.
        for (int i = 0; i < _numberOfRaycasts; i++)
        {
            float angle = (360f / _numberOfRaycasts) * i;
            Vector3 facingDirection = _playerHead.forward;
            facingDirection.y = 0f;
            _directions[i] = Quaternion.AngleAxis(angle, Vector3.up) * facingDirection;
        }

        float headHeight = _playerHead.position.y;
        Dictionary<int, RaycastHit> raycastHitDictionary = new();

        // Raycast to all directions on all height levels.
        for (int j = 0; j < _raycastHeightIntervals.Length; j++)
        {
            float height = headHeight * _raycastHeightIntervals[j];

            for (int i = 0; i < _directions.Length; i++)
            {
                Vector3 start = playerPosOnGround + Vector3.up * height;
                Vector3 end = start + _directions[i] * _raycastDistance;

                Debug.DrawLine(start, end, Color.green, 1 / 50);

                if (Physics.Raycast(start, _directions[i], out RaycastHit hitInfo, _raycastDistance, _layersToCollideWith) == true)
                {
                    _hitThisFrame[i] = true;
                    if(raycastHitDictionary.TryGetValue(i, out _) == false) raycastHitDictionary.Add(i, hitInfo);
                }
            }
        }

        for (int i = 0; i < _directions.Length; i++)
        {
            if (_hitThisFrame[i] == true && _directionIsColliding[i] == false)
            {
                // TODO: Maybe move this time check above we we can avoid doing all this shit on every update.
                if(raycastHitDictionary.TryGetValue(i, out var hit) && _nextTimeAllowCollisionSound < Time.time) 
                {
                    // We have hits and the cooldown for playing collisions sounds has elapsed;
                    AudioPlayer.PlaySoundAtPoint(this, _playerCollisionSound, hit.point, true);
                    _nextTimeAllowCollisionSound = Time.time + PlayerSettings.Developer.TouchDialogueInterval;
#if UNITY_EDITOR
                    SpawnDebugSphereOnHitPoint(hit);
#endif
                }

                _directionIsColliding[i] = true;
            }
            else if (_hitThisFrame[i] == false)
            {
                _directionIsColliding[i] = false;
            }
        }

        // No collisions, reset time buffer on allow collision sounds
        if(raycastHitDictionary.Count == 0)
        {
            // No hits this frame.
            _nextTimeAllowCollisionSound = 0f;
        }
        raycastHitDictionary.Clear();
    }

#if UNITY_EDITOR
    private void SpawnDebugSphereOnHitPoint(RaycastHit hit)
    {
        var go = Instantiate(_hitMarker, hit.point, Quaternion.identity).gameObject;
        Destroy(go, 3f);
    }
#endif
}
