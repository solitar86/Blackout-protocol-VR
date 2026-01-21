using System;
using System.Collections.Generic;
using UnityEngine;

public class TouchableSurface : MonoBehaviour
{
    [SerializeField] private VibrationSettingsSO _firstTouchHapticSettings;
    [SerializeField] private VibrationSettingsSO _touchSlideHapticSettings;
    [SerializeField] private VibrationSettingsSO _touchEndHapticSettings;

    // This keeps track if sliding feedback should be played.
    private List<HandCollidingData> playerHandsDataList = new();

    public GameEvent<Vector3> OnTouchStart = new("First touch");
    public GameEvent<(float distance, Vector3 position)> OnTouchSlide = new("Touch slide");
    public GameEvent<Vector3> OnTouchEnd = new("Touch end");


    private void OnTriggerEnter(Collider other)
    {
        //Here we handle initial contact. Usually a higher intensity vibration.
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (ThisHandHasDataInList(playerHand) == true) return; // This should not happen.
            playerHandsDataList.Add(new HandCollidingData(playerHand, other, playerHand.transform.position));
            playerHand.HandleTouchBegin(_firstTouchHapticSettings, other.transform.position);
            OnTouchStart.Raise(this, other.transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        for (int i = 0; i < playerHandsDataList.Count; i++)
        {
            if (playerHandsDataList[i].collider != other) continue;

            HandlePlayerHandFullyInsideCollider(i);
            HandlePlaySlidingHaptic(other, i);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        //Here we handle when touching ends.
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (ThisHandHasDataInList(playerHand) == true)
            {
                playerHand.HandleTouchEnd(_touchEndHapticSettings);
                RemoveThisHandsDataFromList(playerHand);
                OnTouchEnd.Raise(this, other.transform.position);
            }
        }
    }

    private void HandlePlaySlidingHaptic(Collider playerHandCollider, int i)
    {
        float distance = Vector3.Distance(playerHandCollider.transform.position, playerHandsDataList[i].previousPosition);

        if (distance > _touchSlideHapticSettings.DistanceInterval)
        {
            OnTouchSlide.Raise(this, (distance, playerHandCollider.transform.position));
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
                                            playerHandsDataList[index].handInsideColliderTimer + Time.deltaTime); // Only the timer updates.

            AudioPlayer.PlayErrorSound(this);

            // Handle "Error" Haptic Pulse.
            if (playerHandsDataList[index].handInsideColliderTimer > 0.6f)
            {
                var settings = Resources.Load<VibrationSettingsSO>("Haptics/HandInsideColliderVibrationSettings");
                playerHandsDataList[index].hand.HandleHandInsideCollider(settings);
                playerHandsDataList[index] = new HandCollidingData(playerHandsDataList[index].hand,
                                                                playerHandsDataList[index].collider,
                                                                playerHandsDataList[index].previousPosition,
                                                                0f); // Only the timer updates.
            }
        }
        else
        {
            AudioPlayer.PauseErrorSound();
        }
    }

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

    #endregion
}
