using System;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// Many VO's are held by objects themselves, this class deals with non-specific
/// VO reactioons such as "Ouch!" and curse words etc.
/// UPDATE 2.2.2026
/// Started centralizing "inner" monologue to this class starting from Touch ID VO.
/// At the very least to have a centralized way of going through.
/// </summary>
public class PlayerVOReactionHandler : MonoBehaviour
{
    [SerializeField] private SoundArrayHolder _curseWords;

    void Start()
    {
        EventManager.OnPlayerCurse.AddListener(this, PlayerSayCurseWord);
        EventManager.OnPlayerObjectIDVOShouldPlay.AddListener(this, PlayTouchIDVoiceLine);
    }

    private void OnDisable()
    {
        EventManager.OnPlayerCurse.RemoveListener(this, PlayerSayCurseWord);
        EventManager.OnPlayerObjectIDVOShouldPlay.RemoveListener(this, PlayTouchIDVoiceLine);
    }

    private void PlayTouchIDVoiceLine(Sound IDVOSound)
    {
        Debugger.Log(this.ToString() + "CALLED ID VO", Debugger.TextColor.Purple);
        AudioPlayer.PlayerSoundAtPointWithDelay(this, IDVOSound, Vector3.zero, PlayerSettings.Developer.IdentifyVODelay, false, false);
    }

    private void PlayerSayCurseWord(int severity)
    {
        if(_curseWords != null && _curseWords.SoundArray != null && _curseWords.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _curseWords.SoundArray,
                                                        transform.position,
                                                        _curseWords.LastPlayedSound, false, false);
        }
    }
}
