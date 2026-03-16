using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootupConfirmationHandler : MonoBehaviour
{
    [SerializeField] private float _loopingTTSDelay = 7f;
    private const string TTSTUTORIALPATH = "TTS/Tutorial/";
    private bool _hasSkipped = false;

    private string leftHandTTSFileString = TTSTUTORIALPATH + "TTS_Left";
    private string rightHandTTSFileString = TTSTUTORIALPATH + "TTS_Right";

    #region Unity Callbacks
    private void OnDisable()
    {
        EventManager.OnPlayerWantSkip.RemoveListener(this, OpenTutorialScene);

        EventManager.OnPlayerPushJoystick.RemoveListener(this, StickTTS);
        EventManager.OnPrimaryButtonPressed.RemoveListener(this, PrimaryButtonTTS);
        EventManager.OnSecondaryButtonPressed.RemoveListener(this, SecondaryButtonTTS);
        EventManager.OnTriggerPressed.RemoveListener(this, TriggerButtonTTS);
        EventManager.OnGripPressed.RemoveListener(this, GripButtonTTS);

        // This should be unnecessary as the player
        // is not marked as DontDestroyOnLoad
        // and each scene has a new player prefab and Instance
        //Player.Instance.EnableFingerSnapping();
        //Player.Instance.EnableTurnAndMove();
    }

    private void Start()
    {
        PlayBootUpConfirmationTTS(_loopingTTSDelay);
        Player.Instance.DisableFingerSnapping();
        Player.Instance.DisableTurnAndMove();
        Player.Instance.DisableNorthBeacon();

        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        EventManager.OnPlayerWantSkip.AddListener(this, OpenTutorialScene);

        EventManager.OnPlayerPushJoystick.AddListener(this, StickTTS);
        EventManager.OnPrimaryButtonPressed.AddListener(this, PrimaryButtonTTS);
        EventManager.OnSecondaryButtonPressed.AddListener(this, SecondaryButtonTTS);
        EventManager.OnTriggerPressed.AddListener(this, TriggerButtonTTS);
        EventManager.OnGripPressed.AddListener(this, GripButtonTTS);
    }

    private void UnsubcribeFromEvents()
    {
        EventManager.OnPlayerWantSkip.RemoveListener(this, OpenTutorialScene);

        EventManager.OnPlayerPushJoystick.RemoveListener(this, StickTTS);
        EventManager.OnPrimaryButtonPressed.RemoveListener(this, PrimaryButtonTTS);
        EventManager.OnSecondaryButtonPressed.RemoveListener(this, SecondaryButtonTTS);
        EventManager.OnTriggerPressed.RemoveListener(this, TriggerButtonTTS);
        EventManager.OnGripPressed.RemoveListener(this, GripButtonTTS);
    }



    #endregion


    private void OpenTutorialScene(int value)
    {
        if (_hasSkipped == true) return;
        // Skipping tutorial
        _hasSkipped = true;
        CancelInvoke();
        UnsubcribeFromEvents();
        TTSPlayer.ForceStopAllTTS();
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_OpeningTutorial", true);


        this.CallWithDelay(() =>
        {
            // Just to be sure
            CancelInvoke();
            TTSPlayer.ForceStopAllTTS();
            //
            SceneManager.LoadScene(1);
        }, 2f);

    }

    private void StickTTS(bool isRightHand)
    {
        string handednessString = isRightHand ? rightHandTTSFileString : leftHandTTSFileString;
        TTSPlayer.PlayTTSSequenceWithPaths(false, handednessString, TTSTUTORIALPATH + "TTS_Stick");
        TTSPlayer.PlayTTSSequenceWithPaths(false, handednessString);
        RestartBootUpConfirmationTTS(_loopingTTSDelay);
    }
    private void PrimaryButtonTTS(bool isRightHand)
    {
        string handednessString = isRightHand ? rightHandTTSFileString : leftHandTTSFileString;
        TTSPlayer.PlayTTSSequenceWithPaths(false, handednessString, TTSTUTORIALPATH + "TTS_PrimaryButton");
        TTSPlayer.PlayTTSSequenceWithPaths(false, handednessString);
        RestartBootUpConfirmationTTS(_loopingTTSDelay);
    }
    private void SecondaryButtonTTS(bool isRightHand)
    {
        string handednessString = isRightHand ? rightHandTTSFileString : leftHandTTSFileString;
        TTSPlayer.PlayTTSSequenceWithPaths(false, handednessString, TTSTUTORIALPATH + "TTS_SecondaryButton");
        RestartBootUpConfirmationTTS(_loopingTTSDelay);
    }
    private void TriggerButtonTTS(bool isRightHand)
    {
        string handednessString = isRightHand ? rightHandTTSFileString : leftHandTTSFileString;
        TTSPlayer.PlayTTSSequenceWithPaths(false, handednessString, TTSTUTORIALPATH + "TTS_TriggerButton");
        RestartBootUpConfirmationTTS(_loopingTTSDelay);
    }
    private void GripButtonTTS(bool isRightHand)
    {
        string handednessString = isRightHand ? rightHandTTSFileString : leftHandTTSFileString;
        TTSPlayer.PlayTTSSequenceWithPaths(false, handednessString, TTSTUTORIALPATH + "TTS_GripButton");
        RestartBootUpConfirmationTTS(_loopingTTSDelay);
    }
    private void RestartBootUpConfirmationTTS(float delay  = 2f)
    {
        StopAllCoroutines();
        PlayBootUpConfirmationTTS();
    }
    private void PlayBootUpConfirmationTTS(float delay = 7f)
    {
        this.CallWithDelay(() =>
        {
            TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_GameIsRunning");
        }, delay);

        this.CallWithDelay(() =>
        {
            TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_GameIsRunning");
        }, delay * 2);
    }

}