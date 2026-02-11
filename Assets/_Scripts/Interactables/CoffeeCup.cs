using TMPro;
using UnityEngine;

public class CoffeeCup : PickUpObject
{
    [Space(15)]
    [SerializeField] private SoundArrayHolder _waterSpillSounds;
    [SerializeField] private float _spillVelocityThreshold = 10;

    private MeshRenderer _renderer;
    private float _timer = 0f;
    private float _spillInterval = 0.3f;
    private int _waterAmount = 3;

    public void FillCupWithWater()
    {
        _waterAmount = 3;
    }
    private void SpillAllWater(bool wasAccident)
    {
        if (wasAccident == true && _waterAmount > 0)
        {
            EventManager.OnPlayerCurse.Raise(this, 1);
        }

        for (int i = 0; i < _waterAmount; i++)
        {
            SpillWater();
        }
    }
    public void SpillWater()
    {
        _waterAmount--;

        Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue);
        float height = Vector3.Distance(transform.position, hitInfo.point);
        float delay = Mathf.Sqrt((height * 2) / Mathf.Abs(Physics.gravity.y));

        if (hitInfo.collider.TryGetComponent<BreakableMachine_Water>(out var machine))
        {
            machine.ReactToWater();
        }

        var sound = AudioPlayer.GetRandomSoundFromArray(_waterSpillSounds.SoundArray);
        AudioPlayer.PlayerSoundAtPointWithDelay(this, sound, hitInfo.point, delay, true);

        // Spawn ripple sphere maybe?
    }
    public override void HandlePlaceObjectOnSurface()
    {
        base.HandlePlaceObjectOnSurface();

        transform.up = Vector3.up;
    }

    #region Unity Callbacks
    public override void Awake()
    {
        base.Awake();
#if UNITY_EDITOR
        _renderer = GetComponent<MeshRenderer>();
#endif
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
        if (Velocity > _spillVelocityThreshold)
        {
            SpillAllWater(true);
        }
        // THIS BLOCK OF CODE IS CAUSING CURSING TO HAPPEN TWICE I THINK!

        GetComponentInChildren<TextMeshProUGUI>()?.SetText(_timer.ToString("F2") + "\nW:" + _waterAmount);

#if UNITY_EDITOR
        if (dot < 0.5f)
        {
            _renderer.material.color = Color.red;
        }
        else
        {
            _renderer.material.color = Color.white;
        }
#endif
    }
    #endregion
}

