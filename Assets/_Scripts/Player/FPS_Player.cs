using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-998)]
[RequireComponent(typeof(CharacterController))]
public class FPS_PLAYER : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f; // Units per second

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float cameraFOV = 60f;
    public Transform cameraTransform;

    [Header("Raycast")]
    public float rayDistance = 100f;

    private CharacterController controller;
    private float verticalRotation = 0f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        gameObject.hideFlags = HideFlags.DontSaveInBuild;
    }
#endif


    private void Awake()
    {
#if UNITY_EDITOR
        var xrOrigin = FindFirstObjectByType<XROrigin>();
        
        if (xrOrigin != null)
        {
            var realFootStepHandler = xrOrigin.GetComponentInChildren<PlayerFootStepHandler>();
            var localFootStepHandler = GetComponent<PlayerFootStepHandler>();
            localFootStepHandler.ForceFootStepValues(realFootStepHandler.GetFootStepInterval(), realFootStepHandler.GetFeetSeparationDistance());
            xrOrigin.gameObject.SetActive(false);
        }

#endif
    }
    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

#if UNITY_EDITOR
    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleRaycasts();
    }
#endif
    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            move += transform.forward;

        if (Keyboard.current.sKey.isPressed)
            move -= transform.forward;

        if (Keyboard.current.aKey.isPressed)
            move -= transform.right;

        if (Keyboard.current.dKey.isPressed)
            move += transform.right;

        float moveSpeedThisFrame = moveSpeed;

        if (Keyboard.current.leftShiftKey.isPressed)
        {
            moveSpeedThisFrame *= 3f;
        }

        controller.Move(move.normalized * moveSpeedThisFrame * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity * Time.deltaTime;
        float mouseY = mouseDelta.y * mouseSensitivity * Time.deltaTime;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        cameraTransform.GetComponent<Camera>().fieldOfView = cameraFOV;
    }

#if UNITY_EDITOR
    void HandleRaycasts()
    {
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        RaycastHit hitInfo;

        // Mouse 1 - Single Raycast on Click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Physics.Raycast(ray, out hitInfo, rayDistance))
            {
                hitInfo.collider.TryGetComponent<TouchableSurface>(out var surface);
                surface?.TestFirstTouch(hitInfo.point);

            }
        }

        // Mouse 2 - Single Raycast on Click
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (Physics.Raycast(ray, out hitInfo, rayDistance))
            {
                hitInfo.collider.TryGetComponent<TouchableSurface>(out var surface);
                surface?.TestTouchEnd(hitInfo.point);
            }
        }

        // Mouse 3 - Continuous Raycast while Held
        if (Mouse.current.middleButton.isPressed)
        {
            if (Physics.Raycast(ray, out hitInfo, rayDistance))
            {
                hitInfo.collider.TryGetComponent<TouchableSurface>(out var surface);
                surface?.TestTouchSlide(hitInfo.point);
            }
        }
    }
#endif
}
