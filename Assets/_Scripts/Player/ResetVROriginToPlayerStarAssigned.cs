using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetVROriginToPlayerStartIfAssigned : MonoBehaviour
{
    [SerializeField] private Transform _VR_Origin_Transform;
    [SerializeField] private Transform _startTransform;
    [SerializeField] private Transform _playerTransform;
    private IEnumerator Start()
    {
        if (_startTransform == null) yield break;
        Player.Instance.RecenterPlayerWithNoHeightChange(_startTransform.position, _startTransform.forward);
        yield return null;
    }
}
