using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Player : MonoBehaviour
{
    public static Player Instance;
    [SerializeField] private XROrigin _xrOrigin;
    [SerializeField] private PlayerHand _rightHand;
    [SerializeField] private PlayerHand _leftHand;
    [SerializeField] private DynamicMoveProvider _moveProvider;
    [SerializeField] private SnapTurnProvider _turnProvider;
    [SerializeField] private PlayerFingerSnapHandler _fingerSnapper;



    public bool PlayerCanMove => _moveProvider.enabled;
    public bool PlayerCanTurn => _turnProvider.enabled;

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance.gameObject);
    }
    #endregion

    #region Getters, public functions, helpers.
    
    public void DisableFingerSnapping()
    {
        if(_fingerSnapper == null) _fingerSnapper = FindFirstObjectByType<PlayerFingerSnapHandler>();
        _fingerSnapper.enabled = false;
    }
    public void EnableFingerSnapping()
    {
        if (_fingerSnapper == null) _fingerSnapper = FindFirstObjectByType<PlayerFingerSnapHandler>();
        _fingerSnapper.enabled = true;
    }
    public void DisableTurnAndMove()
    {
        DisableLocomotion();
        DisableSnapTurn();
    }
    public void EnableTurnAndMove()
    {
        EnableLocomotion();
        EnableSnapTurn();
    }
    public void DisableLocomotion()
    {
        if(_moveProvider == null) _moveProvider = FindFirstObjectByType<DynamicMoveProvider>();
        _moveProvider.enabled = false;
    }
    public void EnableLocomotion()
    {
        if (_moveProvider == null) _moveProvider = FindFirstObjectByType<DynamicMoveProvider>();
        _moveProvider.enabled = true;
    }
    public void DisableSnapTurn()
    {
        if (_turnProvider == null) _turnProvider = FindFirstObjectByType<SnapTurnProvider>();
        _turnProvider.enabled = false;
    }
    public void EnableSnapTurn()
    {
        if (_turnProvider == null) _turnProvider = FindFirstObjectByType<SnapTurnProvider>();
        _turnProvider.enabled = true;
    }
    public void RecenterPlayerWithNoHeightChange(Vector3 worldPos, Vector3 facingDirection)
    {
        if (_xrOrigin == null) _xrOrigin = FindFirstObjectByType<XROrigin>();

        float cameraYHeight = _xrOrigin.Camera.transform.position.y;
        worldPos.y = cameraYHeight;
        _xrOrigin.MoveCameraToWorldLocation(worldPos);
        _xrOrigin.MatchOriginUpCameraForward(Vector3.up, facingDirection);
    }
    public XROrigin GetXROrigin() => _xrOrigin;
    public DynamicMoveProvider GetDynamicMoveProvider() => _moveProvider;
    public SnapTurnProvider GetSnapTurnProvider() => _turnProvider;
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
    #endregion
}
