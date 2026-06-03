using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameIdleResetTimer : MonoBehaviour
{

    [SerializeField] private float _timeUntilReset = 10f;
    private float _resetTimer;

    private void OnEnable()
    {
        EventManager.OnPlayerPushJoystick.AddListener(this, ResetTimer);
        EventManager.OnPrimaryButtonPressed.AddListener(this, ResetTimer);
        EventManager.OnSecondaryButtonPressed.AddListener(this, ResetTimer);
        EventManager.OnTriggerPressed.AddListener(this, ResetTimer);
        EventManager.OnGripPressed.AddListener(this, ResetTimer);

        EventManager.OnPlayerTouchPickUp.AddListener(this, ResetTimer);
        EventManager.OnPlayerTouchStaticInteractable.AddListener(this, ResetTimer);

        EventManager.OnRadialMenuOpen.AddListener(this, ResetTimer);
        EventManager.OnRadialMenuClose.AddListener(this, ResetTimer);
        EventManager.OnMenuItemSelect.AddListener(this, ResetTimer);

        EventManager.OnInteractableDetectedOnSurface.AddListener(this, ResetTimer);
        EventManager.OnSurfaceIsEmpthy.AddListener(this, ResetTimer);   
    }



    private void OnDisable()
    {
        EventManager.OnPlayerPushJoystick.RemoveListener(this, ResetTimer);
        EventManager.OnPrimaryButtonPressed.RemoveListener(this, ResetTimer);
        EventManager.OnSecondaryButtonPressed.RemoveListener(this, ResetTimer);
        EventManager.OnTriggerPressed.RemoveListener(this, ResetTimer);
        EventManager.OnGripPressed.RemoveListener(this, ResetTimer);

        EventManager.OnPlayerTouchPickUp.RemoveListener(this, ResetTimer);
        EventManager.OnPlayerTouchStaticInteractable.RemoveListener(this, ResetTimer);

        EventManager.OnRadialMenuOpen.RemoveListener(this, ResetTimer);
        EventManager.OnRadialMenuClose.RemoveListener(this, ResetTimer);
        EventManager.OnMenuItemSelect.RemoveListener(this, ResetTimer);

        EventManager.OnInteractableDetectedOnSurface.RemoveListener(this, ResetTimer);
        EventManager.OnSurfaceIsEmpthy.RemoveListener(this, ResetTimer);
    }



    void Update()
    {
        if(PlayerInputHandler.PlayerIsMoving == true || SceneManager.GetActiveScene().buildIndex == 0)
        {
            ResetTimer();
        }
        else
        {
            _resetTimer += Time.deltaTime;

        }

        Debugger.Log(_resetTimer.ToString("F2"), Debugger.TextColor.LightBlue);

        if (_resetTimer > _timeUntilReset)
        {
            Debugger.Log("Reset Timer reached Reset time", Debugger.TextColor.LightBlue);
            LoadBootupScene();
        }
    }

    private void ResetTimer(float obj)
    {
        ResetTimer();
    }

    private void ResetTimer(int obj)
    {
        ResetTimer();
    }
    private void ResetTimer(StaticInteractable nada)
    {
        ResetTimer();
    }
    private void ResetTimer(PickUpObject nada)
    {
        ResetTimer();
    }
    private void ResetTimer(bool obj)
    {
        ResetTimer();
    }
    private void ResetTimer()
    {
        _resetTimer = 0f;
    }
    private void LoadBootupScene()
    {
        Debugger.Log("Loading bootup scene", Debugger.TextColor.LightBlue);
        Debugger.Log(_resetTimer, Debugger.TextColor.LightBlue);
        ResetTimer();
        PlayerPrefs.DeleteAll();
        PlayerSettings.SetAllDefaults();
        Debugger.Log("Player Prefs have been cleared", Debugger.TextColor.LightGreen);
        SceneManager.LoadScene(0);
    }
}
