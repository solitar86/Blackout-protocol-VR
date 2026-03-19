using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TouchableSurface : MonoBehaviour
{
    [SerializeField] private VibrationSettingsSO _firstTouchHapticSettings;
    [SerializeField] private VibrationSettingsSO _touchSlideHapticSettings;
    [SerializeField] private VibrationSettingsSO _touchEndHapticSettings;
    [Space(15)]
    [SerializeField] private UnityEvent _onSurfaceTouched;

    // This keeps track of hand while collision is happening eg. slide or stay.
    private List<HandCollidingData> playerHandsDataList = new();

    public GameEvent<Vector3> OnHandTouchStart = new("First touch");
    public GameEvent<(float distance, Vector3 position)> OnHandTouchSlide = new("Touch slide");
    public GameEvent<Vector3> OnHandTouchEnd = new("Touch end");
    public GameEvent<Iinteractable> OnTouchedWithObject = new("Interactable touch surface");

    private Collider _collider;

    #region Unity Callbacks -> Trigger Enter/Exit/Stay Callbacks
    private void OnTriggerEnter(Collider other)
    {
        if (this.enabled == false) return;
        //Here we handle initial contact. Usually a higher intensity vibration.
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (ThisHandHasDataInList(playerHand) == true) return; // This should not happen.
            HandPlayerInitialTouch(other, playerHand);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (this.enabled == false) return;
        for (int i = 0; i < playerHandsDataList.Count; i++)
        {
            if (playerHandsDataList[i].collider != other) continue;

            HandlePlayerHandFullyInsideCollider(i);
            HandlePlaySlidingHaptic(other, i);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (this.enabled == false) return;
        //Here we handle when touching ends.
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (ThisHandHasDataInList(playerHand) == true)
            {
                playerHand.HandleTouchEnd(_touchEndHapticSettings);
                RemoveThisHandsDataFromList(playerHand);
                OnHandTouchEnd.Raise(this, other.transform.position);
            }
        }
    }

    #endregion
    private void HandPlayerInitialTouch(Collider other, PlayerHand playerHand)
    {
        playerHandsDataList.Add(new HandCollidingData(playerHand, other, playerHand.transform.position));
        playerHand.HandleTouchBegin(_firstTouchHapticSettings, other.transform.position);
        OnHandTouchStart.Raise(this, other.transform.position);
        _onSurfaceTouched?.Invoke();
    }
    private void HandlePlaySlidingHaptic(Collider playerHandCollider, int i)
    {
        float distance = Vector3.Distance(playerHandCollider.transform.position, playerHandsDataList[i].previousPosition);

        if (distance > _touchSlideHapticSettings.DistanceInterval)
        {
            OnHandTouchSlide.Raise(this, (distance, playerHandCollider.transform.position));
            // playerHandsDataList[i].hand.PlayHapticFeedback(_touchSlideHapticSettings);
            playerHandsDataList[i].hand.HandleTouchSlide(_touchSlideHapticSettings);


            //To update the structs "previous position" variable we need to fully re-assign it.
            playerHandsDataList[i] = new HandCollidingData(playerHandsDataList[i].hand,                         //Do not update this
                                                            playerHandsDataList[i].collider,                    // Do not update this
                                                            playerHandCollider.transform.position,                           // Update this
                                                            playerHandsDataList[i].handInsideColliderTimer);    // Do not update this
        }
    }
    private void HandlePlayerHandFullyInsideCollider(int index)
    {
        if (IsInsideCollider(playerHandsDataList[index].collider, GetComponent<Collider>()))
        {
            // To update the "handinsidecollider" timer we need to fully reassign the struct.
            playerHandsDataList[index] = new HandCollidingData(playerHandsDataList[index].hand,
                                            playerHandsDataList[index].collider,
                                            playerHandsDataList[index].previousPosition,
                                            playerHandsDataList[index].handInsideColliderTimer - Time.deltaTime); // Only the timer updates.

            AudioPlayer.PlayHandInsideColliderError(this);

            // Handle "Error" Haptic Pulse.
            float errorHapticInterval = 0.6f;
            if (playerHandsDataList[index].handInsideColliderTimer <= errorHapticInterval)
            {
                var settings = Resources.Load<VibrationSettingsSO>("Haptics/HandInsideColliderVibrationSettings");
                playerHandsDataList[index].hand.HandleHandInsideCollider(settings);
                playerHandsDataList[index] = new HandCollidingData(playerHandsDataList[index].hand,
                                                                playerHandsDataList[index].collider,
                                                                playerHandsDataList[index].previousPosition,
                                                                errorHapticInterval); // Only the timer updates.
            }
        }
        else
        {
            AudioPlayer.PauseHandInsideColliderError();
        }
    }
    public void HandleTouchedWithPickUpObject(Iinteractable pickUpObject)
    {
        OnTouchedWithObject.Raise(this, pickUpObject);
    }
    //public void HandlePlayerCantGrabThis(Vector3 position)
    //{
    //    EventManager.OnCantCarryObject.Raise(this, -1);
    //}

    #region Helper functions and HandData Struct
    private void RemoveThisHandsDataFromList(PlayerHand playerHand)
    {
        HandCollidingData dataToremove = new();
        foreach (var handData in playerHandsDataList)
        {
            if (handData.hand == playerHand)
                dataToremove = handData;
        }

        playerHandsDataList.Remove(dataToremove);
    }
    private bool ThisHandHasDataInList(PlayerHand playerHand)
    {
        foreach (var handData in playerHandsDataList)
        {
            if (handData.hand == playerHand) return true;
        }
        return false;
    }
    private bool IsInsideCollider(Collider innerCollider, Collider outerCollider)
    {
        if (outerCollider is BoxCollider)
        {
            Vector3[] testPoints =
            {
                innerCollider.bounds.min,
                innerCollider.bounds.max,
                innerCollider.bounds.center
            };

            foreach (var point in testPoints)
            {
                if (outerCollider.ClosestPoint(point) != point)
                    return false;
            }

            return true;
        }

        ////////////////////////////////////////
        ///THIS PART IS UNDER EXPERIMENTATION
        ////////////////////////////////////////
        if (outerCollider is MeshCollider)
        {
            // Check for actual collisions/contact
            Collider[] hits = Physics.OverlapBox(
                innerCollider.bounds.center,
                innerCollider.bounds.extents,
                innerCollider.transform.rotation,
                ~0, // All layers
                QueryTriggerInteraction.Collide
            );

            foreach (var hit in hits)
            {
                if (hit == outerCollider)
                    return true; // The inner collider is still overlapping the mesh
            }

            return false; // No contact ? false
        }

        return false;
    }
    private struct HandCollidingData
    {
        public HandCollidingData(PlayerHand hand, Collider collider, Vector3 position, float timer = 0)
        {
            this.hand = hand;
            this.collider = collider;
            this.previousPosition = position;
            handInsideColliderTimer = timer;
        }

        public PlayerHand hand;
        public Collider collider;
        public Vector3 previousPosition;
        public float handInsideColliderTimer;
    }
    public Collider GetCollider()
    {
        if (_collider == null)
            _collider = GetComponent<Collider>();

        if (_collider == null) return null;
        return _collider;
    }
    #endregion

    #region FPS Testing Helper Functions
#if UNITY_EDITOR
    public void TestFirstTouch(Vector3 touchPosition)
    {
        OnHandTouchStart.Raise(this, touchPosition);
    }

    public void TestTouchEnd(Vector3 touchPosition)
    {
        OnHandTouchEnd.Raise(this, touchPosition);
    }

    public void TestTouchSlide(Vector3 touchPosition)
    {
        float distance = 1f;
        OnHandTouchSlide.Raise(this, (distance, touchPosition));
    }
#endif
    #endregion
}
