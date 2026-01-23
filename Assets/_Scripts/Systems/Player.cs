using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] PlayerHand _rightHand;
    [SerializeField] PlayerHand _leftHand;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance.gameObject);
    }

    public PlayerHand GetRightHand()
    {
        if (_rightHand != null) return _rightHand;
        else
        {
            var hands = FindObjectsByType<PlayerHand>(FindObjectsSortMode.None);
            foreach (var hand in hands)
            {
                if (hand.GetHandXRNode() == UnityEngine.XR.XRNode.RightHand)
                    _rightHand = hand;
                return _rightHand;
            }
        }

        return null;
    }
    public PlayerHand GetLeftHand()
    {
        if (_leftHand != null) return _leftHand;
        else
        {
            var hands = FindObjectsByType<PlayerHand>(FindObjectsSortMode.None);
            foreach (var hand in hands)
            {
                if (hand.GetHandXRNode() == UnityEngine.XR.XRNode.LeftHand)
                    _leftHand = hand;
                return _leftHand;
            }
        }

        return null;
    }
    public bool IsRightHand(PlayerHand hand) => hand == _rightHand;
}
