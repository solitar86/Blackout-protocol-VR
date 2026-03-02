using System;
using System.Collections;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private FadeInTitle _title;
    [SerializeField] private GameObject _interactionTutorialItems;

    private bool _skipTutorial = false;

    private void OnEnable()
    {
        
    }
    private void OnDisable()
    {
        EventManager.OnPlayerWantSkip.RemoveListener(this, SkipTutorial);
    }
    private const string TTSTUTORIALPATH = "TTS/Tutorial/";
    private IEnumerator Start()
    {
        _title?.Hide();
        // Disable menu so player doesn't open it until prompted
        EventManager.OnToggleRadialMenuOnOff.Raise(this, false);
        Player.Instance.DisableTurnAndMove();
        Player.Instance.DisableFingerSnapping();

        yield return new WaitForSeconds(5f);
        _title?.FadeIn();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Welcome");
        yield return new WaitForSeconds(2f);

        EventManager.OnPlayerWantSkip.AddListener(this, SkipTutorial);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_ToSkipTutorial", out float clipDuration);
        yield return new WaitForSeconds(clipDuration + 3f);

        if(_skipTutorial == false)
        {
            StartTutorial();
        }

    }
    private void SkipTutorial(int value)
    {
        _skipTutorial = true;
        GetComponent<TutorialHandler>().StopTutorial();
        StopAllCoroutines();
        EventManager.OnPlayerWantSkip.RemoveListener(this, SkipTutorial);
        InitMainMenuSetup();
    }
    private void InitMainMenuSetup()
    {
        // Enable menu functionality for player
        EventManager.OnToggleRadialMenuOnOff.Raise(this, true);
        // Remove tutorial skip listener if it is still there.
        EventManager.OnPlayerWantSkip.RemoveListener(this, SkipTutorial);
        // Make sure player movement is enabled
        Player.Instance.EnableTurnAndMove();
        Player.Instance.EnableFingerSnapping();
    }
    private void StartTutorial()
    {
        StopAllCoroutines();
        GetComponent<TutorialHandler>().StartTutorial(InitMainMenuSetup);
    }

    #region Helpers

    #endregion
}
