using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestStateListener : MonoBehaviour
{
    [Tooltip("These quest must be in these states or higher for OnRequirementsReached to trigger")]
    [SerializeField] QuestProgressionStep[] _requirements;
    [SerializeField] UnityEvent OnRequirementsMet;
    private bool hasBeenCompleted = false;
    private List<QuestSO> questsToCheck = new();

    #region UnityCallbacks

    private void OnEnable()
    {
        EventManager.OnAnyQuestWasProgressed.AddListener(this, CheckRequirements);
    }
    private void OnDisable()
    {
        EventManager.OnAnyQuestWasProgressed.RemoveListener(this, CheckRequirements);
    }

    private void Start()
    {
        foreach (var questStep in _requirements)
        {
            questsToCheck.Add(questStep.Quest);
        }
    }

    #endregion

    private void CheckRequirements(int value)
    {
        if (hasBeenCompleted == true) return;

        foreach (var quest in questsToCheck)
        {
            var requirement = GetMatchingProgressionStepWithQuest(quest);
            if(quest.State < requirement.ProgressionState)
            {
                //We have not reached this required step
                return;
            }
        }

        // We have met all requirements.
        hasBeenCompleted = true;
        Debugger.Log("Quest requirements were met for " + gameObject.name, Debugger.TextColor.LightGreen);
        OnRequirementsMet.Invoke();

    }

    private QuestProgressionStep GetMatchingProgressionStepWithQuest(QuestSO questToCheck)
    {
        foreach (var item in _requirements)
        {
            if (item.Quest == questToCheck)
                return item;
        }
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Call Complete Quests Events")]
    public void CallOnQuestCompletedEvents()
    {
        OnRequirementsMet?.Invoke();
    }
    
#endif
}
