using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

[ExecuteAlways]
public class CustomRollOff : MonoBehaviour
{
    public static CustomRollOff Instance { get; private set; }

    [Header("Curve Shape Settings")]
    [Tooltip("Higher values = steeper early drop")]
    [Range(0.1f, 200f)]
    [SerializeField] private float steepness = 4f;

    [Tooltip("Number of curve samples")]
    [Range(5, 100)]
    [SerializeField] private int resolution = 30;

    [Header("Preview (Inspector Only)")]
    [SerializeField] private float testMinDistance = 1f;
    [SerializeField] private float testMaxDistance = 20f;

    [SerializeField] private AnimationCurve previewCurve;

    #region UnityCallbacks
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    #endregion

    public AnimationCurve GetLogCurve(float minDistance, float maxDistance, bool invert = false)
    {
        return GenerateLogCurve(minDistance, maxDistance, invert);
    }
    private AnimationCurve GenerateLogCurve(float minDistance, float maxDistance, bool invert = false)
    {
        AnimationCurve curve = new AnimationCurve();

        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)resolution;

            // If invert is true, flip how the curve behaves
            float curveT = invert ? 1f - t : t;

            float distance = Mathf.Lerp(minDistance, maxDistance, t);

            float volume = 1f - Mathf.Log10(1f + steepness * curveT)
                               / Mathf.Log10(1f + steepness);

            curve.AddKey(distance, Mathf.Clamp01(volume));
        }

        // End key (match the curve behavior)
        float endValue = invert ? 1f : 0f;

        Keyframe endKey = new Keyframe(maxDistance, endValue);
        endKey.inTangent = 0f;
        endKey.outTangent = 0f;

        curve.AddKey(endKey);

        return curve;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (testMaxDistance <= testMinDistance)
            testMaxDistance = testMinDistance + 0.01f;

        previewCurve = GenerateLogCurve(testMinDistance, testMaxDistance);
    }
#endif
}