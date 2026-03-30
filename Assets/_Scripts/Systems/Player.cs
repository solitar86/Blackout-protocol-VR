using System;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class Player : MonoBehaviour
{
    public static Player Instance;
    [SerializeField] private XROrigin _xrOrigin;
    [SerializeField] private Transform _playerHead;
    [SerializeField] private PlayerHand _rightHand;
    [SerializeField] private PlayerHand _leftHand;
    [SerializeField] private DynamicMoveProvider _moveProvider;
    [SerializeField] private SnapTurnProvider _turnProvider;
    [SerializeField] private PlayerFingerSnapHandler _fingerSnapper;
    [SerializeField] private PlayerNorthBeaconHandler _northBeacon;

    private Transform _startTransform = null;

    public bool PlayerCanMove => _moveProvider.enabled;
    public bool PlayerCanTurn => _turnProvider.enabled;

    #region Unity Callbacks

    private void OnEnable()
    {
        EventManager.OnStickPressed.AddListener(this, RecenterPlayer);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance.gameObject);

    }

    private void OnDisable()
    {
        EventManager.OnStickPressed.RemoveListener(this, RecenterPlayer);
    }
    #endregion

    #region Getters, public functions, helpers.

    public void RecenterPlayerToStartPositionWithNoTTS()
    {
        if (_xrOrigin == null) _xrOrigin = FindFirstObjectByType<XROrigin>();
        if ((_startTransform == null))
        {
            var marker = FindFirstObjectByType<PlayerStartMarker>();
            if (marker != null)
            {
                _startTransform = marker.transform;
            }
            else
            {
                Debugger.LogWarning("Recenter failed due to null _startTransform");
                return;
            }
        }

        var worldPos = _startTransform.position;
        var facingDirection = _startTransform.forward;
        float cameraYHeight = _xrOrigin.Camera.transform.position.y;
        worldPos.y = cameraYHeight;
        _xrOrigin.MoveCameraToWorldLocation(worldPos);
        _xrOrigin.MatchOriginUpCameraForward(Vector3.up, facingDirection);
    }
    private void RecenterPlayer(bool isRightHand)
    {
        if (_xrOrigin == null) _xrOrigin = FindFirstObjectByType<XROrigin>();
        if ((_startTransform == null))
        {
            var marker = FindFirstObjectByType<PlayerStartMarker>();
            if(marker != null)
            {
                _startTransform = marker.transform;
            }
            else
            {
                TTSPlayer.PlayTTSWithFilePath("TTS/TTS_RecenterFailed");
                return;
            }
        }

        var worldPos = _startTransform.position;
        var facingDirection = _startTransform.forward;
        float cameraYHeight = _xrOrigin.Camera.transform.position.y;
        worldPos.y = cameraYHeight;
        _xrOrigin.MoveCameraToWorldLocation(worldPos);
        _xrOrigin.MatchOriginUpCameraForward(Vector3.up, facingDirection);

        TTSPlayer.PlayTTSWithFilePath("TTS/TTS_Recentered");
        EventManager.OnPlayerStartMove.Raise(this, -1);
    }
    public void DisableFingerSnapping()
    {
        if(_fingerSnapper == null) _fingerSnapper = FindFirstObjectByType<PlayerFingerSnapHandler>();
        _fingerSnapper.Disable();
    }
    public void EnableFingerSnapping()
    {
        if (_fingerSnapper == null) _fingerSnapper = FindFirstObjectByType<PlayerFingerSnapHandler>();
        _fingerSnapper.Enable();
    }
    public void DisableNorthBeacon()
    {
        if (_northBeacon == null) _northBeacon = FindFirstObjectByType<PlayerNorthBeaconHandler>();
        _northBeacon.Disable();
    }
    public void EnableNorthBeacon()
    {
        if (_northBeacon == null) _northBeacon = FindFirstObjectByType<PlayerNorthBeaconHandler>();
        _northBeacon.Enable();
    }
    public void DisableTurnAndMove()
    {
        DisableMovement();
        DisableSnapTurn();
    }
    public void EnableTurnAndMove()
    {
        EnableMovement();
        EnableSnapTurn();
    }
    public void DisableMovement()
    {
        if(_moveProvider == null) _moveProvider = FindFirstObjectByType<DynamicMoveProvider>();
        _moveProvider.enabled = false;
    }
    public void EnableMovement()
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
    public void DisableRadialMenu()
    {
        EventManager.OnToggleRadialMenuOnOff.Raise(this, false);
    }
    public void EnableRadialMenu()
    {
        EventManager.OnToggleRadialMenuOnOff.Raise(this, true);
    }
    public void RecenterPlayerWithNoHeightChange(Vector3 worldPos, Vector3 facingDirection)
    {
        if (_xrOrigin == null) _xrOrigin = FindFirstObjectByType<XROrigin>();

        float cameraYHeight = _xrOrigin.Camera.transform.position.y;
        worldPos.y = cameraYHeight;
        _xrOrigin.MoveCameraToWorldLocation(worldPos);
        _xrOrigin.MatchOriginUpCameraForward(Vector3.up, facingDirection);
        EventManager.OnPlayerStartMove.Raise(this, -1);
    }
    public XROrigin GetXROrigin() => _xrOrigin;
    public DynamicMoveProvider GetDynamicMoveProvider() => _moveProvider;
    public SnapTurnProvider GetSnapTurnProvider() => _turnProvider;
    public Transform GetPlayerHeadTransform() => _playerHead;
    public Vector3 GetPlayerLookingDirection() => _playerHead.forward;
    public Vector3 GetPlayerHeadTransformRight() => _playerHead.right;
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
    public Vector3 GetPlayerEarPosition(bool rightEar)
    {
        return _playerHead.position + _playerHead.right * (rightEar ? 0.2f : -0.2f);
    }
    public Vector3 GetPosInFrontOfPlayerFace()
    {
        return _playerHead.position + _playerHead.transform.forward * 0.25f;
    }
    public bool IsRightHand(PlayerHand hand) => hand == _rightHand;

    #endregion
}
