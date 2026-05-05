using System;
using System.Collections;
using Unity.Tutorials.Core.Editor;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private GameObject _interactionTutorialItems;
    [SerializeField] private Sound _VOIntroDialogue;
    [SerializeField] private Sound _errorSound;
    [SerializeField] private Transform _insectPrefab;

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
        StartCoroutine(TutorialMasterCoroutine());
    }
    private IEnumerator TutorialMasterCoroutine()
    {
        Player.Instance.RecenterPlayerToStartPositionWithNoTTS();
        Player.Instance.DisableTurnAndMove();
        Player.Instance.EnableFingerSnapping();
        Player.Instance.DisableNorthBeacon();
        EventManager.OnToggleRadialMenuOnOff.Raise(this, false);

        yield return StartCoroutine(TTSIntroduction());
        
        yield return StartCoroutine(BasicsOfVRTutorial());

        yield return StartCoroutine(RadialMenuTutorial());

        yield return StartCoroutine(CharacterVoiceIntroduction());

        yield return StartCoroutine(MovementTutorial());

        yield return StartCoroutine(HandInsideColliderTutorial());

        yield return StartCoroutine(NorthBeaconTutorial());

        yield return StartCoroutine(FingerSnapTutorial());

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

        // Introduce REPEAT function
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_RepeatTTSTutorialDone");
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_RepeatTTSTutorial", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);

        // Add progression block until player has tried to repeat
        bool hasRepeatedTTS = false;
        Action<int> playerProgressionDelegate = (_) => hasRepeatedTTS = true;
        EventManager.OnRepeatTTSCalled.AddListener(this, playerProgressionDelegate);
        yield return new WaitUntil(() => hasRepeatedTTS == true);
        EventManager.OnRepeatTTSCalled.RemoveListener(this, playerProgressionDelegate);

        //Player succesfully repeated TTS.
        yield return new WaitForSeconds(TTSPlayer.GetDurationOfTTSClipWithPath(TTSTUTORIALPATH + "TTS_RepeatTTSTutorialDone") + 1);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);

    }
    private IEnumerator BasicsOfVRTutorial()
    {
        float clipDuration = 0f;
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_VRBasics_part1", out clipDuration, true);
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_VRBasics_part1");
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Spawn insect and wait till player kills it.
        var insect = Instantiate(_insectPrefab, Player.Instance.GetPlayerEarPosition(true), Quaternion.identity);
        insect.SetParent(Player.Instance.GetPlayerHeadTransform());
        yield return new WaitUntil(()=> insect.gameObject.activeInHierarchy == false);
        Destroy(insect.gameObject);
        yield return new WaitForSeconds(1f);

        // Say well done
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Welldone", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_VRBasics_part2", out clipDuration, true);
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_VRBasics_part2");
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Spawn insect and wait till player kills it.
        insect = Instantiate(_insectPrefab, Player.Instance.GetPlayerEarPosition(false), Quaternion.identity);
        insect.SetParent(Player.Instance.GetPlayerHeadTransform());
        yield return new WaitUntil(() => insect.gameObject.activeInHierarchy == false);
        Destroy(insect.gameObject);
        yield return new WaitForSeconds(1f);

        // Say well done
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Welldone", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_VRBasics_part3", out clipDuration, true);
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_VRBasics_part3");
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Spawn insect and wait till player kills it.
        insect = Instantiate(_insectPrefab, Player.Instance.GetPosInFrontOfPlayerFace(), Quaternion.identity);
        insect.SetParent(Player.Instance.GetPlayerHeadTransform());
        yield return new WaitUntil(() => insect.gameObject.activeInHierarchy == false);
        Destroy(insect.gameObject);
        yield return new WaitForSeconds(1f);

        // Say well done and continue tutorial
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Welldone", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);
        yield return new WaitForSeconds(0.5f);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);

    }
    private IEnumerator RadialMenuTutorial()
    {
        float clipDuration = 0f;
        // How to use Radial menu info.
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_MenuInfo", out clipDuration, true);
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_MenuInfo");
        yield return new WaitForSeconds(clipDuration);
        EventManager.OnToggleRadialMenuOnOff.Raise(this, true);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_MenuInfo2", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration);

        // Enable Player Menu here and subscribe listener to OnPlayerWantSkip to continue.
        bool playerWantProgress = false;
        Action<int> playerWantProgressDelegate = (_) => playerWantProgress = true;
        EventManager.OnPlayerWantSkip.AddListener(this, playerWantProgressDelegate);

        // If player has not opened menu yet, remind them. 
        yield return new WaitForSeconds(5f);
        if (playerWantProgress == false && RadialMenuManager.Instance.MenuIsOpen == false)
            TTSPlayer.PlayOnLoopUntilInterruptWithFilePath(TTSTUTORIALPATH + "TTS_MenuInfo_Reminder");

        // Player wants to progress tutorial.
        yield return new WaitUntil(() => playerWantProgress == true);
        EventManager.OnPlayerWantSkip.RemoveListener(this, playerWantProgressDelegate);

        yield return new WaitForSeconds(0.5f);
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
    private IEnumerator MovementTutorial()
    {

        float clipDuration = 0f;
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Movement_part1", out clipDuration, true);
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_Movement_part1");
        yield return new WaitForSeconds(clipDuration + 0.1f);

        Player.Instance.EnableTurnAndMove();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Movement_part2", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Enable Player Menu here and subscribe listener to OnPlayerWantSkip to continue.
        bool playerWantProgress = false;
        Action<int> playerWantProgressDelegate = (_) => playerWantProgress = true;
        EventManager.OnPlayerWantSkip.AddListener(this, playerWantProgressDelegate);

        // Allow user to try to move before telling about Recenter-input.
        yield return new WaitForSeconds(3f);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Movement_part3", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Player wants to progress tutorial.
        yield return new WaitUntil(() => playerWantProgress == true);
        EventManager.OnPlayerWantSkip.RemoveListener(this, playerWantProgressDelegate);

        yield return new WaitForSeconds(0.5f);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);
    }
    private IEnumerator HandInsideColliderTutorial()
    {
        float clipDuration = 0f;
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_ErrorSound_part1", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration);
        AudioPlayer.PlaySoundAtPoint(this, _errorSound, Vector3.zero, false, false);
        yield return new WaitForSeconds(0.5f);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_ErrorSound_part2", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration);
    }
    private IEnumerator NorthBeaconTutorial()
    {
        float clipDuration = 0f;
        Player.Instance.EnableNorthBeacon();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_NorthBeacon", out clipDuration, true);
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_NorthBeacon");
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Wait for player to hold trigger to progress
        bool playerWantProgress = false;
        Action<int> playerWantProgressDelegate = (_) => playerWantProgress = true;
        EventManager.OnPlayerWantSkip.AddListener(this, playerWantProgressDelegate);

        // Player wants to progress tutorial.
        yield return new WaitUntil(() => playerWantProgress == true);
        EventManager.OnPlayerWantSkip.RemoveListener(this, playerWantProgressDelegate);

        yield return new WaitForSeconds(0.5f);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);
    }
    private IEnumerator FingerSnapTutorial()
    {
        float clipDuration = 0f;
        Player.Instance.EnableFingerSnapping();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_FingerSnap", out clipDuration, true);
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_FingerSnap");
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Wait for player to hold trigger to progress
        bool playerWantProgress = false;
        Action<int> playerWantProgressDelegate = (_) => playerWantProgress = true;
        EventManager.OnPlayerWantSkip.AddListener(this, playerWantProgressDelegate);

        // Player wants to progress tutorial.
        yield return new WaitUntil(() => playerWantProgress == true);
        EventManager.OnPlayerWantSkip.RemoveListener(this, playerWantProgressDelegate);

        yield return new WaitForSeconds(0.5f);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);
    }
    private IEnumerator InteractionTutorial()
    {
        float clipDuration = 0f;
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Interaction_part1", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        _interactionTutorialItems.SetActive(true);

        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Interaction_part2", out clipDuration, true);
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_Interaction_part2");
        yield return new WaitForSeconds(clipDuration + 0.1f);

        yield return new WaitForSeconds(7);
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_LocatorInfo");
        // Wait until player has
        // Activated both objects and
        // dropped walkie talkie on floor
        yield return new WaitUntil(() => _hasActivatedFaucet && _hasDroppedWalkieTalkie);

        yield return new WaitForSeconds(1f);
        FindFirstObjectByType<WaterFaucet>().Deactivate();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Welldone", out clipDuration,true);
        yield return new WaitForSeconds(clipDuration + 0.1f);

        // Continue with tutorial
        yield return new WaitForSeconds(1.5f); // A short delay.
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_Continue", out clipDuration, true);
        yield return new WaitForSeconds(clipDuration + 1f);
    }
    private void HandleTutorialEnd()
    {
        _onTutorialCompleteAction?.Invoke();
        TTSPlayer.AddRepeatableTTS(TTSTUTORIALPATH + "TTS_ControlsListed");
    }
    public void SkipTutorial()
    {
        _interactionTutorialItems.SetActive(true);
        TTSPlayer.ForceStopAllTTS();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_SkippingTutorial");
        StopAllCoroutines();
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
