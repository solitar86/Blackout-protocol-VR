using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Sound))]
public class SoundDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect rect = new Rect(position.x, position.y, position.width, lineHeight);

        // Foldout
        property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            rect.y += lineHeight + spacing;
            Draw(ref rect, property, nameof(Sound.Clip));

            Draw(ref rect, property, nameof(Sound.Mixergroup));
            Draw(ref rect, property, nameof(Sound.Volume));
            Draw(ref rect, property, nameof(Sound.Pitch));
            Draw(ref rect, property, nameof(Sound.SpacialBlend));

            rect.y += spacing;

            var overrideProp = property.FindPropertyRelative(nameof(Sound.OverrideDefaultDistances));
            EditorGUI.PropertyField(rect, overrideProp);
            rect.y += lineHeight + spacing;

            if (overrideProp.boolValue)
            {
                rect.y += lineHeight + spacing;
                Draw(ref rect, property, nameof(Sound.MinDistance));
                Draw(ref rect, property, nameof(Sound.MaxDistance));
            }

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUI.EndProperty();
    }

    void Draw(ref Rect rect, SerializedProperty property, string fieldName)
    {
        SerializedProperty prop = property.FindPropertyRelative(fieldName);

        float height = EditorGUI.GetPropertyHeight(prop, true);
        rect.height = height;

        EditorGUI.PropertyField(rect, prop, true);

        rect.y += height + EditorGUIUtility.standardVerticalSpacing;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (!property.isExpanded)
            return height;

        float spacing = EditorGUIUtility.standardVerticalSpacing;

        foreach (var field in new[]
        {
            nameof(Sound.Clip),
            nameof(Sound.Mixergroup),
            nameof(Sound.Volume),
            nameof(Sound.Pitch),
            nameof(Sound.SpacialBlend),
            nameof(Sound.OverrideDefaultDistances)
        })
        {
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(field), true) + spacing;
        }

        if (property.FindPropertyRelative(nameof(Sound.OverrideDefaultDistances)).boolValue)
        {
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Sound.MinDistance)), true) + spacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(Sound.MaxDistance)), true) + spacing;
        }

        return height;
    }
}
