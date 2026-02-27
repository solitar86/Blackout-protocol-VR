using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MaterialSwapper))]
public class MaterialSwapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MaterialSwapper swapper = (MaterialSwapper)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Material Swap Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Black Material"))
        {
            swapper.ApplyBlackMaterial();
        }

        if (GUILayout.Button("Debug Material"))
        {
            swapper.ApplyDebugMaterial();
        }
    }
}