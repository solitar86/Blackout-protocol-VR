using System;
using System.Collections;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private GameObject _interactionTutorialItems;
    [SerializeField] private Sound _VOIntroDialogue;

    private bool _skipTutorial = false;
    private bool _hasDroppedWalkieTalkie = false;
    private bool _hasActivatedFaucet = false;
    private const string TTSTUTORIALPATH = "TTS/Tutorial/";

    private Action _onTutorialCompleteAction;

    private void Start()
    {
        _interactionTutorialItems.SetActive(false);
    }
    public void StartTutorial(Action onTutorialCompleteAction)
    {
        _onTutorialCompleteAction = onTutorialCompleteAction;
        StopAllCoroutines();
        StartCoroutine(TutorialCoroutine());
    }
    public void StopTutorial()
    {
        _skipTutorial = true;
        _interactionTutorialItems.SetActive(true);
        TTSPlayer.ForceStopAllTTS();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_SkippingTutorial");
        StopAllCoroutines();
    }
    private IEnumerator TutorialCoroutine()
    {
        yield return StartCoroutine(TTSIntroduction());

        yield return StartCoroutine(RadialMenuTutorial());

        yield return StartCoroutine(CharacterVoiceIntroduction());

        yield return StartCoroutine(MovementTutorial());

        yield return StartCoroutine(InteractionTutorial());

        yield return new WaitForSeconds(1.5f);
        HandleTutorialEnd();
    }
    private IEnumerator TTSIntroduction()
    {
        float clipDuration = 0f;
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_StartingTutorial", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 2f);

        // Introduce TTS voice,
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_TTSIntroduction", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);
    }
    private IEnumerator RadialMenuTutorial()
    {
        float clipDuration = 0f;
        // How to use Radial menu info.
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_MenuInfo", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration);
        EventManager.OnToggleRadialMenuOnOff.Raise(this, true);

        // Enable Player Menu here and subscribe listener to OnCloseEvent to continue.
        bool _menuWasClosed = false;
        Action<int> onMenuClosed = (_) => _menuWasClosed = true;
        EventManager.OnRadialMenuClose.AddListener(this, onMenuClosed);

        // If player has not opened menu yet, remind them. 
        yield return new WaitForSeconds(5f);
        if (_menuWasClosed == false && RadialMenuManager.Instance.MenuIsOpen == false)
            TTSPlayer.PlayOnLoopWithFilePath(TTSTUTORIALPATH + "TTS_MenuInfo_Reminder");

        // Player has opened and closed menu atleast once. Continue.
        yield return new WaitUntil(() => _menuWasClosed == true);
        EventManager.OnRadialMenuClose.RemoveListener(this, onMenuClosed);
        EventManager.OnToggleRadialMenuOnOff.Raise(this, false);

        yield return new WaitForSeconds(1.5f);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);
    }
    private IEnumerator CharacterVoiceIntroduction()
    {
        //Introduce character voice.
        yield return new WaitForSeconds(2);
        AudioPlayer.PlaySoundAtPoint(this, _VOIntroDialogue, transform.position, false, false);
        yield return new WaitForSeconds(_VOIntroDialogue.Clip.length + 2f);
    }
    private IEnumerator InteractionTutorial()
    {
        float clipDuration = 0f;
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Interaction_part1", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        _interactionTutorialItems.SetActive(true);

        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Interaction_part2", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Wait until player has
        // Activated both objects and
        // dropped walkie talkie on floor
        yield return new WaitUntil(() => _hasActivatedFaucet && _hasDroppedWalkieTalkie);

        // Continue with tutorial
        yield return new WaitForSeconds(1.5f); // A short delay.
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);
    }
    private IEnumerator MovementTutorial()
    {
        float clipDuration = 0f;
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Movement_part1", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);


        Player.Instance.EnableTurnAndMove();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Movement_part2", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Short pause before continue
        yield return new WaitForSeconds(1.5f);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);
    }
    private void HandleTutorialEnd()
    {
        _onTutorialCompleteAction?.Invoke();
    }
    #region Helpers
    public void SetHasDroppedWalkieTalkie(bool value)
    {
        _hasDroppedWalkieTalkie = value;
    }
    public void SetHasActivatedFaucet(bool value)
    {
        _hasActivatedFaucet = value;
    }
    #endregion
}
