using UnityEngine;

public class UIAudioHandler : MonoBehaviour
{

    [SerializeField] Sound _openMenuSound;
    [SerializeField] Sound _closeMenuSound;
    [SerializeField] Sound _selectButtonSound;
    [SerializeField] Sound _activateButtonSound;

    #region Unity Callbacks
    private void OnEnable()
    {
        EventManager.OnRadialMenuOpen.AddListener(this, PlayMenuOpenSound);
        EventManager.OnRadialMenuClose.AddListener(this, PlayMenuCloseSound);
        EventManager.OnMenuItemSelect.AddListener(this, PlayMenuItemSelectedSound);
        EventManager.OnMenuItemActivate.AddListener(this, PlayMenuItemActivatedSound);
    }
    private void OnDisable()
    {
        EventManager.OnRadialMenuOpen.RemoveListener(this, PlayMenuOpenSound);
        EventManager.OnRadialMenuClose.RemoveListener(this, PlayMenuCloseSound);
        EventManager.OnMenuItemSelect.RemoveListener(this, PlayMenuItemSelectedSound);
        EventManager.OnMenuItemActivate.RemoveListener(this, PlayMenuItemActivatedSound);
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


}
