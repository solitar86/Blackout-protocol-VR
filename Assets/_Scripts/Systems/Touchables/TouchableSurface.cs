using System.Collections.Generic;
using UnityEngine;

public class TouchableSurface : MonoBehaviour
{
    [SerializeField] private VibrationSettingsSO _firstTouchHapticSettings;
    [SerializeField] private VibrationSettingsSO _touchSlideHapticSettings;
    [SerializeField] private VibrationSettingsSO _touchEndHapticSettings;
    private List<PlayerHand> _playerHands = new();

    private Vector3 _previousPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (_playerHands.Contains(playerHand)) return;
            _playerHands.Add(playerHand);
            playerHand.FirstTouch(_firstTouchHapticSettings);
            _previousPosition = playerHand.transform.position;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        float distance = Vector3.Distance(other.transform.position, _previousPosition);
        if (distance > _touchSlideHapticSettings._distanceInterval)
        {
            if (other.TryGetComponent<PlayerHand>(out var playerHand))
            {
                playerHand.FirstTouch(_touchSlideHapticSettings);
                _previousPosition = playerHand.transform.position;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (_playerHands.Contains(playerHand))
            {
                _playerHands.Remove(playerHand);
            }

        }
        /*
        if (other.TryGetComponent<PlayerHand>(out var playerHand))
        {
            if (_playerHands.Contains(playerHand)) return;
            _playerHands.Add(playerHand);
            playerHand.FirstTouch(_firstTouchHapticSettings);
        }
        */
    }
}
