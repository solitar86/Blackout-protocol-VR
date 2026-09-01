using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class TriggerVolumeWithUnityEvent : MonoBehaviour
{
    [SerializeField] private string _volumeName;
    [SerializeField] private Color _volumeGizmoColor;
    [SerializeField] private bool _deleteTriggerOnEnter, _deleteTriggerOnExit;
    [Space(20)]
    [SerializeField] private UnityEvent _onEnterEvent, _onExitEvent;

    #region UnityCallbacks
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerSettings.Accessibility.LocationVOEnabled == false) return;
        if (other.TryGetComponent<Player>(out _))
        {
            _onEnterEvent?.Invoke();
            if (_deleteTriggerOnEnter) Destroy(gameObject);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (PlayerSettings.Accessibility.LocationVOEnabled == false) return;
        if (other.TryGetComponent<Player>(out _))
        {
            _onExitEvent?.Invoke();
            if (_deleteTriggerOnExit) Destroy(gameObject);
        }
    }

    private void Reset()
    {
        gameObject.name = "TRIGGERVOLUME: " + _volumeName;
    }

    private void OnValidate()
    {
        gameObject.name = "TRIGGERVOLUME: " + _volumeName;
    }
    private void OnDrawGizmosSelected()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            if (col == null) continue;

            Transform t = col.transform;

            // Save matrix
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // Match collider transform
            Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);

            if (col is BoxCollider box)
            {
                Gizmos.color = _volumeGizmoColor;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.color = _volumeGizmoColor;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }

            // Restore matrix
            Gizmos.matrix = oldMatrix;

#if UNITY_EDITOR
            // Draw label at the collider object's position
            Handles.Label(t.position, _volumeName);
#endif
        }
    }
}
