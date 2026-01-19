using UnityEngine;

[CreateAssetMenu(fileName = "VibrationSettings", menuName = "Vibration Settings SO")]
public class VibrationSettingsSO : ScriptableObject
{
	[SerializeField] public float _distanceInterval = 1;
	[SerializeField, Range(0f, 1f)] public float _amplitude = 1;
	[SerializeField, Range(0.001f, 5f)] public float _duration = 1;
	[SerializeField] public float _frequency = 1;


    private void OnValidate()
    {
        if (_duration > _distanceInterval)
        {
            Debug.Log("<color=#6CE322> Duration must be less than interval</color>");
            _duration = _distanceInterval - 0.001f;
        }

        if (_duration < 0)
        {
            _duration = 0;
        }

        if (_distanceInterval < 0)
        {
            _distanceInterval = 0;
        }
    }
}