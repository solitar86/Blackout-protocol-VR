using System;
using UnityEngine;

[RequireComponent (typeof(VibrationPlayerDirect))]
public class TouchRippleSpawner : MonoBehaviour
{
    [SerializeField] private Transform _rippleSphere;
    [SerializeField] private int _numberToSpawn = 1;
    [SerializeField, Range(0.001f, 2f)] private float _spawnInterval = 0.1f;

    public void SpawnTouchVisual(Vector3 position)
    {
        for (int i = 0; i < _numberToSpawn; i++)
        {
            this.CallWithDelay(() => SpawnSphere(position), _spawnInterval * i);
        }
        
    }
    private void SpawnSphere(Vector3 position)
    {
        Instantiate(_rippleSphere, position, Quaternion.identity);
    }
}
