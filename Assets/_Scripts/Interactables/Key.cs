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


    [SerializeField] KeyQuestProgressionStep[] _questProgressionWhenTouchedWithKey;


    private float _nextTimeAllowKeyDoesntFitVO = 0;
    private float _KeyDoesntFitVOMinInterval = 0f;

    #region Unity Callbacks
    private void Awake()
    {
        base.Awake();
        _KeyDoesntFitVOMinInterval = _keyDoesntFitVO.Clip.length + 1f;
    }
    #endregion

    public override void HandleCollisiondWithSpecificObjects(GameObject environmentObject)
    {

        foreach (var questStep in _questProgressionWhenTouchedWithKey)
        {
            if(questStep.requiredCollisionGameObject == environmentObject)
            {
                questStep.ProgressThisQuest();
            }
        }

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
                HandleInteractionWithOpenableObject(environmentObject);
            }
        }
    }
    private void HandleInteractionWithOpenableObject(GameObject environmentObject)
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

[System.Serializable]
public class KeyQuestProgressionStep
{
    [SerializeField] public GameObject requiredCollisionGameObject;
    [SerializeField] private QuestProgressionStep questProgression;

    public void ProgressThisQuest()
    {
        EventManager.OnProgressQuest.Raise(this, questProgression);
    }
}
