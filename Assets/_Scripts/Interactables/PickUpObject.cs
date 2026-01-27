using System;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

[SelectionBase]
public abstract class PickUpObject : MonoBehaviour, Iinteractable
{
    [SerializeField] private Sound _touchIdentifyVO;
    [Space(5)]
    [SerializeField] private PickUpHoldOffsetSettings _offsetSettings;
    [Space(5)]
    [SerializeField] private SoundArrayHolder _pickUpSounds;

    private float _nextTimeAllowTouchVO = 0f;

    private Collider _collider;
    private Rigidbody _ridibody;
    private Transform parentTransformReference = null;

    public virtual void Awake()
    {
        _collider = GetComponent<Collider>();
        _ridibody = GetComponent<Rigidbody>();
        _ridibody.isKinematic = false;
    }
    #region Unity Callbacks
    public virtual void Update()
    {
        // In case we need this at some point
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
        CollideWithObject();
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
    }

    public virtual void CollideWithObject()
    {
        // DO SOMETHING
        // PLAY SOUND ETC
    }

    public virtual void PickUp(Transform parent)
    {
        transform.SetParent(parent);
        parentTransformReference = parent;
    }
    public virtual void Touch()
    {
        EventManager.OnPlayerTouchPickUp.Raise(this, this);
        if(_nextTimeAllowTouchVO < Time.time)
        {
            // Play Touch Dialogue for this object
            _nextTimeAllowTouchVO = Time.time + PlayerSettings.Developer.TouchDialogueInterval;
            AudioPlayer.PlaySoundAtPoint(this, _touchIdentifyVO, transform.position, true);
        }
    }
    public virtual void EndTouch()
    {
        // Do Something
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
        CollideWithObject();
    }

    void Iinteractable.PickUp(Transform parent)
    {
        Debugger.Log("Picking up " + gameObject.name, gameObject);
        PickUp(parent);
    }

    void Iinteractable.Touch()
    {
        //Debugger.Log("Touching " + gameObject.name, gameObject);
        Touch();
    }

    void Iinteractable.EndTouch()
    {
        //Debugger.Log("Stopped touching " + gameObject.name, gameObject);
        EndTouch();
    }



    #endregion
}
