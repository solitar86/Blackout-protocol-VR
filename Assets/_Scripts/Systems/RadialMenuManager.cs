using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RadialMenuManager : MonoBehaviour
{
    public static RadialMenuManager Instance;
    [SerializeField] private float _minDistanceFromCenterToSelect = 0.15f;
    [SerializeField] private VibrationSettingsSO _selectButtonHapticSettings;
    [SerializeField] private Transform _menuAnchor; // Name this better and make it not a placeholder POS :D
    [Space(15)]

    private PlayerHand _playerMenuHand;
    private RadialMenu _currentMenu;
    private List<RadialMenuItem> _currentMenuItems = new();
    private Stack<RadialMenu> _previousMenus = new();
    private bool _menuIsVisible = false;
    private int _selectedMenuPart = 0;
    private bool _menuSystemDisabled = false;
    public bool MenuIsOpen => _menuIsVisible;

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance.gameObject);
    }
    private void OnEnable()
    {
        EventManager.OnPrimaryButtonPressed.AddListener(this, OnPrimaryButtonPressed);
        EventManager.OnTriggerPressed.AddListener(this, OnTriggerPressed);
        EventManager.OnSecondaryButtonPressed.AddListener(this, OnSecondaryButtonPressed);
        EventManager.OnPlayerStartMove.AddListener(this, OnPlayerStartMove);

        EventManager.OnToggleRadialMenuOnOff.AddListener(this, ToggleMenuSystemOnOff);
    }
    private void OnDisable()
    {
        EventManager.OnPrimaryButtonPressed.RemoveListener(this, OnPrimaryButtonPressed);
        EventManager.OnTriggerPressed.RemoveListener(this, OnTriggerPressed);
        EventManager.OnSecondaryButtonPressed.RemoveListener(this, OnSecondaryButtonPressed);
        EventManager.OnPlayerStartMove.RemoveListener(this, OnPlayerStartMove);

        EventManager.OnToggleRadialMenuOnOff.RemoveListener(this, ToggleMenuSystemOnOff);
    }
    private void Update()
    {
        if (_menuIsVisible == false) return;

        HandleRadialMenuItemSelection();

        //This is debugging code
        /* 
        for (int i = 0; i < _currentMenu.Count; i++)
        {
            Vector3 direction = _testObject.up;
            float rotationAmount = (360f / _currentMenu.Count) * i;
            Vector3 rotationAxis = _testObject.forward;
            Quaternion rotation = Quaternion.AngleAxis(rotationAmount, rotationAxis);
            Vector3 rotatedVector = rotation * direction;
            Debug.DrawLine(_testObject.position, _testObject.position + rotatedVector * 0.5f, Color.red, 1f);
        }
        */
    }
    private void OnDrawGizmos()
    {
        if (_menuIsVisible == false) return;
        Gizmos.DrawSphere(_menuAnchor.transform.position, _minDistanceFromCenterToSelect);
    }
    #endregion

    #region Input Responses
    private void OnPrimaryButtonPressed(bool isRightHand)
    {
        if (_menuSystemDisabled == true) return;
        // Activate and position menu
        _menuIsVisible = !_menuIsVisible;
        if (!_menuIsVisible)
        {
            CloseRadialMenu();
        }
        else
        {
            OpenRadialMenu(isRightHand);
        }
    }
    private void OnTriggerPressed(bool isRightHand)
    {
        // Activate menu item.
        if (_menuIsVisible == false) return;
        _currentMenuItems[_selectedMenuPart].Activate();
        EventManager.OnMenuItemActivate.Raise(this, -1);

    }
    private void OnSecondaryButtonPressed(bool isRightHand)
    {
        if (_currentMenu != null && _menuIsVisible == true)
        {
            OpenPreviousMenuOrCloseMenu(true);
        }
    }
    private void OnPlayerStartMove(int value)
    {
        if (_menuIsVisible) CloseRadialMenu();
    }
    #endregion
    private void OpenRadialMenu(bool isRightHand)
    {
        if (_menuAnchor == null) _menuAnchor = GetComponentInChildren<TextMeshPro>().transform;
        _menuAnchor.gameObject.SetActive(true);
        _playerMenuHand = isRightHand ? Player.Instance.GetRightHand() : Player.Instance.GetLeftHand();
        _menuAnchor.forward = _playerMenuHand.transform.up;
        _menuAnchor.position = _playerMenuHand.transform.position;
        PopulateCurrentRadialMenu(RadialMenuHolder.Mainmenu);
    }
    public void SetAsCurrentRadialMenu(RadialMenu menu)
    {
        PopulateCurrentRadialMenu(menu);
        if (_menuIsVisible == false) EventManager.OnRadialMenuOpen.Raise(this, -1);
    }
    private void PopulateCurrentRadialMenu(RadialMenu menu, bool wasBackButton = false)
    {
        if (menu == null)
        {
            Debugger.LogError("Can't open null RadialMenu.");
            return;
        }

        // Handle previous menu Stack
        if (_currentMenu != null && wasBackButton == false)
        {
            _previousMenus.Push(_currentMenu);
        }

        _currentMenuItems.Clear();
        _currentMenu = menu;
        _currentMenuItems = new List<RadialMenuItem>(_currentMenu.MenuItems);
        TTSPlayer.PlayTTSSequenceWithPaths(true,
            menu.MenuTitleTTSFilePath,
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Open",
            TTSPlayer.GetTTSNumberFilePath(_currentMenuItems.Count),
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Items");
    }
    public void OpenPreviousMenuOrCloseMenu(bool wasBackButton = false)
    {
        if (_previousMenus.Count > 0)
        {
            PopulateCurrentRadialMenu(_previousMenus.Pop(), true);
            return;
        }
        CloseRadialMenu();
    }
    private void HandleMenuItemSelectionChange(int part)
    {
        _menuAnchor?.GetComponentInChildren<TextMeshPro>().SetText(_currentMenuItems[part].Name);
        _playerMenuHand.HandleTouchEnd(_selectButtonHapticSettings);
        TTSPlayer.PlayTTSWithFilePath(_currentMenuItems[part].TTSFilePath);

    }
    private void CloseRadialMenu()
    {
        _currentMenuItems.Clear();
        _currentMenu = null;
        _previousMenus.Clear();
        _menuAnchor.gameObject.SetActive(false);
        TTSPlayer.PlayTTSSequenceWithPaths(true,
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Mainmenu",
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Closed");

        if (_menuIsVisible) _menuIsVisible = false;
        EventManager.OnRadialMenuClose.Raise(this, -1);
    }
    private void HandleRadialMenuItemSelection()
    {
        float handDistanceFromMenu = Vector3.Distance(_playerMenuHand.transform.position,
                                                        _menuAnchor.transform.position);
        if (handDistanceFromMenu < _minDistanceFromCenterToSelect) return;

        Vector3 fromMenuToHand = _playerMenuHand.transform.position - _menuAnchor.position;
        Vector3 projected = Vector3.ProjectOnPlane(fromMenuToHand, _menuAnchor.forward * -1);
        float angle = Vector3.SignedAngle(_menuAnchor.up, projected, _menuAnchor.forward * -1);
        if (angle < 0) angle += 360f;

        int part = (int)angle * _currentMenuItems.Count / 360;

        if (part != _selectedMenuPart)
        {
            _selectedMenuPart = part;
            EventManager.OnMenuItemSelect.Raise(this, (float)part / _currentMenuItems.Count);
            HandleMenuItemSelectionChange(part);
        }
    }

    #region Helpers
    public string GetTTSPathForSnapTurnAngle(float angle)
    {
        switch (angle)
        {
            case 22.5f:
                return "TTS_Menu_22";
            case 45f:
                return "TTS_Menu_45";
            case 90f:
                return "TTS_Menu_90";
            case 180f:
                return "TTS_Menu_180";
            default:
                return string.Empty;
        }
    }
    
    private void ToggleMenuSystemOnOff(bool enabled)
    {
        _menuSystemDisabled = !enabled;
    }
    #endregion
}



/// <summary>
/// A single menu item with delegate and TTS filepath to play when hovered.
/// </summary>
public class RadialMenuItem
{
    /// <summary>
    /// Constructor for a single Radial menu which can go in a menu holder.
    /// </summary>
    /// <param name="name">Name of item</param>
    /// <param name="menuAction">Action to execute on click</param>
    /// <param name="ttsPath">Soundfile to play on select</param>
    public RadialMenuItem(string name, Action menuAction, string ttsPath)
    {
        _name = name;
        _ttsFilePath = ttsPath; // This file will play on select.
        OnActivateAction = menuAction; // This action will execute when pressed.
    }
    private string _name;
    private string _ttsFilePath;
    private Action OnActivateAction;

    public string TTSFilePath => _ttsFilePath;
    public string Name => _name;

    public void Activate()
    {
        OnActivateAction?.Invoke();
    }
}

/// <summary>
/// Holds a list of context menu items and relevant data.
/// </summary>
public class RadialMenu
{
    public string Name;
    public RadialMenuItem[] MenuItems;
    public string MenuTitleTTSFilePath;

    public RadialMenu(string name, string menuTitleTTSFilePath, RadialMenuItem[] menuItems)
    {
        Name = name;
        MenuTitleTTSFilePath = menuTitleTTSFilePath;
        MenuItems = menuItems;
    }
}

/// <summary>
/// Holds all programmed menu "panels" for the game.
/// </summary>
public static class RadialMenuHolder
{
    public const string MENUTTSFILEFOLDERPATH = "TTS/Menu/";

    public static RadialMenuItem BackButton = new RadialMenuItem("Previous menu",
        () => RadialMenuManager.Instance.OpenPreviousMenuOrCloseMenu(true),
        MENUTTSFILEFOLDERPATH + "TTS_Menu_Back");

    public static RadialMenuItem QuitButton = new RadialMenuItem("Quit Game",
    () =>
    {
        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + "TTS_Menu_Goodbye", true);
        var delayObject = new GameObject("path");
        var mono = delayObject.AddComponent<Delay>();

        mono.CallWithDelay(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }, 2f);

    }, MENUTTSFILEFOLDERPATH + "TTS_Menu_Quit");

    #region Main Menu

    /////////////////////////
    // MAIN MENU
    /////////////////////////
    public static RadialMenu Mainmenu = new RadialMenu(
        "Mainmenu", MENUTTSFILEFOLDERPATH + "TTS_Menu_MainMenu",
        new RadialMenuItem[]
        {
           new RadialMenuItem(
                "Sound settings",
                () => { RadialMenuManager.Instance.SetAsCurrentRadialMenu(SoundSettingsMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_SoundSettings"
            ),
            new RadialMenuItem(
                "Accessibility settings",
                () => { RadialMenuManager.Instance.SetAsCurrentRadialMenu(AccessibilityMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_AccessibilityOptions"
            ),
            BackButton,
            new RadialMenuItem(
                "Snap turn settings",
                () => { RadialMenuManager.Instance.SetAsCurrentRadialMenu(SnapTurnMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_SnapTurnAngle"
            ),
           QuitButton
        }
    );

    #endregion

    #region Accessibility menu
    /////////////////////////
    // ACCESSIBILITY MENU
    /////////////////////////
    public static RadialMenu AccessibilityMenu = new RadialMenu(
        "Accessibility Menu", MENUTTSFILEFOLDERPATH + "TTS_Menu_AccessibilityOptions",
        new RadialMenuItem[]
        {
           new RadialMenuItem(
                "Visible Hands",
                () => {
                        bool enabled = PlayerSettings.Accessibility.ToggleHands();
                        string ttsPath = enabled ? "TTS_Menu_Enabled" : "TTS_Menu_Disabled";
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + ttsPath, true);
                        },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_VisibleHands"
            ),
            new RadialMenuItem(
                "Movement Particles",
                () => {
                        bool enabled = PlayerSettings.Accessibility.ToggleParticles();
                        string ttsPath = enabled ? "TTS_Menu_Enabled" : "TTS_Menu_Disabled";
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + ttsPath, true);
                        },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_MovementParticles"
            ),
            BackButton,
            new RadialMenuItem(
                "Touche Ripple Visual",
                () => {
                        bool enabled = PlayerSettings.Accessibility.ToggleTouchRipple();
                        string ttsPath = enabled ? "TTS_Menu_Enabled" : "TTS_Menu_Disabled";
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + ttsPath, true);
                        },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_TouchRipple"
            )

        }
    );
    #endregion

    #region Snap turn menu
    /////////////////////////
    // Snap Turn Menu
    /////////////////////////
    public static RadialMenu SnapTurnMenu = new RadialMenu(
        "Snap turn settings", MENUTTSFILEFOLDERPATH + "TTS_Menu_SnapTurnAngle",
        new RadialMenuItem[]
        {
           new RadialMenuItem(
                "Increase Angle",
                () => {
                        var success = PlayerSettings.Movement.TryIncreaseSnapTurnAngle();
                        if( success == false)
                        {
                            TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + "TTS_Fail");
                            return;
                        }
                         // Setting was changed, Tell player current setting.
                        string angleTTSPath = RadialMenuManager.Instance. GetTTSPathForSnapTurnAngle(PlayerSettings.Movement.SnapTurnAngle);
                        if(angleTTSPath == string.Empty)
                        {
                            Debugger.LogWarning("Tried to play Snapturn angle TTS file but failed");
                            Debugger.PlayBlipSound();
                        }
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + angleTTSPath);

                       },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_IncreaseAngle"
            ),
           BackButton,
         new RadialMenuItem(
                "Decrease Angle",
                () => {
                        var success = PlayerSettings.Movement.TryDecreaseSnapTurnAngle();
                        if( success == false)
                        {
                            TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + "TTS_Fail");
                            return;
                        }
                         // Setting was changed, Tell player current setting.
                        string angleTTSPath = RadialMenuManager.Instance. GetTTSPathForSnapTurnAngle(PlayerSettings.Movement.SnapTurnAngle);
                        if(angleTTSPath == string.Empty)
                        {
                            Debugger.LogWarning("Tried to play Snapturn angle TTS file but failed");
                            Debugger.PlayBlipSound();
                            return;
                        }
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + angleTTSPath);

                       },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_DecreaseAngle"
            )

        }
        );
    #endregion

    #region Sound Settings menu
    /////////////////////////
    // SOUND SETTINGS MENU
    /////////////////////////
    public static RadialMenu SoundSettingsMenu = new RadialMenu(
        "Sound settings", MENUTTSFILEFOLDERPATH + "TTS_Menu_SoundSettings",
        new RadialMenuItem[]
        {
            new RadialMenuItem(
                "Open TTS Speed settings",
                () => { RadialMenuManager.Instance.SetAsCurrentRadialMenu(TTSSpeedMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Speed"
            ),
            BackButton,
            new RadialMenuItem(
                "Open TTS Volume settings",
                () => { RadialMenuManager.Instance.SetAsCurrentRadialMenu(TTSVolumeMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Volume"
            ),

        }
    );
    /////////////////////////
    // TTS SPEED SETTING MENU
    /////////////////////////
    public static RadialMenu TTSSpeedMenu = new RadialMenu(
        "TTS Speed menu", MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Speed",
        new RadialMenuItem[]
        {
            new RadialMenuItem(
                "Increase TTS Speed",
                () => { PlayerSettings.Audio.IncreaseTTS_Speed();
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Sample"); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_IncreaseSpeed"
            ),
            BackButton,
            new RadialMenuItem(
                "Decrease TTS Speed",
                () => { PlayerSettings.Audio.DecreaseTTS_Speed();
                TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Sample"); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_DecreaseSpeed"
            )
        }
    );
    #endregion

    #region TTS Volume Settings
    /////////////////////////
    // TTS VOLUME SETTING MENU
    /////////////////////////
    public static RadialMenu TTSVolumeMenu = new RadialMenu(
        "TTS Volume menu", MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Volume",
        new RadialMenuItem[]
        {
            new RadialMenuItem(
                "Increase TTS Volume",
                () => { PlayerSettings.Audio.IncreaseTTS_Volume();
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Sample"); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_IncreaseVolume"
            ),
            BackButton,
            new RadialMenuItem(
                "Decrease TTS Volume",
                () => { PlayerSettings.Audio.LowerTTS_Volume();
                TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Sample"); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_DecreaseVolume"
            )
        }
    );
    #endregion
}

