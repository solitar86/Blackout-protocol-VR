using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class RadialMenuHandler : MonoBehaviour
{
    [SerializeField] private Transform _testObject;


    private Plane _menuPlane;
    private bool _menuIsVisible = false;
    private PlayerHand _playerMenuHand;

    private int _selectedMenuPart = 0;

    private List<RadialMenuItem> _currentMenu = new();

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
            _testObject.gameObject.SetActive(false);
        }
        else
        {
            _testObject.gameObject.SetActive(true);
            _playerMenuHand = FindAnyObjectByType<PlayerHand>();
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

    }

    private void CreateContextMenu(RadialMenu menu)
    {
        _currentMenu.Clear();
        _currentMenu = new List<RadialMenuItem>(menu.MenuItems);
        TTSPlayer.PlayTTSWithFilePath(menu.MenuTitlePath, true);
    }

    private void Update()
    {
        if (_menuIsVisible == false) return;

        Vector3 fromMenuToHand = _playerMenuHand.transform.position - _testObject.position;
        Vector3 projected = Vector3.ProjectOnPlane(fromMenuToHand, _testObject.forward * -1);
        float angle = Vector3.SignedAngle(_testObject.up, projected, _testObject.forward * -1);
        if (angle < 0) angle += 360f;

        int part = (int)angle * _currentMenu.Count / 360;

        if (part != _selectedMenuPart)
        {
            _selectedMenuPart = part;
            HandleMenuItemSelectionChange(part);
        }

        for (int i = 0; i < _currentMenu.Count; i++)
        {
            Vector3 direction = _testObject.up;
            float rotationAmount = (360f / _currentMenu.Count) * i;
            Vector3 rotationAxis = _testObject.forward;
            Quaternion rotation = Quaternion.AngleAxis(rotationAmount, rotationAxis);
            Vector3 rotatedVector = rotation * direction;
            Debug.DrawLine(_testObject.position, _testObject.position + rotatedVector * 0.5f, Color.red, 1f);
        }
    }

    private void HandleMenuItemSelectionChange(int part)
    {
        //TTSPlayer.PlayNumber(part);
        _testObject.GetComponentInChildren<TextMeshPro>().SetText(part.ToString());
        TTSPlayer.PlayTTSWithFilePath(_currentMenu[part].TTSFilePath);
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
    public string MenuTitlePath;

    public RadialMenu(string name, string menuTitlePath, RadialMenuItem[] menuItems)
    {
        Name = name;
        MenuTitlePath = menuTitlePath;
        MenuItems = menuItems;
    }
}

/// <summary>
/// Holds all programmed menu "panels" for the game.
/// </summary>
public static class RadialMenuHolder
{
    public const string MENUTTSFOLDER = "TTS/Menu/";

    // TTS SPEED SETTING MENU
    public static RadialMenu TTSSpeedMenu = new RadialMenu(
        "TTS Speed menu", MENUTTSFOLDER + "TTS_Menu_TTS_Speed",
        new RadialMenuItem[]
        {
            new RadialMenuItem(
                "Increase TTS Speed",
                () => { PlayerSettings.Audio.IncreaseTTS_Speed(); },
                MENUTTSFOLDER + "TTS_Menu_IncreaseSpeed"
            ),
            new RadialMenuItem(
                "Decrease TTS Speed",
                () => { PlayerSettings.Audio.DecreaseTTS_Speed(); },
                MENUTTSFOLDER + "TTS_Menu_DecreaseSpeed"
            )
        }
    );
}

