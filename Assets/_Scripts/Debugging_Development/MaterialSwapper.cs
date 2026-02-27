using UnityEngine;

public class MaterialSwapper : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material blackMaterial;
    [SerializeField] private Material debugMaterial;

    public void ApplyBlackMaterial()
    {
        ApplyMaterial(blackMaterial);
    }

    public void ApplyDebugMaterial()
    {
        ApplyMaterial(debugMaterial);
    }

    private void ApplyMaterial(Material newMaterial)
    {
        if (newMaterial == null)
        {
            Debug.LogWarning("Material is not assigned.");
            return;
        }

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
#if UNITY_EDITOR
        foreach (MeshRenderer renderer in renderers)
        {
            UnityEditor.Undo.RecordObject(renderer, "Swap Materials");

            // Replace all materials in the renderer
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = newMaterial;
            }

            renderer.sharedMaterials = materials;

            UnityEditor.EditorUtility.SetDirty(renderer);
        }
#endif
    }
}