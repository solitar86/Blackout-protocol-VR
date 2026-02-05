using System.IO;
using UnityEngine;

[SelectionBase, RequireComponent(typeof(Rigidbody))]
public abstract class PickUpObject : MonoBehaviour, Iinteractable
{
    [Header("Dialogue to play on player touch.")]
    [SerializeField] private Sound _touchIdentifyVO;
    [Space(5), Header("Settings for how item is oriented when held")]
    [SerializeField] private PickUpHoldOffsetSettings _offsetSettings;
    [Header("Where can this object be placed?")]
    [SerializeField] private LayerMask _placeableSurfaceLayerMask;
    [Space(5), Header("Sounds for interactions")]
    [SerializeField] private SoundArrayHolder _pickUpSounds;
    [SerializeField] private SoundArrayHolder _dropSounds;
    [SerializeField] internal Sound _impactSound; // This should be a sound holder, TODO
    [Tooltip("What velocity == volume 1f")]
    [SerializeField] private float _velocityToVolumeCap = 10f;
    [Tooltip("Curve is built on Awake() based on VelocityToVolumeCap")]
    [SerializeField] private AnimationCurve _velocityToVolumeCurve;
    [Space(5), Header("Haptic settings for when we touch object")]
    [SerializeField] private VibrationSettingsSO _touchHapticSettings;
    // [SerializeField] private int _repeatTouchHapticNumTimes = 1;
    [Space(5), Header("Haptic settings for when we pickup or drop object")]
    [SerializeField] private VibrationSettingsSO _pickUpAndDropHapticSettings;
    [SerializeField] private int _repeatPickUpHapticNumTimes = 2;

    private bool _isHeld;
    private float _velocity;
    private float _nextTimeAllowTouchVO = 0f;
    private Vector3 _startingPosition;
    private Vector3 _previousPosition;
    private Quaternion _startingRotation;
    private PlayerHand _holdingHand;
    private Collider _collider;
    private Rigidbody _ridibody;
    private Transform parentTransformReference = null;

    public bool IsHeld => _isHeld;
    public float Velocity => _velocity;

    #region Unity Callbacks
    public virtual void Awake()
    {
        _startingPosition = transform.position;
        _startingRotation = transform.rotation;

        _collider = GetComponent<Collider>();
        _ridibody = GetComponent<Rigidbody>();
        _ridibody.isKinematic = false;
        _ridibody.useGravity = false;
        _ridibody.mass = 0.1f;
        _ridibody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _velocityToVolumeCurve = new AnimationCurve(
        new Keyframe(0f, 0.01f),
        new Keyframe(_velocityToVolumeCap, 1f));
    }
    public virtual void Update()
    {
        if (_isHeld == false) return;
        _velocity = (_previousPosition - transform.position).magnitude / Time.deltaTime;
        _previousPosition = transform.position;
    }
    public virtual void FixedUpdate()
    {
        if (parentTransformReference != null)
        {
            transform.localPosition = _offsetSettings != null ? _offsetSettings.PositionOffset : Vector3.zero;
            transform.localRotation = _offsetSettings != null ? Quaternion.Euler(_offsetSettings.RotationOffset) : Quaternion.identity;
        }

        //BUG FIX, This might cause problems.
        // Zero out velocity so the object
        // doesn't pickup velocity when colliding.
        _ridibody.linearVelocity = Vector3.zero;
        _ridibody.angularVelocity = Vector3.zero;
    }
    private void OnCollisionEnter(Collision collision)
    {
        // We make the assumption that this object is
        // being held or it won't make a sound.
        HandleCollisiondWithEnvironment();
    }
    private void Reset()
    {
        if (_touchHapticSettings == null)
        {
            _touchHapticSettings = Resources.Load<VibrationSettingsSO>("Haptics/PickUpDropDefaultSettingsSO");
        }
        if (_pickUpAndDropHapticSettings == null)
        {
            _pickUpAndDropHapticSettings = Resources.Load<VibrationSettingsSO>("Haptics/PickUpDropDefaultSettingsSO");
        }
    }
    #endregion
    public virtual void Activate()
    {
        // Do Something
    }
    public virtual void Drop()
    {
        transform.SetParent(null);
        parentTransformReference = null;

        // Handle drop haptics.
        if (_pickUpAndDropHapticSettings != null)
        {
            _holdingHand?.HandlePickUpOrDropObject(_pickUpAndDropHapticSettings, _repeatPickUpHapticNumTimes);
        }
        else
        {
            Debugger.LogWarning(gameObject.name + "has null pickup/drop haptic settings.", gameObject);
            return;
        }

        // Handle drop sounds.
        if (_dropSounds != null && _dropSounds.SoundArray != null && _dropSounds.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                _dropSounds.SoundArray,
                                                _holdingHand.transform.position,
                                                _dropSounds.LastPlayedSound,
                                                true);
        }

