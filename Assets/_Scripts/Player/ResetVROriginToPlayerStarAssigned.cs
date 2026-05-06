using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This is a Monobehavior which can reset the player on sceneload
/// and is used as an anchor if the player recenters themselves.
/// </summary>
public class RecenterPositionAndStartRecenterOnLoad : MonoBehaviour
{
    [SerializeField] private bool _recenterOnSceneLoad = true;
    [Tooltip("The player VR origin transform")]
    [SerializeField] private Transform _VR_Origin_Transform;
    [Tooltip("The transform to which to match VR origin position and forward vector")]
    [SerializeField] private Transform _startTransform;
    private IEnumerator Start()
    {
        if (_recenterOnSceneLoad == false) yield break;
        if (_startTransform == null) yield break;
        Player.Instance.RecenterPlayerWithNoHeightChange(_startTransform.position, _startTransform.forward);
        yield return null;
    }
}
