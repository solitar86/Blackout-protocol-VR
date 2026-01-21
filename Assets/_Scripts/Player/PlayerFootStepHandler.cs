using System.Diagnostics;
using UnityEngine;

public class PlayerFootStepHandler : MonoBehaviour
{
    [SerializeField] LayerMask _whatCountsAsGround;
    [SerializeField] private float stepDistance = 0.4f;
    [SerializeField] private SoundArrayHolder _defaultFootSteps;
  

    private Vector3 lastStepPosition;

    private void Start()
    {
        lastStepPosition = new Vector3(transform.position.x, 0f, transform.position.z);
    }

    private void Update()
    {
        Vector3 currentXZ = new Vector3(transform.position.x, 0f, transform.position.z);
        float distance = Vector3.Distance(currentXZ, lastStepPosition);
        if (distance >= stepDistance)
        {
            lastStepPosition = currentXZ;
            HandleAppropriateFootStepSound();
        }
    }

    private void HandleAppropriateFootStepSound()
    {
        AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                    _defaultFootSteps.SoundArray,
                                                    transform.position,
                                                    _defaultFootSteps.LastPlayedSound,
                                                    true);
    }
}
