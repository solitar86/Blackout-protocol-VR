using TMPro;
using UnityEngine;

public class CoffeeCup : PickUpObject
{
    [Space(15), Header("Coffee cup specific settings")]
    [SerializeField] private LayerMask _layersToLookForWaterInteractables;
    [SerializeField] private float _breakOnImpactVelocityThreshold = 6;
    [SerializeField] private Sound _cupFullIDVO;
    [SerializeField] private Sound _spillWaterVO;
    [SerializeField] private Sound _glassBreakSound;
    [SerializeField] private SoundArrayHolder _waterSpillSounds;
    [SerializeField] private float _spillVelocityThreshold = 10;

    private float _timer = 0f;
    private float _spillInterval = 0.3f;
    private int _waterAmount = 0;
    public bool IsFull => _waterAmount != 0;

    public void FillCupWithWater()
    {
        if(_waterAmount != 3)
        {
            EventManager.OnGeneralVOShouldPlay.Raise(this, _cupFullIDVO);
        }
        _waterAmount = 3;
    }
    private void SpillAllWater(bool wasAccident)
    {
        if (wasAccident == true && _waterAmount > 0)
        {
            EventManager.OnPlayerSpillAllWater.Raise(this, _spillWaterVO);
        }

        for (int i = 0; i < _waterAmount; i++)
        {
            SpillWater();
        }
    }
    public void SpillWater()
    {
        _waterAmount--;

        Physics.Raycast(transform.position,
                        Vector3.down,
                        out RaycastHit hitInfo,
                        float.MaxValue,
                        _layersToLookForWaterInteractables,
                        QueryTriggerInteraction.UseGlobal);

        float height = Vector3.Distance(transform.position, hitInfo.point);
        float delay = Mathf.Sqrt((height * 2) / Mathf.Abs(Physics.gravity.y));

        if (hitInfo.collider.TryGetComponent<BreakableMachine_Water>(out var machine))
        {
            machine.ReactToWater();
        }

        var sound = AudioPlayer.GetRandomSoundFromArray(_waterSpillSounds.SoundArray);
        AudioPlayer.PlaySoundAtPointWithDelay(this, sound, hitInfo.point, delay, true);

        TouchRippleSpawner.SpawnTouchVisualStatic(hitInfo.point);
    }
    public override void HandleObjectPlacementAfterDrop()
    {
        base.HandleObjectPlacementAfterDrop();

        transform.up = Vector3.up;
    }
    public override void HandleSpecialCasesForHittingFloor(Vector3 dropPosition, float delay)
    {
        AudioPlayer.PlaySoundAtPointWithDelay(this, _glassBreakSound, dropPosition, delay, usePitchVariation:false);
    }
    public override void HandleCollisiondWithEnvironment(GameObject environmentObject)
    {
        base.HandleCollisiondWithEnvironment(environmentObject);

        if(Velocity > _breakOnImpactVelocityThreshold)
        {
            AudioPlayer.PlaySoundAtPoint(this, _glassBreakSound, transform.position, usePitchVariation: false);
            ForceRemoveObjectFromHandAndReturnToStartPosition(HoldingHand);
        }
    }

    #region Unity Callbacks
    public override void Awake()
    {
        base.Awake();
    }
    public override void Update()
    {
        base.Update();
        float dot = Vector3.Dot(transform.up, Vector3.up);

        if (dot < 0.5f)
        {
            _timer += Time.deltaTime * (1f - Mathf.Abs(dot));
            if (_timer > _spillInterval)
            {
                _timer -= _spillInterval;
                if (_waterAmount > 0) SpillWater();
            }
        }
        else
        {
            _timer = 0f;
        }

        // THIS BLOCK OF CODE IS CAUSING CURSING TO HAPPEN TWICE I THINK!
        if (Mathf.Sign(dot) == -1)
        {
            SpillAllWater(false);
        }
        if (CustomSnapTurnProviderWrapper.IsSnapTurning == false && Velocity > _spillVelocityThreshold)
        {
            // This is causing snapturning to cause water spilling.
            SpillAllWater(true);
        }
        // THIS BLOCK OF CODE IS CAUSING CURSING TO HAPPEN TWICE I THINK!

    }
    #endregion
}

