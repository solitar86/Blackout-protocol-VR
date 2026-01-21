using System;
using UnityEngine;

[RequireComponent (typeof(VibrationPlayerDirect))]
public class TouchRippleSpawner : MonoBehaviour
{
    [SerializeField] private Transform _rippleSphere;

    public void SpawnTouchVisual(Vector3 position)
    {
        Instantiate(_rippleSphere, position, Quaternion.identity);
    }
}
