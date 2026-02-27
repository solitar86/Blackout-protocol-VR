using UnityEngine;

[CreateAssetMenu(fileName = "PickUp Hold OffsetSettings", menuName = "New Hold Offset Settings")]
public class HoldPickupOffsetSettingsSO : ScriptableObject
{
    public Vector3 RotationOffset = Vector3.zero;
    public Vector3 PositionOffset = Vector3.zero;
}
