using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RadialMenuHandler : MonoBehaviour
{
    public static RadialMenuHandler Instance;
    [SerializeField] private float _minDistanceFromCenterToSelect = 0.5f;
    [SerializeField] private VibrationSettingsSO _selectButtonHapticSettings;
    [SerializeField] private Transform _testObject;
    [Space(15)]
    [SerializeField] Sound _openMenuSound;
    [SerializeField] Sound _closeMenuSound;
    [SerializeField] Sound _selectButtonSound;
    [SerializeField] Sound _activateButtonSound;


    private bool _menuIsVisible = false;
    private PlayerHand _playerMenuHand;
    private int _selectedMenuPart = 0;
    private List<RadialMenuItem> _currentMenu = new();
    private Stack<RadialMenu> _previousMenus = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance.gameObject);
    }

    private void Start()
    {
        EventManager.OnPrimaryButtonPressed.AddListener(this, OnPrimaryButtonPressed);
        EventManager.OnTriggerPressed.AddListener(this, OnTriggerPressed);
    }

    private void OnDisable()
    {
        EventManager.OnPrimaryButtonPressed.RemoveListener(this, OnPrimaryButtonPressed);
        EventManager.OnTriggerPressed.RemoveListener(this, OnTriggerPressed);
    }

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
            CreateContextMenu(RadialMenuHolder.TTSSpeedMenu);

        }
    }

    private void OnTriggerPressed(bool isRightHand)
    {
        // Activate menu item.
        if (_menuIsVisible == false) return;
        _currentMenu[_selectedMenuPart].Activate();
        EventManager.OnMenuItemActivate.Raise(this, -1);

    }

    private void CreateContextMenu(RadialMenu menu)
    {
        if(menu == null) 
        _currentMenu.Clear();
        _currentMenu = new List<RadialMenuItem>(menu.MenuItems);
        TTSPlayer.PlayTTSSequenceWithPaths(true,
            menu.MenuTitleTTSFilePath,
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Open",
            TTSPlayer.GetTTSNumberFilePath(_currentMenu.Count),
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Items");
    }

    public void OpenContextMenu(RadialMenu menu)
    {
        CreateContextMenu(menu);
        if(_menuIsVisible == false) EventManager.OnRadialMenuOpen.Raise(this, -1);
    }

    public void PreviousMenu()
    {
        if(_previousMenus.Count > 0)
        {
            CreateContextMenu(_previousMenus.Pop());
            return;
        }
        CloseRadialMenu();
    }
    
    private void HandleMenuItemSelectionChange(int part)
    {
        //TTSPlayer.PlayNumber(part);
        _testObject.GetComponentInChildren<TextMeshPro>().SetText(part.ToString());
        _playerMenuHand.HandleTouchEnd(_selectButtonHapticSettings);
        TTSPlayer.PlayTTSWithFilePath(_currentMenu[part].TTSFilePath);

    }

    private void CloseRadialMenu()
    {
        _testObject.gameObject.SetActive(false);
        TTSPlayer.PlayTTSSequenceWithPaths(true,
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Mainmenu",
            RadialMenuHolder.MENUTTSFILEFOLDERPATH + "TTS_Menu_Closed");

        if (_menuIsVisible) _menuIsVisible = false;
        EventManager.OnRadialMenuClose.Raise(this, -1);
    }
    private void Update()
    {
        if (_menuIsVisible == false) return;

        HandleRadialMenuItemSelection();

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

    private void HandleRadialMenuItemSelection()
    {
        float handDistanceFromMenu = Vector3.Distance(_playerMenuHand.transform.position,
                                                        _testObject.transform.position);

        if (handDistanceFromMenu < _minDistanceFromCenterToSelect) return;

        Debugger.Log("Can select");
        
        Vector3 fromMenuToHand = _playerMenuHand.transform.position - _testObject.position;
        Vector3 projected = Vector3.ProjectOnPlane(fromMenuToHand, _testObject.forward * -1);
        float angle = Vector3.SignedAngle(_testObject.up, projected, _testObject.forward * -1);
        if (angle < 0) angle += 360f;

        int part = (int)angle * _currentMenu.Count / 360;

        if (part != _selectedMenuPart)
        {
            _selectedMenuPart = part;
            EventManager.OnMenuItemSelect.Raise(this, (float)part/_currentMenu.Count);
            HandleMenuItemSelectionChange(part);
        }
    }

    private void OnDrawGizmos()
    {
        if(_menuIsVisible == false) return;
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
        () => RadialMenuHandler.Instance.PreviousMenu(),
        MENUTTSFILEFOLDERPATH + "TTS_Menu_Back");

    // MAIN MENU
    public static RadialMenu Mainmenu = new RadialMenu(
        "Mainmenu", MENUTTSFILEFOLDERPATH + "TTS_Menu_MainMenu",
        new RadialMenuItem[]
        {
           new RadialMenuItem(
                "Open Sound settings",
                () => { RadialMenuHandler.Instance.OpenContextMenu(SoundSettingsMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_SoundSettings"
            ),
        }
    );

    // SOUND SETTINGS MENU
    public static RadialMenu SoundSettingsMenu = new RadialMenu(
    "Sound settings", MENUTTSFILEFOLDERPATH + "TTS_Menu_SoundSettings",
    new RadialMenuItem[]
    {
            new RadialMenuItem(
                "Open TTS Speed settings",
                () => { RadialMenuHandler.Instance.OpenContextMenu(TTSSpeedMenu); },
                MENUTTSFILEFOLDERPATH + "TTS_Menu_IncreaseSpeed"
            ),
            //new RadialMenuItem(
            //    "Open TTS Volume settings",
            //    () => { RadialMenuHandler.Instance.OpenContextMenu(TTSVolumeMenu); },
            //    MENUTTSFILEFOLDERPATH + "TTS_Menu_TTS_Volume"
            //),

    }
);

    // TTS SPEED SETTING MENU
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
}

