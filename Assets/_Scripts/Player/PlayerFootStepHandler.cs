using UnityEngine;

public class PlayerFootStepHandler : MonoBehaviour
{
    [SerializeField] LayerMask _whatCountsAsGround;
    [SerializeField] private float _feetSeparationDistance = 0.3f;
    [SerializeField] private SoundArrayHolder _defaultFootSteps;
    [SerializeField] private float _footStepSoundDistanceInterval = 0.4f;
    private bool _isLeftFoot = true; // Player starts with left foot step.


    private Vector3 lastStepPosition;

    private void Start()
    {
        lastStepPosition = new Vector3(transform.position.x, 0f, transform.position.z);
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

    private void HandleAppropriateFootStepSound()
    {

        if (Physics.Raycast(transform.position,
                            Vector3.down,
                            out RaycastHit hitInfo,
                            float.MaxValue,
                            _whatCountsAsGround))
        {

            var position = CalculateFootStepPosition(hitInfo.point, transform);

            // Store played footstep as previous in Scriptable Object so we don't repeat it. 
            _defaultFootSteps.LastPlayedSound = AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _defaultFootSteps.SoundArray,
                                                        hitInfo.point,
                                                        _defaultFootSteps.LastPlayedSound,
                                                        true);
        }

    }

    private Vector3 CalculateFootStepPosition(Vector3 pointOnGround, Transform transform)
    {
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
