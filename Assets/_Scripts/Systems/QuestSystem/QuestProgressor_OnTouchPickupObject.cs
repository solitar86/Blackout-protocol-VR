using UnityEngine;
public class QuestProgressor_OnTouchPickupObject : MonoBehaviour
{
    [Tooltip("When this object is touched it will set this queststate to the defined state")]
    [SerializeField] private QuestProgressionStep _questProgressionStep;

    private void OnEnable()
    {
        EventManager.OnPlayerTouchPickUp.AddListener(this, OnTouched);
    }
    private void OnDisable()
    {
        EventManager.OnPlayerTouchPickUp.RemoveListener(this, OnTouched);
    }

    private void OnTouched(PickUpObject touchedItem)
    {
        if (touchedItem.Equals(GetComponent<PickUpObject>()))
        {
            EventManager.OnProgressQuest.Raise(this, _questProgressionStep);
        }
    }
}


