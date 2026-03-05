using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Random = UnityEngine.Random;

public class PlayerFootStepHandler : MonoBehaviour
{
    [SerializeField] LayerMask _whatCountsAsGround;
    [SerializeField] private float _feetSeparationDistance = 0.3f;
    [SerializeField] private SoundArrayHolder _defaultFootSteps;
    [SerializeField] private float _footStepSoundDistanceInterval = 0.4f;
    [Header("Snap turn sound settings")]
    [Tooltip("Snap turn playes 2 footsteps at random delay. Set min and max here:")]
    [SerializeField] private float _minDelayBetweenShuffle = 0.05f;
    [Tooltip("Snap turn playes 2 footsteps at random delay. Set min and max here:")]
    [SerializeField] private float _maxDelayBetweenShuffle = 1f;
    
    private bool _isLeftFoot = true; // Player starts with left foot step.

    public GameEvent<int> OnPlayerTakeFootstep = new("Player Footstep");

    private Vector3 lastStepPosition;

    #region Unity Callbacks
    private void OnEnable()
    {
        lastStepPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        //SnapTurnProvider.OnPlayerSnapTurn += HandlePlayerSnapTurnFootSteps;
        CustomSnapTurnProviderWrapper.OnPlayerSnapTurn += HandlePlayerSnapTurnFootSteps;
        EventManager.OnPlayerStartMove.AddListener(this, HandlePlayerStartMove);
    }
    private void OnDisable()
    {
        //SnapTurnProvider.OnPlayerSnapTurn -= HandlePlayerSnapTurnFootSteps;
        CustomSnapTurnProviderWrapper.OnPlayerSnapTurn -= HandlePlayerSnapTurnFootSteps;
        EventManager.OnPlayerStartMove.RemoveListener(this, HandlePlayerStartMove);
    }
    private void Update()
    {
        Vector3 currentXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        float distance = Vector3.Distance(currentXZ, lastStepPosition);
        if (distance >= _footStepSoundDistanceInterval)
        {
            lastStepPosition = currentXZ;
            HandleAppropriateFootStepSound();
        }
    }
    #endregion
    private void HandleAppropriateFootStepSound()
    {
        if (Physics.Raycast(transform.position,
                            Vector3.down,
                            out RaycastHit hitInfo,
                            float.MaxValue,
                            _whatCountsAsGround, QueryTriggerInteraction.Collide))
        {
            SoundArrayHolder footStepSounds = GetAppripriateFootStepArray(hitInfo);
            var position = CalculateFootStepPosition(hitInfo.point, transform);
            footStepSounds.LastPlayedSound = AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                                footStepSounds.SoundArray,
                                                                hitInfo.point,
                                                                footStepSounds.LastPlayedSound,
                                                                true);
            OnPlayerTakeFootstep.Raise(this, -1);
        }
    }
    private SoundArrayHolder GetAppripriateFootStepArray(RaycastHit hitInfo)
    {
        SoundArrayHolder footsteps;
        hitInfo.collider.TryGetComponent<FootstepSurface>(out var surface);

        if (surface != null)
            footsteps = surface.GetFootStepSounds();
        else
            footsteps = _defaultFootSteps;

        return footsteps;
    }
    private void HandlePlayerSnapTurnFootSteps(bool wasRightTurn)
    {
        //////////////////////////////////////////////////////////////////////
        // This function playes a shuffling sound at the players
        // position whenever a turn is executed im the SnapTurn provider class.
        //////////////////////////////////////////////////////////////////////

        if (Physics.Raycast(transform.position,
                            Vector3.down,
                            out RaycastHit hitInfo,
                            float.MaxValue,
                            _whatCountsAsGround, QueryTriggerInteraction.Collide))
        {
            SoundArrayHolder footStepSounds = GetAppripriateFootStepArray(hitInfo);
            float delay = 0f;
            for (int i = 0; i < 2; i++)
            {
                this.CallWithDelay(() =>
                {
                    footStepSounds.LastPlayedSound = AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                                footStepSounds.SoundArray,
                                                                hitInfo.point,
                                                                footStepSounds.LastPlayedSound,
                                                                true);
                }, delay);
                delay += Random.Range(_minDelayBetweenShuffle, _maxDelayBetweenShuffle);
            }
        }
    }
    private void HandlePlayerStartMove(int value)
    {
        HandleAppropriateFootStepSound();
    }
    private Vector3 CalculateFootStepPosition(Vector3 pointOnGround, Transform transform)
    {
        // This function controls how far from the players position
        // to the left or right the footstep is. This is so
        // we can exaggerate the spatial positioning of footsteps
        // left or right even beyond what is "realistic".

        pointOnGround += transform.right * (_isLeftFoot ? -1f : 1f) * _feetSeparationDistance;
        _isLeftFoot = !_isLeftFoot;

        if (Debugger.isEnabled) Debug.DrawRay(pointOnGround, Vector3.up * 0.5f, Color.green, 5f);

        return pointOnGround;
    }
    
#if UNITY_EDITOR
    public void ForceFootStepValues(float footStepSoundDistanceInterval, float feetSeparationDistance)
    {
        _feetSeparationDistance = footStepSoundDistanceInterval;
        _feetSeparationDistance = feetSeparationDistance;
    }
    public float GetFootStepInterval() => _footStepSoundDistanceInterval;
    public float GetFeetSeparationDistance() => _feetSeparationDistance;
#endif
}
