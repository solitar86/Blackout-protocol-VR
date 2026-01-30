using System;
using UnityEngine;
/// <summary>
/// Many VO's are held by objects themselves, this class deals with non-specific
/// VO reactioons such as "Ouch!" and curse words etc.
/// </summary>
public class PlayerVOReactionHandler : MonoBehaviour
{
    [SerializeField] private SoundArrayHolder _curseWords;

    void Start()
    {
        EventManager.OnPlayerCurse.AddListener(this, PlayerSayCurseWord);
    }

    private void OnDisable()
    {
        EventManager.OnPlayerCurse.RemoveListener(this, PlayerSayCurseWord);
    }

    private void PlayerSayCurseWord(int severity)
    {
        if(_curseWords != null && _curseWords.SoundArray != null && _curseWords.SoundArray.Length > 0)
        {
            AudioPlayer.PlayRandomSoundFromArrayAtPoint(this,
                                                        _curseWords.SoundArray,
                                                        transform.position,
                                                        _curseWords.LastPlayedSound);
        }
    }
}
