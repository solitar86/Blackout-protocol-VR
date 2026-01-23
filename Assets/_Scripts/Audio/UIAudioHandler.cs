using UnityEngine;

public class UIAudioHandler : MonoBehaviour
{

    [SerializeField] Sound _openMenuSound;
    [SerializeField] Sound _closeMenuSound;
    [SerializeField] Sound _selectButtonSound;
    [SerializeField] Sound _activateButtonSound;
    private void Start()
    {
        EventManager.OnRadialMenuOpen.AddListener(this, PlayMenuOpenSound);
        EventManager.OnRadialMenuClose.AddListener(this, PlayMenuCloseSound);
        EventManager.OnMenuItemSelect.AddListener(this, PlayMenuItemSelectedSound);
        EventManager.OnMenuItemActivate.AddListener(this, PlayMenuItemActivatedSound);
    }

    private void PlayMenuOpenSound(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _openMenuSound, Vector3.zero);
    }
    private void PlayMenuCloseSound(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _closeMenuSound, Vector3.zero);
    }
    private void PlayMenuItemSelectedSound(float value)
    {
        _selectButtonSound.Pitch = 1 + value; // TEST THIS
        AudioPlayer.PlaySoundAtPoint(this, _selectButtonSound, Vector3.zero);
    }
    private void PlayMenuItemActivatedSound(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _activateButtonSound, Vector3.zero);
    }

    private void OnDisable()
    {

    }

}
