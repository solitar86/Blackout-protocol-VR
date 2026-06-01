using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
public class LocationVOTriggerVolume : MonoBehaviour
{
    [SerializeField] private Sound _voLocationSound;

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerSettings.Accessibility.LocationVOEnabled == false) return;
        if (other.TryGetComponent<Player>(out _))
        {
            EventManager.OnPlayerLocationIDShouldPlay.Raise(this, _voLocationSound);
        }

    }


    private void OnValidate()
    {
        if (_voLocationSound != null)
        {
            var mixer = Resources.Load<AudioMixer>("MainMixer");
            _voLocationSound.Mixergroup = mixer.FindMatchingGroups("InnerMonologue")[0];
            _voLocationSound.SpacialBlend = 0;
        }

    }

    private void OnDrawGizmosSelected()
    {
        var collider = GetComponent<Collider>();

        if (collider == null) return;

        Transform t = collider.transform;

        // Save matrix
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // Match collider transform
        Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);

        if (collider is BoxCollider box)
        {
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.25f);
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (collider is SphereCollider sphere)
        {
            Gizmos.color = new Color(0.5f, 0f, 1f, 0.9f);
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }

        // Restore matrix
        Gizmos.matrix = oldMatrix;

#if UNITY_EDITOR
        // Draw label at the collider object's position
        Handles.Label(t.position, collider.gameObject.name);
#endif

    }
}

