using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        SceneManager.sceneLoaded += OnSceneLoaded_HandleSceneLoad;
    }

    private void OnDisable()
    {
        EventManager.OnPrimaryButtonPressed.RemoveListener(this, OnPrimaryButtonPressed);
        EventManager.OnTriggerPressed.RemoveListener(this, OnTriggerPressed);
        EventManager.OnSecondaryButtonPressed.RemoveListener(this, OnSecondaryButtonPressed);
        EventManager.OnPlayerStartMove.RemoveListener(this, OnPlayerStartMove);

        EventManager.OnToggleRadialMenuOnOff.RemoveListener(this, ToggleMenuSystemOnOff);

        SceneManager.sceneLoaded += OnSceneLoaded_HandleSceneLoad;
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
        if (IsRadialMenuBlocked() == true) return;
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

    #region Core Functions
    private void OpenRadialMenu(bool isRightHand)
    {
        if (_menuAnchor == null) _menuAnchor = GetComponentInChildren<TextMeshPro>().transform;
        _menuAnchor.gameObject.SetActive(true);
        _playerMenuHand = isRightHand ? Player.Instance.GetRightHand() : Player.Instance.GetLeftHand();
        _menuAnchor.forward = _playerMenuHand.transform.up;
        _menuAnchor.position = _playerMenuHand.transform.position;

        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            EmptyAndRePopulateCurrentRadialMenu(RadialMenuHolder.TutorialSceneMainMenu);
        }
        else
        {
            EmptyAndRePopulateCurrentRadialMenu(RadialMenuHolder.Mainmenu);
        }
        EventManager.OnRadialMenuOpen.Raise(this, -1);
    }
    public void SetAsCurrentRadialMenu(RadialMenu menu)
    {
        EmptyAndRePopulateCurrentRadialMenu(menu);
        if (_menuIsVisible == false) EventManager.OnRadialMenuOpen.Raise(this, -1);
    }
    private void EmptyAndRePopulateCurrentRadialMenu(RadialMenu menu, bool wasBackButton = false)
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
            EmptyAndRePopulateCurrentRadialMenu(_previousMenus.Pop(), true);
            return;
        }
        CloseRadialMenu();
    }
    private void HandleMenuItemSelectionChange(int part)
    {
        // TODO: MAKE THIS A PERMANENT REFERENCE (if we need it )
        _menuAnchor?.GetComponentInChildren<TextMeshPro>().SetText(_currentMenuItems[part].Name);
        _playerMenuHand.HandleTouchEnd(_selectButtonHapticSettings);

        if(_currentMenuItems[part].InfoTTSFilePath == null)
        {
            //This item only has a name, no additional info.
            TTSPlayer.PlayTTSWithFilePath(_currentMenuItems[part].NameTTSFilePath);
            return;
        }

        // This menu item has additional information coming after the name is spoken.
        // Play those in sequence while allowing for interrupt.
        TTSPlayer.PlayTTSSequenceWithPaths(false,
                _currentMenuItems[part].NameTTSFilePath,
                _currentMenuItems[part].InfoTTSFilePath);


    }
    private void CloseRadialMenu(bool wasSceneLoad = false)
    {
        _currentMenuItems.Clear();
        _currentMenu = null;
        _previousMenus.Clear();
        _menuAnchor?.gameObject.SetActive(false); //BUG NOTE: This caused a null reference for some reason??
        _playerMenuHand = null;

        if(wasSceneLoad ==  false)
        {
            TTSPlayer.PlayTTSSequenceWithPaths(true,
                RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Mainmenu",
                RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Closed");
        }

        if (_menuIsVisible) _menuIsVisible = false;
        EventManager.OnRadialMenuClose.Raise(this, -1);

        PlayerSettings.SaveSettings();
    }
    private void HandleRadialMenuItemSelection()
    {
        if(_playerMenuHand == null)
        {
            // Something has gone wrong for us to get here.
            Debugger.LogWarning("Player hand transform is missing, closing radial menu.", Debugger.TextColor.LightRed);
            CloseRadialMenu(); return;
        }
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
    #endregion

    #region Helpers & Maintenance
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
    private bool IsRadialMenuBlocked()
    {
        if (_menuSystemDisabled == true)
        {
            Debugger.Log("Radial menu blocked by being disabled");
            return true;
        }

        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            Debugger.Log("Radial menu blocked by us being in bootup scene");
            return true;
        }

        return false;
        
    }
    private void OnSceneLoaded_HandleSceneLoad(Scene arg0, LoadSceneMode arg1)
    {
        bool wasSceneLoad = true;
        CloseRadialMenu(wasSceneLoad);
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
    /// <param name="nameTTSPath">Soundfile to play on select</param>
    /// <param name="extraInfoTTSPath">Soundfile for optional explanation of item</param>
    public RadialMenuItem(string name, Action menuAction, string nameTTSPath, string extraInfoTTSPath = null)
    {
        _name = name;
        _nameTTSFilePath = nameTTSPath; // This file will play on select.
        OnActivateAction = menuAction; // This action will execute when pressed.
        _infoTTSFilePath = extraInfoTTSPath;
    }
    private string _name;
    private string _nameTTSFilePath;
    public string _infoTTSFilePath;
    private Action OnActivateAction;

    public string NameTTSFilePath => _nameTTSFilePath;
    public string InfoTTSFilePath => _infoTTSFilePath;
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

    public static RadialMenuItem StartGameButton = new RadialMenuItem("Start Game",
    () =>
    {
        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + "TTS_Menu_StartingGame");
        var delayObject = new GameObject("Start Game With Delay Object");
        var mono = delayObject.AddComponent<Delay>();

        mono.CallWithDelay(() =>
        {
            SceneManager.LoadScene(2);
        }, 3f);
    },
    MENUTTSFILEFOLDERPATH + "TTS_Menu_StartGame");

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
                MENUTTSFILEFOLDERPATH + "TTS_Menu_VisibleHands",
                MENUTTSFILEFOLDERPATH + "TTS_Menu_Visiblehands_description"
            ),
            new RadialMenuItem(
                "Movement Particles",
                () => {
                        bool enabled = PlayerSettings.Accessibility.ToggleParticles();
                        string ttsPath = enabled ? "TTS_Menu_Enabled" : "TTS_Menu_Disabled";
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + ttsPath, true);
                        },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_MovementParticles",
                MENUTTSFILEFOLDERPATH + "TTS_Menu_MovementParticles_description"
            ),
            BackButton,
            new RadialMenuItem(
                "Touche Ripple Visual",
                () => {
                        bool enabled = PlayerSettings.Accessibility.ToggleTouchRipple();
                        string ttsPath = enabled ? "TTS_Menu_Enabled" : "TTS_Menu_Disabled";
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + ttsPath, true);
                        },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_TouchRipple",
                MENUTTSFILEFOLDERPATH + "TTS_Menu_TouchRipple_description"
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

    #region Tutorial Specific MainMenu

    /////////////////////////
    // Tutorial Scene Main Menu
    /////////////////////////
    public static RadialMenu TutorialSceneMainMenu = new RadialMenu(
    "Mainmenu", MENUTTSFILEFOLDERPATH + "TTS_Menu_MainMenu",
    new RadialMenuItem[]
    {
           StartGameButton,
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
}

