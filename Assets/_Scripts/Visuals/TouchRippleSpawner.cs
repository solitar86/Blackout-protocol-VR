using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

[RequireComponent (typeof(VibrationPlayerDirect))]
public class TouchRippleSpawner : MonoBehaviour
{
    //TODO This class could very well just be
    //a static class, doesn't need to be a Monobehavior

    [SerializeField] private Transform _rippleSphere;
    [SerializeField] private int _numberToSpawn = 1;
    [SerializeField, Range(0.001f, 2f)] private float _spawnInterval = 0.1f;

    private ObjectPool<GameObject> _rippleSphereGOPool;

    private void Start()
    {
        _rippleSphereGOPool = new ObjectPool<GameObject>(
            createFunc: CreateSphere,
            actionOnGet: OnGetSphere,
            actionOnRelease: OnRelease,
            actionOnDestroy: OnDestroySphere,
            collectionCheck: true,
            defaultCapacity: 18,
            maxSize: 54
            );
    }

    private GameObject CreateSphere()
    {
        var go = Instantiate(_rippleSphere, Vector3.zero, Quaternion.identity).gameObject;
        return go;
    }

    private void OnGetSphere(GameObject go)
    {
        go.SetActive(true);
    }

    private void OnRelease(GameObject go)
    {
        var sphere = go.GetComponent<RippleSphere>();
        sphere.ResetSphere();
    }

    private void OnDestroySphere(GameObject go)
    {
        Destroy(go);
    }


    public void SpawnTouchVisual(Vector3 position)
    {
        if (PlayerSettings.Accessibility.Enabled == false) return;

        for (int i = 0; i < _numberToSpawn; i++)
        {
            this.CallWithDelay(() => SpawnSphere(position), _spawnInterval * i);
        }
        
    }
    private void SpawnSphere(Vector3 position)
    {
        var go = _rippleSphereGOPool.Get();
        go.transform.position = position;
    }
}
