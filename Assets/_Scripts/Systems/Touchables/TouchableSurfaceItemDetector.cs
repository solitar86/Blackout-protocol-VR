using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TouchableSurface))]
public class TouchableSurfaceItemDetector : MonoBehaviour
{
    [SerializeField] private float _addedDetectionBuffer = 0.01f;
    [SerializeField] private LayerMask _detectableLayers;
    private TouchableSurface _surface;
    private float _nextTimeAllowVO = 0f;
    private float _nextTImeAllowPing = 0f;

    #region Unity Callbacks
    private void OnEnable()
    {
        _surface = GetComponent<TouchableSurface>();
        _surface.OnTouchStart.AddListener(this, OnPlayerTouchBegin);
    }
    private void OnDisable()
    {
        _surface.OnTouchStart.RemoveListener(this, OnPlayerTouchBegin);
    }
    #endregion
    private void OnPlayerTouchBegin(Vector3 touchPosition)
    {
        if (_surface == null) _surface = GetComponent<TouchableSurface>();
        var surfaceCollider = _surface.GetCollider();
        if (surfaceCollider == null) return;

        var bounds = surfaceCollider.bounds;
        var colliders = Physics.OverlapBox(
                        bounds.center,
                        bounds.extents * (1 + _addedDetectionBuffer),
                        _surface.transform.rotation,
                        _detectableLayers);

        /*
        // OLD CODE, DOES THIS NOW MOVE MORE RELIABLY?
        var colliders = Physics.OverlapBox(_surface.transform.position,
                                        surfaceCollider.bounds.extents * (1 + _addedDetectionBuffer),
                                        _surface.transform.rotation,
                                        _detectableLayers
        );
        */
        

        // Find all interactables among colliders found.
        var interactables = new List<Iinteractable>();
        foreach (var item in colliders)
        {
            if (item.TryGetComponent<Iinteractable>(out var interactable))
                interactables.Add(interactable);
        }

        if (interactables.Count == 0) return;

        HandleInteractablePing(interactables);

        if(ShouldCallInteractableDetectedEvent())
        {
            // Currently this is only used by the VO handler.
            EventManager.OnInteractableDetectedOnSurface.Raise(this, -1);
        }

    }
    private bool ShouldCallInteractableDetectedEvent()
    {
        if (_nextTimeAllowVO < Time.time)
        {
            _nextTimeAllowVO = Time.time + PlayerSettings.Developer.TouchDialogueInterval;
            return true;
        }
        return false;
    }
    private void HandleInteractablePing(List<Iinteractable> interactables)
    {
        if (_nextTImeAllowPing < Time.time)
        {
            float defaultDelay = 0.2f;
            foreach (var interactable in interactables)
            {
                interactable.Ping(defaultDelay);

                defaultDelay *= 1.5f;
            }
            _nextTImeAllowPing = Time.time + PlayerSettings.Developer.ItemPingInterval;
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (_surface == null) _surface = GetComponent<TouchableSurface>();
        var surfaceCollider = _surface.GetCollider();
        if (surfaceCollider == null) return;

        var bounds = surfaceCollider.bounds;

        Gizmos.matrix = Matrix4x4.TRS(
            bounds.center,
            _surface.transform.rotation,
            Vector3.one
        );

        // Exact collider bounds
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, bounds.size);


        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            Vector3.zero,
            bounds.size * (1 + _addedDetectionBuffer)
        );
    }

}
