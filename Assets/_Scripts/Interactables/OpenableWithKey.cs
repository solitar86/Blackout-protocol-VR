using UnityEngine;
using UnityEngine.Events;

public class OpenableWithKey : MonoBehaviour
{
    [SerializeField] private UnityEvent _OnOpenedWithKey;
    public void OpenWithKey(Key key)
    {
        _OnOpenedWithKey?.Invoke();
    }

    [ContextMenu("Open cupboard")]
    public void OpenCupboard()
    {
        OpenWithKey(null);
    }
}
