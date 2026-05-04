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
    #region Fields

    [Header("Dialogue and SFX for touching this object")]
    [Tooltip("An array of sounds which we can choose from when we touch this object.")]
    [SerializeField] private SoundArrayHolder _touchSoundsHolder;

    [Space(5), Header("Sounds for interactions")]
    [Tooltip("An array of sounds which we can choose from when we pickup this object.")]
    [SerializeField] private SoundArrayHolder _pickUpSounds;

    [Tooltip("An array of sounds which we can choose from when we let go of this object.")]
    [SerializeField] private SoundArrayHolder _dropOnSurfaceSounds;

    [Tooltip("Character voice-over that should speak when we touch object.")]
    [SerializeField] private Sound _touchIdentifyVO;

    [Tooltip("Sound to make when object is pinged -> surface near is touched.")]
    [SerializeField] private Sound _pingSound;

    [Tooltip("An array of sound to play when we hit this object againts a surface while it's beind held.")]
    [SerializeField] internal SoundArrayHolder _impactSoundHolder; // This should be a sound holder, TODO

    [SerializeField] private bool _velocityEffectsVolume = true;
    [Tooltip("What velocity == volume 1f")]
    [SerializeField] private float _velocityToVolumeCap = 10f;

    [Tooltip("Curve is built on Awake() based on VelocityToVolumeCap")]
    [SerializeField] private AnimationCurve _velocityToVolumeCurve;

    [Space(5), Header("Haptic settings for touch, pickup, drop and interact")]
    [SerializeField] private VibrationSettingsSO _touchHapticSettings;
    [SerializeField] private VibrationSettingsSO _pickUpAndDropHapticSettings;

    [Tooltip("How this object should orient itself in the players hand. Adjust at runtime to see results.")]
    [Space(5), Header("Settings for how item is oriented when held")]
    [SerializeField] private HoldPickupOffsetSettingsSO _offsetSettings;

    [Header("Where can this object be placed?")]
    [SerializeField] private LayerMask _placeableSurfaceLayerMask;
    [SerializeField] private LayerMask _floorLayer;

    private bool _isHeld;
    private float _velocity;
    private float _nextTimeAllowImpactSound = 0f;
    private float _impactSoundBuffer = 0.25f;
    private Vector3 _startingPosition;
    private Vector3 _previousPosition;
    private Quaternion _startingRotation;
    private PlayerHand _holdingHand;
    private Collider _collider;
    private Rigidbody _ridibody;
    /// <summary>
    /// I Do not remember why this field exists.
    /// </summary>
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

    /// <summary>
    /// Reference to the hand which SHOULD BE holding this object if it's held.
    /// </summary>
    public PlayerHand HoldingHand => _holdingHand;

    #endregion

    #region Unity Callbacks
    public virtual void Awake()
    {
        _startingPosition = transform.position;
        _startingRotation = transform.rotation;
        Debugger.Log("Awake was called on: " + gameObject.name, Debugger.TextColor.Orange);

        InitRigidBody();
    }
    public virtual void Update()
    {
        if (_isHeld == false) return;
        _velocity = (_previousPosition - transform.position).magnitude / Time.deltaTime;
        _previousPosition = transform.position;
    }
    public virtual void FixedUpdate()
    {

        if (_ridibody == null) InitRigidBody();
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
        HandleCollisiondWithEnvironment(collision.gameObject);
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

    #region Core functions
    public virtual void Activate()
    {
        // Do Something
    }
    public virtual void Release()
    {
        if (_isHeld == false || _holdingHand == null)
            return; // This can happen if object is force removed from player.

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
        if (_dropOnSurfaceSounds != null && _dropOnSurfaceSounds.SoundArray != null && _dropOnSurfaceSounds.SoundArray.Length > 0)
        {
            //BUG NOTE: Holding hand can sometimes be null here?
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                _dropOnSurfaceSounds.SoundArray,
                                                _holdingHand.transform.position,
                                                _dropOnSurfaceSounds.LastPlayedSound,
                                                true);
        }

        Debugger.Log("Setting Holding HandTo Null", Debugger.TextColor.Orange);
        _holdingHand = null;
        _isHeld = false;
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

        PlayPickUpSound();

        EventManager.OnAnyObjectPickUpObjectPickedUp.Raise(this, -1);
    }
    public virtual void Touch(PlayerHand hand)
    {
        HandTouchIDVoiceline();
        HandleItemTouchSound(hand);
        HandleTouchHaptics(hand);
        if(IsHeld == true)
        {
            // We touched an item with our other hand which we are holding.
            HandleHoldingHandHapticsOnObjectTouched();
        }
    }
    public virtual void Ping(float delay)
    {
        if (_pingSound.Clip != null)
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


        if (isValidSurface == true)
        {
            float dropSoundDelay = 0f;
            float height = Vector3.Distance(transform.position, hitInfo.point);
            dropSoundDelay = CalculateDropSoundDelay(height);
            transform.position = hitInfo.point;

            //Rotate to lay flat on ground.
            Vector3 normalDirectionInWorld = transform.forward;
            Quaternion rotation = Quaternion.FromToRotation(normalDirectionInWorld, Vector3.up);
            transform.rotation = rotation * transform.rotation;

            PlayObjectPlacedOnSurfaceSound(_impactSoundHolder, hitInfo.point, dropSoundDelay);
            EventManager.OnAnyPickUpObjectPlacedOnSurface.Raise(this, this);
        }
        else
        {
            HandleObjectDroppedOnFloor();
        }
        _collider.enabled = true; // Is this necessary
    }
    public virtual void HandleObjectDroppedOnFloor()
    {
        Physics.Raycast(transform.position,
                        Vector3.down,
                        out RaycastHit hitInfo,
                        float.MaxValue,
                        _floorLayer);

        Vector3 dropHitPosition = hitInfo.point;
        ResetPositionAndRotationToStartPosAndRot();

        float curseWordDelay = 0.25f;
        this.CallWithDelay(() =>
        {
            EventManager.OnPlayerCurse.Raise(this, 0);
        }, curseWordDelay);

        float dropSoundDelay = 1f; // Hard coded duration for dropping on floor.
        PlayObjectPlacedOnSurfaceSound(_impactSoundHolder, dropHitPosition, dropSoundDelay);
        HandleSpecialCasesForHittingFloor(dropHitPosition, dropSoundDelay);
        EventManager.OnAnyPickUpObjectHitFloor.Raise(this, this);
    }
    public virtual void HandleCollisiondWithEnvironment(GameObject environmentObject)
    {
        if (_isHeld == false) return; // This now assumes that we have to be holding
                                      // the object for it to make an impact sound
                                      // does not work if we want objects to be thrown.

        if (_nextTimeAllowImpactSound > Time.time) return;

        if (_impactSoundHolder == null || _impactSoundHolder.SoundArray == null || _impactSoundHolder.SoundArray.Length == 0)
        {
            Debugger.LogWarning(gameObject.name + " does not have valid impact sounds");
            return;
        }

        Sound impactSound = AudioPlayer.GetRandomSoundFromArray(_impactSoundHolder.SoundArray, _impactSoundHolder.LastPlayedSound);
        _impactSoundHolder.LastPlayedSound = impactSound;

        if (_velocityEffectsVolume == true)
        {

            Sound impactWithModVolume = new Sound(impactSound);
            impactWithModVolume.Volume = _velocityToVolumeCurve.Evaluate(Velocity);
            AudioPlayer.PlaySoundAtPoint(this, impactWithModVolume, transform.position, true);
        }
        else
        {
            AudioPlayer.PlaySoundAtPoint(this, impactSound, transform.position, true);
        }

        if (environmentObject.TryGetComponent<TouchableSurface>(out var surface))
        {
            surface.HandleTouchedWithPickUpObject(this);
        }

        HandleCollisiondWithSpecificObjects(environmentObject);

        _nextTimeAllowImpactSound = Time.time + _impactSoundBuffer;
    }

    /// <summary>
    /// Override this method to add behavior for specific collisions with objects
    /// </summary>
    /// <param name="environmentObject">What we collided with</param>
    public virtual void HandleCollisiondWithSpecificObjects(GameObject environmentObject)
    {
        //Add custom logic.
    }

    public virtual void HandleSpecialCasesForHittingFloor(Vector3 dropPosition, float delay)
    {
        // Do something if necessary.
    }
    /// <summary>
    /// Force remove something from player hand.
    /// Add "HoldingHand" as input parameter, it is a derived
    /// field from the abstract Pick-up object class.
    /// </summary>
    /// <param name="holdingHand"></param>
    public virtual void ForceRemoveObjectFromHandAndReturnToStartPosition(PlayerHand holdingHand)
    {
        transform.SetParent(null);
        parentTransformReference = null;

        if (holdingHand != null)
        {
            // I do not understand why sometimes holdingHand is null if you hit
            // a coffee cup againts a wall but it isn't if you hit a table etc.
            EventManager.OnForceRemovePickUpObject.Raise(this, holdingHand.IsRightHand);
        }

        _holdingHand = null;
        _isHeld = false;
        ResetPositionAndRotationToStartPosAndRot();
    }

    #endregion;

    #region InterfaceFunctions
    void Iinteractable.Activate()
    {
        Debugger.Log("Activating " + gameObject.name, gameObject);
        Activate();
    }
    void Iinteractable.Release()
    {
        Release();
    }
    void Iinteractable.PickUp(Transform parent, PlayerHand hand)
    {
        PickUp(parent, hand);
    }
    void Iinteractable.Touch(PlayerHand hand)
    {
        Touch(hand);
    }
    void Iinteractable.EndTouch()
    {
        EndTouch();
    }
    void Iinteractable.Ping(float delay)
    {
        Ping(delay);
    }

    #endregion

    #region Private functions / helpers for organization purposes
    private void InitRigidBody()
    {
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
    private void ResetPositionAndRotationToStartPosAndRot()
    {
        transform.position = _startingPosition;
        transform.rotation = _startingRotation;
    }
    private void HandTouchIDVoiceline()
    {
        EventManager.OnPlayerTouchPickUp.Raise(this, this);
        // TODO: Consider should we be able to "touch" something if we are holding it?
        // Play Touch Dialogue for this object
        //_nextTimeAllowTouchVO = Time.time + PlayerSettings.Developer.TouchDialogueInterval;
        //AudioPlayer.PlaySoundAtPoint(this, _touchIdentifyVO, transform.position, true);
        EventManager.OnPlayerObjectIDVOShouldPlay.Raise(this, _touchIdentifyVO);

    }
    private void HandleTouchHaptics(PlayerHand hand)
    {
        if (_touchHapticSettings != null)
        {
            hand.HandleTouchBegin(_touchHapticSettings, this.transform.position);
            return;
        }
        Debugger.LogWarning(gameObject.name + " has null touch haptic settings.", gameObject);
    }
    private void HandleItemTouchSound(PlayerHand hand)
    {
        // Play item specific touch sound
        if (_touchSoundsHolder != null && _touchSoundsHolder.SoundArray != null && _touchSoundsHolder.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _touchSoundsHolder.SoundArray,
                                                        hand.transform.position,
                                                        _touchSoundsHolder.LastPlayedSound,
                                                        true,
                                                        true);
        }
    }
    private void HandleHoldingHandHapticsOnObjectTouched()
    {
        if (_touchHapticSettings != null && HoldingHand != null)
        {
            HoldingHand.HandleTouchBegin(_touchHapticSettings, this.transform.position);
            return;
        }
        Debugger.LogWarning(gameObject.name + " has null touch haptic settings.", gameObject);
    }
    public virtual void PlayObjectPlacedOnSurfaceSound(SoundArrayHolder impactSoundHolder, Vector3 point, float delay)
    {
        if (_impactSoundHolder == null || _impactSoundHolder.SoundArray == null || _impactSoundHolder.SoundArray.Length == 0)
        {
            Debugger.LogWarning(gameObject.name + " does not have valid impact sounds");
            return;
        }
        Sound impactSound = AudioPlayer.GetRandomSoundFromArray(_impactSoundHolder.SoundArray, _impactSoundHolder.LastPlayedSound);
        _impactSoundHolder.LastPlayedSound = impactSound;
        Sound impactWithModVolume = new Sound(impactSound);
        impactWithModVolume.Volume = 0.5f;
        // Playes drop sound twice to simulate hitting and falling on to a side.
        AudioPlayer.PlaySoundAtPointWithDelay(this, impactWithModVolume, point, delay, true);
        AudioPlayer.PlaySoundAtPointWithDelay(this, impactWithModVolume, point, delay + Random.Range(0.01f, 0.25f), true);
    }
    public virtual void PlayPickUpSound()
    {
        if (_pickUpSounds != null && _pickUpSounds.SoundArray != null && _pickUpSounds.SoundArray.Length > 0)
        {
            var position = transform.position;
            if (_holdingHand != null) position = _holdingHand.transform.position;

            _pickUpSounds.LastPlayedSound = AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                                                    _pickUpSounds.SoundArray,
                                                                                    position,
                                                                                    _pickUpSounds.LastPlayedSound,
                                                                                    true);
        }
    }
    private static float CalculateDropSoundDelay(float height)
    {
        return Mathf.Sqrt((height * 2) / Mathf.Abs(Physics.gravity.y));
    }
    #endregion

}
