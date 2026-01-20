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


    private void OnTriggerEnter(Collider other)
    {
        //Here we handle initial contact. Usually a higher intensity vibration.
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (ThisHandHasDataInList(playerHand) == true) return; // This should not happen.
            playerHandsDataList.Add(new HandCollidingData(playerHand, other, playerHand.transform.position));
            playerHand.PlayHapticFeedback(_firstTouchHapticSettings);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        for (int i = 0; i < playerHandsDataList.Count; i++)
        {
            // Is player hand fully inside the collider?
            if (IsInsideCollider(playerHandsDataList[i].collider, GetComponent<Collider>()))
            {
                Debugger.Log(playerHandsDataList[i].collider.name + " is inside " + transform.name, Debugger.TextColor.LightRed);
                AudioPlayer.PlayErrorSound(this);
            }

            if (playerHandsDataList[i].collider != other) continue;

            //If this hand, that is still touching the collider has moved more
            //than the haptic settings threshold, play vibration (and sound at some point)
            float distance = Vector3.Distance(other.transform.position, playerHandsDataList[i].previousPosition);
            if (distance > _touchSlideHapticSettings._distanceInterval)
            {
                playerHandsDataList[i].hand.PlayHapticFeedback(_touchSlideHapticSettings);

                //To update the structs "previous position" variable we need to fully re-assign it.
                playerHandsDataList[i] = new HandCollidingData(playerHandsDataList[i].hand,
                                                                playerHandsDataList[i].collider,
                                                                other.transform.position);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Here we handle when touching ends.
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (ThisHandHasDataInList(playerHand) == true)
            {
                playerHand.PlayHapticFeedback(_touchEndHapticSettings);
                RemoveThisHandsDataFromList(playerHand);
            }
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
        public HandCollidingData(PlayerHand hand, Collider collider, Vector3 position)
        {
            this.hand = hand;
            this.collider = collider;
            this.previousPosition = position;
        }

        public PlayerHand hand;
        public Collider collider;
        public Vector3 previousPosition;
    }

    #endregion
}
