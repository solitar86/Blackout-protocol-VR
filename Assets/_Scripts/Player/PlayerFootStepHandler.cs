using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class PlayerFootStepHandler : MonoBehaviour
{
    [SerializeField] LayerMask _whatCountsAsGround;
    [SerializeField] private float _feetSeparationDistance = 0.3f;
    [SerializeField] private SoundArrayHolder _defaultFootSteps;
    [SerializeField] private SoundArrayHolder _carpetFootsteps;
    [SerializeField] private SoundArrayHolder _snapTurnFootSounds;
    [SerializeField] private float _footStepSoundDistanceInterval = 0.4f;
    private bool _isLeftFoot = true; // Player starts with left foot step.

    public GameEvent<int> OnPlayerTakeFootstep = new("Player Footstep");

    private Vector3 lastStepPosition;

    #region Unity Callbacks
    private void OnEnable()
    {
        lastStepPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        SnapTurnProvider.OnPlayerSnapTurn += HandlePlayerSnapTurnFootSteps;
        EventManager.OnPlayerStartMove.AddListener(this, HandlePlayerStartMove);
    }

    private void OnDisable()
    {
        SnapTurnProvider.OnPlayerSnapTurn -= HandlePlayerSnapTurnFootSteps;
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
                            _whatCountsAsGround))
        {

            SoundArrayHolder footStepSounds = GetAppripriateFootStepArray();
            var position = CalculateFootStepPosition(hitInfo.point, transform);
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        footStepSounds.SoundArray,
                                                        hitInfo.point,
                                                        footStepSounds.LastPlayedSound,
                                                        true);
            OnPlayerTakeFootstep.Raise(this, -1);
        }
    }
    private SoundArrayHolder GetAppripriateFootStepArray()
    {
        return _defaultFootSteps;
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
                    _whatCountsAsGround))
        {
            _snapTurnFootSounds.LastPlayedSound = AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _snapTurnFootSounds.SoundArray,
                                                        hitInfo.point,
                                                        _snapTurnFootSounds.LastPlayedSound,
                                                        true);
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

        if(Debugger.isEnabled) Debug.DrawRay(pointOnGround, Vector3.up * 0.5f, Color.green, 5f);

        return pointOnGround;
    }

#if UNITY_EDITOR
    public void ForceFootStepValues(float footStepSoundDistanceInterval,  float feetSeparationDistance)
    {
        _feetSeparationDistance = footStepSoundDistanceInterval;
        _feetSeparationDistance = feetSeparationDistance;
    }
    public float GetFootStepInterval() => _footStepSoundDistanceInterval;
    public float GetFeetSeparationDistance() => _feetSeparationDistance;
#endif
}
