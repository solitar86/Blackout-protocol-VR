using UnityEngine;

public class SetCustomRollOffOnStart : MonoBehaviour
{
    void Start()
    {
        var source = GetComponent<AudioSource>();
        source.SetCustomCurve(AudioSourceCurveType.CustomRolloff,
                        CustomRollOff.Instance.GetLogCurve(source.minDistance, source.maxDistance));
    }
}
