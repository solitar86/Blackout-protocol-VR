using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent (typeof(VibrationPlayerDirect))]
public class TouchRippleSpawner : MonoBehaviour
{
    //TODO This class could very well just be
    //a static class, doesn't need to be a Monobehavior

    [SerializeField] private Transform _rippleSphere;
    [SerializeField] private int _numberToSpawn = 1;
    [SerializeField, Range(0.001f, 2f)] private float _spawnInterval = 0.1f;

    public static TouchRippleSpawner spawner;
    private ObjectPool<GameObject> _rippleSphereGOPool;
    private Transform _rippleSphereParent;

    #region Unity Callbacks
    private void Awake()
    {
        _rippleSphereParent = new GameObject("RippleSphereParent").transform;
    }
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
    #endregion

    #region Object pool Delegates
    private GameObject CreateSphere()
    {
        var go = Instantiate(_rippleSphere, _rippleSphereParent).gameObject;
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
    #endregion

    /// <summary>
    /// Used for external calls to touch ripple spawners in
    /// rare cases such as spilling water.
    /// </summary>
    /// <param name="position"></param>
    public static void SpawnTouchVisualStatic(Vector3 position)
    {
        if(spawner == null)
        {
            TouchRippleSpawner spawner = FindFirstObjectByType<TouchRippleSpawner>();
        }
        spawner?.SpawnTouchVisual(position);
    }

    public void SpawnTouchVisual(Vector3 position)
    {
        if (PlayerSettings.Accessibility.TouchRipple == false) return;

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