        _holdingHand = null;
        _isHeld = false;
        _nextTimeAllowTouchVO = 0f;
        HandlePlaceObjectOnSurface();
    }
    public virtual void PickUp(Transform parent, PlayerHand hand)
    {
        transform.SetParent(parent);
        parentTransformReference = parent;
        _holdingHand = hand;
        _isHeld = true;

        if (_pickUpAndDropHapticSettings == null)
        {
            Debugger.LogWarning(gameObject.name + "has null pickup/drop haptic settings.", gameObject);
            return;
        }

        _holdingHand.HandlePickUpOrDropObject(_pickUpAndDropHapticSettings, _repeatPickUpHapticNumTimes);

        if (_pickUpSounds != null && _pickUpSounds.SoundArray != null && _pickUpSounds.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _pickUpSounds.SoundArray,
                                                        _holdingHand.transform.position,
                                                        _pickUpSounds.LastPlayedSound,
                                                        true);
        }
    }
    public virtual void Touch(PlayerHand hand)
    {
        EventManager.OnPlayerTouchPickUp.Raise(this, this);

        if (_nextTimeAllowTouchVO < Time.time)
        {
            // Play Touch Dialogue for this object
            _nextTimeAllowTouchVO = Time.time + PlayerSettings.Developer.TouchDialogueInterval;
            AudioPlayer.PlaySoundAtPoint(this, _touchIdentifyVO, transform.position, true);
        }

        if (_touchHapticSettings == null)
        {
            Debugger.LogWarning(gameObject.name + " has null touch haptic settings.", gameObject);
            return;
        }

        hand.HandleTouchBegin(_touchHapticSettings, this.transform.position);


    }
    public virtual void EndTouch()
    {
        // Do Something
    }
    public virtual void HandlePlaceObjectOnSurface()
    {
        // Place object on ground / surface
        _collider.enabled = false; // Is this necessary?
        bool isValidSurface = Physics.Raycast(transform.position,
                                Vector3.down,
                                out RaycastHit hitInfo,
                                float.MaxValue,
                                _placeableSurfaceLayerMask);
        float delay = 0f;
        if (isValidSurface)
        {
            float height = Vector3.Distance(transform.position, hitInfo.point);
            delay = Mathf.Sqrt((height * 2) / Mathf.Abs(Physics.gravity.y));
            transform.position = hitInfo.point;

            //Rotate to lay flat on ground.
            Vector3 normalDirectionInWorld = transform.forward;
            Quaternion rotation = Quaternion.FromToRotation(normalDirectionInWorld, Vector3.up);
            transform.rotation = rotation * transform.rotation;
            Sound impactWithModVolume = new Sound(_impactSound);
            impactWithModVolume.Volume = 0.3f;
            AudioPlayer.PlayerSoundAtPointWithDelay(this, impactWithModVolume, hitInfo.point, delay, true);
            AudioPlayer.PlayerSoundAtPointWithDelay(this, impactWithModVolume, hitInfo.point, delay + Random.Range(0.01f, 0.5f), true);
        }
        else
        {
            transform.position = _startingPosition;
            transform.rotation = _startingRotation;
            // Somehow notify player that the object 
            // teleported back to where it was found from.
            Debugger.PlayBlipSound();
            delay = 1f;
            Sound impactWithModVolume = new Sound(_impactSound);
            impactWithModVolume.Volume = 0.3f;
            AudioPlayer.PlayerSoundAtPointWithDelay(this, impactWithModVolume, _startingPosition, delay, true);
            AudioPlayer.PlayerSoundAtPointWithDelay(this, impactWithModVolume, _startingPosition, delay + Random.Range(0.01f, 0.5f), true);
        }
        _collider.enabled = true; // Is this necessary
    }
    public virtual void HandleCollisiondWithEnvironment()
    {
        if (_isHeld == false) return; // This now assumes that we have to be holding
                                      // the object for it to make an impact sound
                                      // does not work if we want objects to be thrown.
        Sound impactWithModVolume = new Sound(_impactSound);
        impactWithModVolume.Volume = _velocityToVolumeCurve.Evaluate(Velocity);
        AudioPlayer.PlaySoundAtPoint(this, impactWithModVolume, transform.position, true);
    }

    #region InterfaceFunctions
    void Iinteractable.Activate()
    {
        Debugger.Log("Activating " + gameObject.name, gameObject);
        Activate();
    }
    void Iinteractable.Drop()
    {
        Debugger.Log("Dropping " + gameObject.name, gameObject);
        Drop();
    }
    void Iinteractable.CollideWithObject()
    {
        HandleCollisiondWithEnvironment();
    }
    void Iinteractable.PickUp(Transform parent, PlayerHand hand)
    {
        Debugger.Log("Picking up " + gameObject.name, gameObject);
        PickUp(parent, hand);
    }
    void Iinteractable.Touch(PlayerHand hand)
    {
        //Debugger.Log("Touching " + gameObject.name, gameObject);
        Touch(hand);
    }
    void Iinteractable.EndTouch()
    {
        //Debugger.Log("Stopped touching " + gameObject.name, gameObject);
        EndTouch();
    }
    #endregion
}
