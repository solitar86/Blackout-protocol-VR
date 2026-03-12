using UnityEngine;

public class QuestProgressor_OnTouchStaticInteractable : MonoBehaviour
{
    [Tooltip("When this object is touched it will set this queststate to the defined state")]
    [SerializeField] private QuestProgressionStep _questProgressionStep;

    private void OnEnable()
    {
        EventManager.OnPlayerTouchStaticInteractable.AddListener(this, OnTouched);
    }
    private void OnDisable()
    {
        EventManager.OnPlayerTouchStaticInteractable.RemoveListener(this, OnTouched);
    }

    private void OnTouched(StaticInteractable touchedItem)
    {
        if (touchedItem.Equals(GetComponent<StaticInteractable>()))
        {
            EventManager.OnProgressQuest.Raise(this, _questProgressionStep);
        }
    }
}
