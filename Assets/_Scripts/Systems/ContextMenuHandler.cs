using TMPro;
using UnityEngine;

public class ContextMenuHandler : MonoBehaviour
{
    [SerializeField] Transform _testObject;

    private Plane _menuPlane;
    private bool _menuIsVisible = false;
    private Transform _playerHand;

    private int numButtons = 7;
    private int _lastPart = 0;

    private void Start()
    {
        EventManager.OnPrimaryButtonPressed.AddListener(this, PrimaryButtonPressed);
    }
    private void OnDisable()
    {
        EventManager.OnPrimaryButtonPressed.RemoveListener(this, PrimaryButtonPressed);
    }

    private void PrimaryButtonPressed(bool isRightHand)
    {
        _menuIsVisible = !_menuIsVisible;
        if (!_menuIsVisible)
        {
            _testObject.gameObject.SetActive(false);
        }
        else
        {
            _testObject.gameObject.SetActive(true);
            _playerHand = FindAnyObjectByType<PlayerHand>().transform;
            _testObject.forward = Camera.main.transform.forward;
            _testObject.position = _playerHand.position;
        }

        //_menuPlane = new Plane();
        //_menuPlane.SetNormalAndPosition(Camera.main.transform.forward, position);
    }

    private void Update()
    {
        if (_menuIsVisible == false) return;

        Vector3 fromMenuToHand = _playerHand.position - _testObject.position;
        Vector3 projected = Vector3.ProjectOnPlane(fromMenuToHand, _testObject.forward * -1);
        Debug.DrawLine(_testObject.position, _testObject.position + projected);

        float angle = Vector3.SignedAngle(_testObject.up, projected, _testObject.forward * -1);
        if (angle < 0) angle += 360f;

        int part = (int)angle * numButtons / 360;
        _testObject.GetComponentInChildren<TextMeshPro>().SetText(part.ToString());

        if(part != _lastPart)
        {
            TTSPlayer.PlayNumber(part);
            _lastPart = part;
        }

        for (int i = 0; i < numButtons; i++)
        {
            Vector3 direction = _testObject.up;
            float rotationAmount = (360f / numButtons) * i;
            Vector3 rotationAxis = _testObject.forward;
            Quaternion rotation = Quaternion.AngleAxis(rotationAmount, rotationAxis);
            Vector3 rotatedVector = rotation * direction;
            Debug.DrawLine(_testObject.position, _testObject.position + rotatedVector * 0.5f, Color.red, 1f);
        }
    }

}
