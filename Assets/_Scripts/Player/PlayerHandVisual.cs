using UnityEngine;

public class PlayerHandVisual : MonoBehaviour
{
    [SerializeField] Transform _followTarget;
    private Rigidbody _rigidbody;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {

        _rigidbody.linearVelocity = (_followTarget.position - _rigidbody.position) / Time.fixedDeltaTime;
        Quaternion rotationDifference = _followTarget.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);
        Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;
        _rigidbody.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);

        // Half life alyx logic here waiting for inputhandler bugs to be fixed.
        //if (PlayerInputHandler.PlayerIsMoving == false)
        //{
        //    if (_rigidbody.isKinematic == false) _rigidbody.isKinematic = true;
        //    _rigidbody.linearVelocity = (_followTarget.position - _rigidbody.position) / Time.fixedDeltaTime;
        //    Quaternion rotationDifference = _followTarget.rotation * Quaternion.Inverse(transform.rotation);
        //    rotationDifference.ToAngleAxis(out float angleInDegree, out Vector3 rotationAxis);
        //    Vector3 rotationDifferenceInDegree = angleInDegree * rotationAxis;
        //    _rigidbody.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
        //}
        //else
        //{
        //    if (_rigidbody.isKinematic == true) _rigidbody.isKinematic = false;
        //    _rigidbody.position = _followTarget.position;
        //    _rigidbody.rotation = _followTarget.rotation;
        //    Debugger.Log("Player is moving");
        //}
    }

}
