using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RadialMenuManager : MonoBehaviour
{
    public static RadialMenuManager Instance;
    [SerializeField] private float _minDistanceFromCenterToSelect = 0.5f;
    [SerializeField] private VibrationSettingsSO _selectButtonHapticSettings;
    [SerializeField] private Transform _testObject; // Name this better and make it not a placeholder POS :D
    [Space(15)]

    private PlayerHand _playerMenuHand;
    private RadialMenu _currentMenu;
    private List<RadialMenuItem> _currentMenuItems = new();
    private Stack<RadialMenu> _previousMenus = new();
    private bool _menuIsVisible = false;
    private int _selectedMenuPart = 0;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance.gameObject);
    }

    private void Start()
    {
        EventManager.OnPrimaryButtonPressed.AddListener(this, OnPrimaryButtonPressed);
        EventManager.OnTriggerPressed.AddListener(this, OnTriggerPressed);
        EventManager.OnSecondaryButtonPressed.AddListener(this, OnSecondaryButtonPressed);
        EventManager.OnPlayerStartMove.AddListener(this, OnPlayerStartMove);
    }

    private void OnDisable()
    {
        EventManager.OnPrimaryButtonPressed.RemoveListener(this, OnPrimaryButtonPressed);
        EventManager.OnTriggerPressed.RemoveListener(this, OnTriggerPressed);
        EventManager.OnSecondaryButtonPressed.RemoveListener(this, OnSecondaryButtonPressed);
        EventManager.OnPlayerStartMove.RemoveListener(this, OnPlayerStartMove);
    }

    #region Input Responses
    private void OnPrimaryButtonPressed(bool isRightHand)
    {
        // Activate and position menu
        _menuIsVisible = !_menuIsVisible;
        if (!_menuIsVisible)
        {
            CloseRadialMenu();
        }
        else
        {
            _testObject.gameObject.SetActive(true);
            _playerMenuHand = isRightHand ? Player.Instance.GetRightHand() : Player.Instance.GetLeftHand();
            _testObject.forward = Camera.main.transform.forward;
            _testObject.position = _playerMenuHand.transform.position;
            PopulateCurrentContexMenu(RadialMenuHolder.Mainmenu);

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

    public void OpenContextMenu(RadialMenu menu)
    {
        PopulateCurrentContexMenu(menu);
        if (_menuIsVisible == false) EventManager.OnRadialMenuOpen.Raise(this, -1);
    }
    private void PopulateCurrentContexMenu(RadialMenu menu, bool wasBackButton = false)
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
            PopulateCurrentContexMenu(_previousMenus.Pop(), true);
            return;
        }
        CloseRadialMenu();
    }
    private void HandleMenuItemSelectionChange(int part)
    {
        _testObject?.GetComponentInChildren<TextMeshPro>().SetText(_currentMenuItems[part].Name);
        _playerMenuHand.HandleTouchEnd(_selectButtonHapticSettings);
        TTSPlayer.PlayTTSWithFilePath(_currentMenuItems[part].TTSFilePath);

    }
    private void CloseRadialMenu()
    {
        _currentMenuItems.Clear();
        _currentMenu = null;
        _previousMenus.Clear();
        _testObject.gameObject.SetActive(false);
        TTSPlayer.PlayTTSSequenceWithPaths(true,
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Mainmenu",
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Closed");

        if (_menuIsVisible) _menuIsVisible = false;
        EventManager.OnRadialMenuClose.Raise(this, -1);
    }
    private void HandleRadialMenuItemSelection()
    {
        float handDistanceFromMenu = Vector3.Distance(_playerMenuHand.transform.position,
                                                        _testObject.transform.position);
        if (handDistanceFromMenu < _minDistanceFromCenterToSelect) return;

        Vector3 fromMenuToHand = _playerMenuHand.transform.position - _testObject.position;
        Vector3 projected = Vector3.ProjectOnPlane(fromMenuToHand, _testObject.forward * -1);
        float angle = Vector3.SignedAngle(_testObject.up, projected, _testObject.forward * -1);
        if (angle < 0) angle += 360f;

        int part = (int)angle * _currentMenuItems.Count / 360;

        if (part != _selectedMenuPart)
        {
            _selectedMenuPart = part;
            EventManager.OnMenuItemSelect.Raise(this, (float)part / _currentMenuItems.Count);
            HandleMenuItemSelectionChange(part);
        }
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
        Gizmos.DrawSphere(_testObject.transform.position, _minDistanceFromCenterToSelect);
    }
}



/// <summary>
/// A single menu item with delegate and TTS filepath to play when hovered.
/// </summary>
public class RadialMenuItem
{
    public RadialMenuItem(string name, Action menuAction, string ttsPath)
    {
        _name = name;
        _ttsFilePath = ttsPath;
        OnActivateAction = menuAction;
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

    public static RadialMenuItem QuitButton = new RadialMenuItem("Previous menu",
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
                "Open Sound settings",
                () => { RadialMenuManager.Instance.OpenContextMenu(SoundSettingsMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_SoundSettings"
            ),
           BackButton,
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
                    // TEST IF THIS WORKS, IF IT DOES REMOVE THIS COMMENT.
                        bool enabled = PlayerSettings.Accessibility.ToggleHands();
                        string ttsPath = enabled ? "TTS_Menu_Enabled" : "TTS_Menu_Disabled";
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + ttsPath, true);
                        },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_VisibleHands"
            ),
            new RadialMenuItem(
                "Movement Particles",
                () => {
                    // TEST IF THIS WORKS, IF IT DOES REMOVE THIS COMMENT.
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
                    // TEST IF THIS WORKS, IF IT DOES REMOVE THIS COMMENT.
                        bool enabled = PlayerSettings.Accessibility.ToggleTouchRipple();
                        string ttsPath = enabled ? "TTS_Menu_Enabled" : "TTS_Menu_Disabled";
                        TTSPlayer.PlayTTSWithFilePath(MENUTTSFILEFOLDERPATH + ttsPath, true);
                        },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_TouchRipple"
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
                () => { RadialMenuManager.Instance.OpenContextMenu(TTSSpeedMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Speed"
            ),
            BackButton,
            new RadialMenuItem(
                "Open TTS Volume settings",
                () => { RadialMenuManager.Instance.OpenContextMenu(TTSVolumeMenu); },
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

