using UnityEngine;
using UnityEngine.SceneManagement;

public class BootupConfirmationHandler : MonoBehaviour
{
    private const string TTSTUTORIALPATH = "TTS/Tutorial/";
    private bool _hasSkipped = false;
    private void Start()
    {
        TTSPlayer.PlayOnLoopWithFilePath(TTSTUTORIALPATH + "TTS_GameIsRunning");
        EventManager.OnPlayerWantSkip.AddListener(this, OpenTutorialScene);
    }

    private void OpenTutorialScene(int value)
    {
        if (_hasSkipped == true) return;
        // Skipping tutorial
        _hasSkipped = true;
        TTSPlayer.PlayTTSWithFilePath(TTSTUTORIALPATH + "TTS_OpeningTutorial");

        this.CallWithDelay(() =>
        {
            SceneManager.LoadScene(1);
        }, 2f);



    }

    private void OnDisable()
    {
        EventManager.OnPlayerWantSkip.RemoveListener(this, OpenTutorialScene);
    }
}