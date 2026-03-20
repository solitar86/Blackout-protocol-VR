using System;
using UnityEngine;

public class UIAudioHandler : MonoBehaviour
{

    [SerializeField] Sound _openMenuSound;
    [SerializeField] Sound _closeMenuSound;
    [SerializeField] Sound _selectButtonSound;
    [SerializeField] Sound _activateButtonSound;
    [SerializeField] Sound _backButtonSound;

    #region Unity Callbacks
    private void OnEnable()
    {
        EventManager.OnRadialMenuOpen.AddListener(this, PlayMenuOpenSound);
        EventManager.OnRadialMenuClose.AddListener(this, PlayMenuCloseSound);
        EventManager.OnMenuItemSelect.AddListener(this, PlayMenuItemSelectedSound);
        EventManager.OnMenuItemActivate.AddListener(this, PlayMenuItemActivatedSound);
        EventManager.OnPreviousMenuOpened.AddListener(this, PlayBackButtonSound);
        EventManager.OnMenuBlocked.AddListener(this, HandleMenuBlockedSound);
    }
    private void OnDisable()
    {
        EventManager.OnRadialMenuOpen.RemoveListener(this, PlayMenuOpenSound);
        EventManager.OnRadialMenuClose.RemoveListener(this, PlayMenuCloseSound);
        EventManager.OnMenuItemSelect.RemoveListener(this, PlayMenuItemSelectedSound);
        EventManager.OnMenuItemActivate.RemoveListener(this, PlayMenuItemActivatedSound);
        EventManager.OnPreviousMenuOpened.RemoveListener(this, PlayBackButtonSound);
        EventManager.OnMenuBlocked.RemoveListener(this, HandleMenuBlockedSound);
    }


    #endregion

    private void PlayMenuOpenSound(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _openMenuSound, Vector3.zero, false, false);
    }
    private void PlayMenuCloseSound(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _closeMenuSound, Vector3.zero, false, false);
    }
    private void PlayMenuItemSelectedSound(float value)
    {
        _selectButtonSound.Pitch = 1 + value; // TEST THIS
        AudioPlayer.PlaySoundAtPoint(this, _selectButtonSound, Vector3.zero, false, false);
    }
    private void PlayMenuItemActivatedSound(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _activateButtonSound, Vector3.zero, false, false);
    }

    private void PlayBackButtonSound(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _backButtonSound, Vector3.zero, false, false);
    }

    private void HandleMenuBlockedSound(int value)
    {
        float delay = 0.1f;

        if(value == 0)
        {
            // Menu is blocked by being disabled.
            for (int i = 0; i < 2; i++)
            {
                this.CallWithDelay(() =>
                {
                    AudioPlayer.PlaySoundAtPoint(this, _backButtonSound, Vector3.zero, false, false);

                },delay *= 2); 
            }
        }

    }


}
