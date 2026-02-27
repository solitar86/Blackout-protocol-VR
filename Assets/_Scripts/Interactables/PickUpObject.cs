using System.IO;
using UnityEngine;

/// <summary>
/// All object which the player can hold and interact with
/// derive from this PickUpObjectClass. It handles haptics,
/// sounds, VO reactions and all other shared functionality
/// with all "pickuppable" objects. Atleast currently
/// this violates the single responsibility principle but 
/// do not be concerned. <3: The developer.
/// </summary>
[SelectionBase, RequireComponent(typeof(Rigidbody))]
public abstract class PickUpObject : MonoBehaviour, Iinteractable
{
    [Header("Dialogue and SFX for touching this object")]
    [Tooltip("An array of sounds which we can choose from when we touch this object.")]
    [SerializeField] private SoundArrayHolder _touchSoundsHolder;

    [Tooltip("Character voice-over that should speak when we touch object.")]
    [SerializeField] private Sound _touchIdentifyVO;

    [Tooltip("Sound to make when object is pinged -> surface near is touched.")]
    [SerializeField] private Sound _pingSound;

    [Tooltip("How this object should orient itself in the players hand. Adjust at runtime to see results.")]
    [Space(5), Header("Settings for how item is oriented when held")]
    [SerializeField] private HoldPickupOffsetSettingsSO _offsetSettings;

    [Header("Where can this object be placed?")]
    [SerializeField] private LayerMask _placeableSurfaceLayerMask;

    [Space(5), Header("Sounds for interactions")]
    [Tooltip("An array of sounds which we can choose from when we pickup this object.")]
    [SerializeField] private SoundArrayHolder _pickUpSounds;

    [Tooltip("An array of sounds which we can choose from when we drop this object.")]
    [SerializeField] private SoundArrayHolder _dropSounds;

    [Tooltip("An array of sound to play when we hit this object againts a surface.")]
    [SerializeField] internal Sound _impactSound; // This should be a sound holder, TODO

    [Tooltip("What velocity == volume 1f")]
    [SerializeField] private float _velocityToVolumeCap = 10f;

    [Tooltip("Curve is built on Awake() based on VelocityToVolumeCap")]
    [SerializeField] private AnimationCurve _velocityToVolumeCurve;

    [Space(5), Header("Haptic settings for touch, pickup, drop and interact")]
    [SerializeField] private VibrationSettingsSO _touchHapticSettings;
    [SerializeField] private VibrationSettingsSO _pickUpAndDropHapticSettings;

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

    /// <summary>
    /// Is this object currently in the players hand.
    /// </summary>
    public bool IsHeld => _isHeld;

