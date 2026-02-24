using UnityEditor;
using UnityEngine;

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

    public AnimationCurve GetLogCurve(float minDistance, float maxDistance)
    {
        return GenerateLogCurve(minDistance, maxDistance);
    }
    private AnimationCurve GenerateLogCurve(float minDistance, float maxDistance)
    {
        AnimationCurve curve = new AnimationCurve();

        // Sample up to second-to-last point
        for (int i = 0; i < resolution; i++)
        {
            float t = i / (float)resolution;
            float distance = Mathf.Lerp(minDistance, maxDistance, t);

            float volume = 1f - Mathf.Log10(1f + steepness * t)
                               / Mathf.Log10(1f + steepness);

            curve.AddKey(distance, Mathf.Clamp01(volume));
        }

        Keyframe endKey = new Keyframe(maxDistance, 0f);
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