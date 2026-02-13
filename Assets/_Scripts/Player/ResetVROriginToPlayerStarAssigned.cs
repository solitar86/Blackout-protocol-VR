using System.Collections;
using UnityEngine;

public class ResetVROriginToPlayerStartIfAssigned : MonoBehaviour
{
    [SerializeField] private Transform _VR_Origin_Transform;
    [SerializeField] private Transform _startTransform;
    [SerializeField] private Transform _playerTransform;
    private IEnumerator Start()
    {
        // This has to be delayd or some XR function
        // Sets us in the default position based on
        // Where we are within the playspace.
        yield return new WaitForEndOfFrame();

        _playerTransform = Camera.main.transform;

        if (_VR_Origin_Transform != null && _startTransform != null && _playerTransform != null)
        {
            Vector3 playerOffset = _playerTransform.position - _VR_Origin_Transform.position;
            playerOffset.y = 0; // ignore Y axis

            Vector3 newOriginPosition = _startTransform.position - playerOffset;
            _VR_Origin_Transform.position = newOriginPosition;
            _VR_Origin_Transform.rotation = _startTransform.rotation;
        }
    }
}