    /// <summary>
    /// This can and is used to determine if something hits another object
    /// With enough force to make a louder sound or break something.
    /// </summary>
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
            _holdingHand?.HandlePickUpOrDropObject(_pickUpAndDropHapticSettings, null);
        }
        else
        {
            Debugger.LogWarning(gameObject.name + "has null pickup/drop haptic settings.", gameObject);
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
        HandleObjectPlacementAfterDrop();
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

        _holdingHand.HandlePickUpOrDropObject(_pickUpAndDropHapticSettings, this);

        if (_pickUpSounds != null && _pickUpSounds.SoundArray != null && _pickUpSounds.SoundArray.Length > 0)
        {
            _pickUpSounds.LastPlayedSound = AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
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
            // TODO: Consider should we be able to "touch" something if we are holding it?
            // Play Touch Dialogue for this object
            _nextTimeAllowTouchVO = Time.time + PlayerSettings.Developer.TouchDialogueInterval;
            AudioPlayer.PlaySoundAtPoint(this, _touchIdentifyVO, transform.position, true);
        }

        // Play item specific touch sound
        if(_touchSoundsHolder != null && _touchSoundsHolder.SoundArray != null && _touchSoundsHolder.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _touchSoundsHolder.SoundArray,
                                                        hand.transform.position,
                                                        _touchSoundsHolder.LastPlayedSound,
                                                        true,
                                                        true);
        }

        if (_touchHapticSettings == null)
        {
            Debugger.LogWarning(gameObject.name + " has null touch haptic settings.", gameObject);
            return;
        }

        hand.HandleTouchBegin(_touchHapticSettings, this.transform.position);


    }
    public virtual void Ping(float delay)
    {
        if(_pingSound.Clip != null)
        {
            AudioPlayer.PlaySoundAtPointWithDelay(this, _pingSound, transform.position, delay, true, true);
            return;
        }
        // No ping sound assigned
        var clip = Resources.Load<AudioClip>("Audio/SFX_ItemDetectPlaceHolder");
        var delayObject = new GameObject("Play clip with delay: " + clip.name);
        var mono = delayObject.AddComponent<Delay>();
        float buffer = 0.1f;

        mono.CallWithDelay(() =>
        {
            AudioPlayer.PlayClipAtPoint(this, clip, transform.position, volume: .5f, true, true);
        }, delay);
        Destroy(mono.gameObject, delay + buffer);
    }
    public virtual void EndTouch()
    {
        // Do Something
    }
    public virtual void HandleObjectPlacementAfterDrop()
    {
        // Place object on ground / surface
        _collider.enabled = false; // Is this necessary?
        bool isValidSurface = Physics.Raycast(transform.position,
                                Vector3.down,
                                out RaycastHit hitInfo,
                                float.MaxValue,
                                _placeableSurfaceLayerMask);
        float dropSoundDelay = 0f;

        if (isValidSurface == true)
        {
            float height = Vector3.Distance(transform.position, hitInfo.point);
            dropSoundDelay = CalculateDropSoundDelay(height);
            transform.position = hitInfo.point;

            //Rotate to lay flat on ground.
            Vector3 normalDirectionInWorld = transform.forward;
            Quaternion rotation = Quaternion.FromToRotation(normalDirectionInWorld, Vector3.up);
            transform.rotation = rotation * transform.rotation;

            PlayObjectHitSufaceSounds(hitInfo.point, dropSoundDelay);
            EventManager.OnAnyPickUpObjectPlacedOnSurface.Raise(this, this);
        }
        else
        {
            // We are droppin someplace which isn't good - like floor - Handle that case.
            transform.position = _startingPosition;
            transform.rotation = _startingRotation;

            // Somehow notify player that the object 
            // teleported back to where it was found from.
            // this is currenly that.
            Debugger.PlayBlipSound();

            float curseWordDelay = 0.25f;
            this.CallWithDelay(() =>
            {
                EventManager.OnPlayerCurse.Raise(this, 0);
            }, curseWordDelay);

            dropSoundDelay = 1f;
            Sound impactWithModVolume = new Sound(_impactSound);
            impactWithModVolume.Volume = 0.3f;
            PlayObjectHitSufaceSounds(_startingPosition, dropSoundDelay);
            EventManager.OnAnyPickUpObjectHitFloor.Raise(this, this);
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
        Touch(hand);
    }
    void Iinteractable.EndTouch()
    {
        //Debugger.Log("Stopped touching " + gameObject.name, gameObject);
        EndTouch();
    }
    void Iinteractable.Ping(float delay)
    {
        Ping(delay);
    }


    #endregion

    #region Helpers etc.
    private void PlayObjectHitSufaceSounds(Vector3 point, float delay)
    {
        Sound impactWithModVolume = new Sound(_impactSound);
        impactWithModVolume.Volume = 0.3f;
        AudioPlayer.PlaySoundAtPointWithDelay(this, impactWithModVolume, point, delay, true);
        AudioPlayer.PlaySoundAtPointWithDelay(this, impactWithModVolume, point, delay + Random.Range(0.01f, 0.5f), true);
    }
    private static float CalculateDropSoundDelay(float height)
    {
        return Mathf.Sqrt((height * 2) / Mathf.Abs(Physics.gravity.y));
    }
    #endregion

}
