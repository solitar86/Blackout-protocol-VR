using UnityEngine;

/// <summary>
/// This is the baseclass for all interactables which can't be picked up by the player.
/// But which can be activated by pressing trigger and then they do something.
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class StaticInteractable : MonoBehaviour, Iinteractable
{
    [Header("Dialogue to play on player touch.")]
    [SerializeField] private Sound _touchIdentifyVO;
    [Space(5),Header("Touch and interact sound holders")]
    [SerializeField] private SoundArrayHolder _touchStartHolder;
    [SerializeField] private SoundArrayHolder _touchEndHolder;
    [SerializeField] private SoundArrayHolder _activateSoundHolder;
    [Space(5), Header("Touch and interact sound holders")]
    [SerializeField] private VibrationSettingsSO _touchHapticSettings;

    private PlayerHand _touchingHand;
    protected PlayerHand TouchingHand => _touchingHand;
    private Collider _collider;
    private float _nextTimeAllowTouchVO;
    private bool _isActivated = false;

    public bool IsActivated => _isActivated;

    #region Core Functions
    public virtual void TouchStay(PlayerHand hand)
    {
        if(hand == null)
        {
            Debugger.Log($"Touching {gameObject.name}, but player hand is null");
        }
        hand.HandleTouchSlide(_touchHapticSettings);
    }
    public virtual void Touch(PlayerHand hand)
    {
        _touchingHand = hand;

        if(_touchStartHolder != null && _touchStartHolder.SoundArray != null && _touchStartHolder.SoundArray.Length != 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _touchStartHolder.SoundArray,
                                                        transform.position,
                                                        _touchStartHolder.LastPlayedSound,
                                                        true);
        }

        _touchingHand.HandleTouchBegin(_touchHapticSettings, _touchingHand.transform.position);

        if (_nextTimeAllowTouchVO < Time.time)
        {
            // Play Touch Dialogue for this object
            _nextTimeAllowTouchVO = Time.time + PlayerSettings.Developer.TouchDialogueInterval;
            AudioPlayer.PlaySoundAtPoint(this, _touchIdentifyVO, transform.position, true);
        }
    }
    private void PickUp(Transform parent, PlayerHand hand)
    {
        EventManager.OnCantCarryObject.Raise(this, -1);
        hand.HandlePickUpOrDropObject();
    }
    public virtual void Ping(float delay = 0)
    {

    }
    public virtual void Activate()
    {
        if (_activateSoundHolder != null && _activateSoundHolder.SoundArray != null && _activateSoundHolder.SoundArray.Length != 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                     _activateSoundHolder.SoundArray,
                                     transform.position,
                                     _activateSoundHolder.LastPlayedSound,
                                     true);
        }
        _isActivated = !_isActivated;
        EventManager.OnAnyInteractableActivated.Raise(this, this);
    }
    public virtual void EndTouch()
    {
        if (_touchingHand == null) return; // This guards against double calls if 
                                           // the object has multiple trigger
                                           // colliders so we get 2x Enter & Exit calls

        if (_touchEndHolder != null && _touchEndHolder.SoundArray != null && _touchEndHolder.SoundArray.Length != 0)
        {
                AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                _touchEndHolder.SoundArray,
                                                _touchingHand.transform.position,
                                                _touchEndHolder.LastPlayedSound,
                                                true);
        }
        _touchingHand.HandleTouchEnd(_touchHapticSettings);
        _touchingHand = null;
    }

    #endregion
    
    #region Unity Callbacks
    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }
    private void OnCollisionStay(Collision collision)
    {
        TouchStay(_touchingHand);
    }

    #endregion

    #region Interactable interface functions

    void Iinteractable.Activate()
    {
        Activate();
    }
    //void Iinteractable.CollideWithEnvironment()
    //{
        
    //}
    void Iinteractable.Release()
    {
        
    }
    void Iinteractable.EndTouch()
    {
        EndTouch();
    }
    void Iinteractable.Ping(float delay)
    {
        Ping(delay);
    }
    void Iinteractable.PickUp(Transform parent, PlayerHand hand)
    {
        PickUp(parent, hand);
    }

    void Iinteractable.Touch(PlayerHand hand)
    {
        Touch(hand);
    }

    #endregion
}
