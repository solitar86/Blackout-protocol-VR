using UnityEngine;
using UnityEngine.Events;

public class StaticInteractableActivationDetector : MonoBehaviour
{
    public UnityEvent OnThisActivated;
    public UnityEvent OnThisDeactivated;
       
    private void OnEnable()
    {
        EventManager.OnAnyInteractableActivated.AddListener(this, OnActivateInteractable);   
    }
    private void OnDisable()
    {
        EventManager.OnAnyInteractableActivated.RemoveListener(this, OnActivateInteractable);
    }

    private void OnActivateInteractable(StaticInteractable interactable)
    {
        if (interactable.Equals(GetComponent<StaticInteractable>()))
        {
            if (interactable.IsActivated == true)
            {
                OnThisActivated?.Invoke();
                Debugger.Log($"{interactable} was <color=#00FF00>activated</color>");
            }
            else
            {
                OnThisDeactivated?.Invoke();
                Debugger.Log($"{interactable} was <color=#FF0000>deactivated</color>");
            }
        }
    }
}
