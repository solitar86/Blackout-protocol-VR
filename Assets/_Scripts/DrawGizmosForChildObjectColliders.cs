
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class DrawGizmosForChildObjectColliders : MonoBehaviour
{

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
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.25f);
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.color = new Color(0.5f, 0f, 1f, 0.9f);
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }

            // Restore matrix
            Gizmos.matrix = oldMatrix;

#if UNITY_EDITOR
            // Draw label at the collider object's position
            Handles.Label(t.position, col.gameObject.name);
#endif
        }
    }
}
