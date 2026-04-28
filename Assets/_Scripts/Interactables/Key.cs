using System;
using UnityEngine;

public class Key : PickUpObject
{
    [Space(15)]
    [Header("Key Specific Settings")]
    [SerializeField] Sound _keyDoesntFitVO;

    [Tooltip("These objects will trigger the 'Key Doesn't fit' voiceline")]
    [SerializeField] GameObject[] _invalidObjects;

    [Tooltip("These objects will be interacted with")]
    [SerializeField] GameObject[] _validObjects;


    private float _nextTimeAllowKeyDoesntFitVO = 0;
    private float _KeyDoesntFitVOMinInterval = 0f;

    #region Unity Callbacks
    private void Awake()
    {
        _KeyDoesntFitVOMinInterval = _keyDoesntFitVO.Clip.length + 1f;
    }

    #endregion

    public override void HandleCollisiondWithSpecificObjects(GameObject environmentObject)
    {
        foreach (var item in _invalidObjects)
        {
            if (item.Equals(environmentObject))
            {
                //This item is specifically marked as an invalid object
                if (_nextTimeAllowKeyDoesntFitVO < Time.time)
                {
                    PlayKeyDoesntFitVoiceline();
                    _nextTimeAllowKeyDoesntFitVO = Time.time + 1f;
                    return;
                }
            }
        }

        foreach (var item in _validObjects)
        {
            if(item.Equals(environmentObject))
            {
                // This key works on this object
                HandleKeyInteractionWithValidObject(environmentObject);
            }
        }
    }

    private void HandleKeyInteractionWithValidObject(GameObject environmentObject)
    {
        environmentObject.SetActive(false);
        ForceRemoveObjectFromHandAndReturnToStartPosition(HoldingHand);
        if(environmentObject.TryGetComponent<OpenableWithKey>(out var openable))
        {
            openable.OpenWithKey(this);
        }

    }

    private void PlayKeyDoesntFitVoiceline()
    {
        AudioPlayer.PlaySoundAtPoint(this, _keyDoesntFitVO, Player.Instance.transform.position, usePitchVariation: false, spatialize: false);
    }
}
