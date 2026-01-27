using UnityEngine;
using UnityEngine.XR;

public class Hammer : PickUpObject
{
    [Tooltip("What velocity == volume 1f")]
    [SerializeField] private float _velocityToVolumeCap = 10f;
    [Tooltip("Curve is built on Awake() based on VelocityToVolumeCap")]
    [SerializeField] private AnimationCurve _velocityToVolumeCurve;
    [SerializeField] private Sound _impactSound;

    private bool _isHeld;
    private Vector3 _previousPosition;
    private float _velocity;

    public bool IsHeld => _isHeld;
    public float Velocity => _velocity;

    public override void Awake()
    {
        base.Awake();
        _velocityToVolumeCurve = new AnimationCurve(
        new Keyframe(0f, 0.01f),
        new Keyframe(_velocityToVolumeCap, 1f));
    }

    public override void Update()
    {
        base.Update();

        _velocity = (_previousPosition - transform.position).magnitude / Time.deltaTime;
        _previousPosition = transform.position;

    }
    public override void Drop()
    {
        base.Drop();
        _isHeld = false;
    }

    public override void PickUp(Transform parent)
    {
        base.PickUp(parent);
        _isHeld = true;
    }

    public override void CollideWithObject()
    {
        // CONSIDER MOVING THIS SOUND LOGIC TO PARENT PICKUP OBJECT CLASS
        base.CollideWithObject();
        Sound impactWithModVolume = new Sound(_impactSound);
        impactWithModVolume.Volume = _velocityToVolumeCurve.Evaluate(Velocity);
        AudioPlayer.PlaySoundAtPoint(this, impactWithModVolume, transform.position, true);
    }
}
