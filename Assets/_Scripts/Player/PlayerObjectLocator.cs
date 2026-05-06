using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using System;

public class PlayerObjectLocator : MonoBehaviour
{
    #region Fields
    [SerializeField] private float _overlapSphereRadius = 1f;
    [SerializeField] private LayerMask _layerMasksToSearch;
    [SerializeField] private AnimationCurve _volumeCurve;
    [SerializeField] private Sound _nothingCloseVO;
    [SerializeField] private Sound _locatorSound;

    private float _minPitch = 0.9f;
    private float _maxPitch = 1.1f;
    private AudioSource _locatorSource;
    private Transform _trackedObjectTransform;
    private PlayerHand _thisHand;
    private float _maxVolume;
    private bool _isTracking;
    public bool IsTracking { get { return _isTracking; } }
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        EventManager.OnGripHeld.AddListener(this, StartObjectLocation);
        EventManager.OnGripReleased.AddListener(this, StopObjectLocation);
        EventManager.OnPlayerTouchPickUp.AddListener(this, OnPlayerTouchPickUp);
        EventManager.OnPlayerTouchStaticInteractable.AddListener(this, OnPlayerTouchStaticInteractable);
    }
    private void Awake()
    {
        _thisHand = GetComponent<PlayerHand>();
        _maxVolume = _locatorSound.Volume;
       // _volumeCurve = CustomRollOff.Instance.GetLogCurve(0, _overlapSphereRadius, invert: true);
    }
    private void OnDisable()
    {
        EventManager.OnGripHeld.RemoveListener(this, StartObjectLocation);
        EventManager.OnGripReleased.RemoveListener(this, StopObjectLocation);
        EventManager.OnPlayerTouchPickUp.RemoveListener(this, OnPlayerTouchPickUp);
        EventManager.OnPlayerTouchStaticInteractable.RemoveListener(this, OnPlayerTouchStaticInteractable);
    }

    #endregion

    #region Core Functions
    private void StartObjectLocation(bool isRightHand)
    {
        if (_thisHand.IsRightHand != isRightHand) return;
        if(_thisHand.IsHoldingObject) return;

        // Find all colliders near hand.
        var objects = Physics.OverlapSphere(transform.position, _overlapSphereRadius, _layerMasksToSearch, QueryTriggerInteraction.Collide);
        if (objects.Length == 0) return;
        
        // Select all interactables from those colliders.
        var interactables = objects.Where(o => o.gameObject.TryGetComponent<Iinteractable>(out var iinteractable)).ToList();
        if (interactables.Count == 0)
        {
            HandleNothingCloseBy(_nothingCloseVO);
            return;
        }

        //Find the closest interactables transform
        float closestDistance = float.MaxValue;
        Transform closestTransform = null;

        for(int i = 0; i < interactables.Count; i++)
        {
            var distance = Vector3.SqrMagnitude(interactables[i].transform.position - transform.position);
            if(distance < closestDistance)
            {
                closestDistance = distance;
                closestTransform = interactables[i].transform;
            }
        }

        _trackedObjectTransform = closestTransform;
        if (_trackedObjectTransform == null) return;

        _isTracking = true;
        StartCoroutine(ObjectLocatorCoroutine());

    }
    private IEnumerator ObjectLocatorCoroutine()
    {
        float squaredDistance = float.MaxValue;
        float squaredMaxDistance = _overlapSphereRadius * _overlapSphereRadius;
        float linearValue = float.MaxValue;

        if (_locatorSource == null) _locatorSource = AudioPlayer.CreateLoopingAudioSource(this, _locatorSound, spatialize:false);
        if(_locatorSource.isPlaying == false) _locatorSource.Play();

        while(_isTracking)
        {
            _locatorSource.transform.position = transform.position;
            //_locatorSource.transform.position = Player.Instance.GetPlayerHeadTransform().position;
            squaredDistance = Vector3.SqrMagnitude(transform.position - _trackedObjectTransform.position);
            linearValue = Mathf.Clamp01((squaredDistance / squaredMaxDistance));
            float logValue =  1 - _volumeCurve.Evaluate(linearValue);
            _locatorSource.volume = Mathf.Lerp(0.01f, _maxVolume, logValue);
            _locatorSource.pitch = Mathf.Lerp(_minPitch, _maxPitch, logValue);

            yield return null;
        }
    }
    private void StopObjectLocation(bool isRightHand)
    {
        _isTracking = false;
        if(_locatorSource != null && _locatorSource.isPlaying) _locatorSource.Stop();
        StopAllCoroutines();
    }
    private void HandleNothingCloseBy(Sound nothingCloseVO)
    {
        if(nothingCloseVO == null || nothingCloseVO.Clip == null) return;
        EventManager.OnPlayerObjectIDVOShouldPlay.Raise(this, nothingCloseVO);
    }

    private void OnPlayerTouchPickUp(PickUpObject pickupObject)
    {
        if(pickupObject.transform == _trackedObjectTransform)
        {
            StopObjectLocation(_thisHand.IsRightHand);
        }
    }
    private void OnPlayerTouchStaticInteractable(StaticInteractable interactable)
    {
        if (interactable.transform == _trackedObjectTransform)
        {
            StopObjectLocation(_thisHand.IsRightHand);
        }
    }


    #endregion
}