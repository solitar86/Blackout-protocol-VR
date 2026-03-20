using UnityEngine;

[CreateAssetMenu(fileName = "VibrationSettings", menuName = "Vibration Settings SO")]
public class VibrationSettingsSO : ScriptableObject
{
    [Tooltip("This is the min distance at which the haptic will trigger if sliding on a surface")]
	[SerializeField] public float DistanceInterval = 1;

	[SerializeField, Range(0f, 1f)] public float Amplitude = 1;
	[SerializeField, Range(0.001f, 5f)] public float Duration = 1;

	[SerializeField] public float Frequency = 250;
    [SerializeField, Range(1,10)] public int RepeatTimes = 1;
    [Tooltip("This is the time between repeats")]
    [SerializeField] public float TimeInterval = 0.25f;


    private void OnValidate()
    {
        if (Duration > DistanceInterval)
        {
            Debugger.LogWarning("<color=#6CE322> Duration must be less than interval</color>");
            Duration = DistanceInterval - 0.001f;
        }

        if (Duration < 0)
        {
            Duration = 0;
        }

        if (DistanceInterval < 0)
        {
            DistanceInterval = 0;
        }

        if (RepeatTimes == 0) RepeatTimes = 1;
    }
}