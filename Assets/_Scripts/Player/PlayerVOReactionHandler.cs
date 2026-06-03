using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LightTransport;
/// <summary>
/// Many VO's are held by objects themselves, this class deals with non-specific
/// VO reactioons such as "Ouch!" and curse words etc.
/// </summary>
public class PlayerVOReactionHandler : MonoBehaviour
{
    [Tooltip("How many reactions can be queued before more can be added")]
    [SerializeField] private int _maxQueudVoicelines = 2;
    [Tooltip("How much time between queued voice lines")]
    [SerializeField] private float _innerMonologueBuffer = 0.75f;
    [Tooltip("How much time before repeating voicelines if touching the same object")]
    [SerializeField] private float _IDVoicelineRepeatBufferDuration = 5f;
    [Space(5), Header("Voiceline Sound Holders")]
    [SerializeField] private SoundArrayHolder _leftTurnVO, _rightTurnVO;
    [SerializeField] private SoundArrayHolder _droppedObjectOnSurface;
    [SerializeField] private SoundArrayHolder _droppedObjectOnFloorVO;
    [SerializeField] private SoundArrayHolder _somethingHereVO;
    [SerializeField] private SoundArrayHolder _bumpIDUnknownObstacleVO;
    [SerializeField] private SoundArrayHolder _cantCarryVO;
    [SerializeField] private SoundArrayHolder _pickupSuccesfulVO;
    [SerializeField] private SoundArrayHolder _surfaceEmptyVO;

    private float _turnVODelay = 0.25f;
    private float _itemDroppedDelay = 0.5f;
    private float _itemDetectedDelay = 1f;
    private float _cantCarryDelay = 0.25f;
    private float _pickUpSuccessDelay = 0.1f;


    private float _nextTimeAllowInnerMonologue = 0f;
    private float _nextTimeAllowLocationIDVO = 0f;
    private float _nextTimeAllowIDVoiceLine = 0f;

    private Queue<Sound> _queuedVoiceLines = new();

    private Sound _previousLocationVO;
    private Sound _previousObjectIDVO;

    private const string VO_NUMBERSPATH = "VO_Numbers/VO_Numbers_";

    #region Unity Callbacks
    private void OnEnable()
    {
        EventManager.OnPlayerShouldSayNumber.AddListener(this, PlayNumberVO);
        EventManager.OnGeneralVOShouldPlay.AddListener(this, PlayGeneralVOLine);
        EventManager.PlayerVO_ObjectDropOnFloorReaction.AddListener(this, PlayDroppedOnFloorReaction);
        EventManager.OnCantCarryObject.AddListener(this, PlayerSayCantCarry);
        EventManager.OnPlayerObjectIDVOShouldPlay.AddListener(this, PlayTouchIDVoiceLine);
        EventManager.OnPlayerBumpIDVOShouldPlay.AddListener(this, PlayBumpIDVoiceLine);
        EventManager.OnPlayerLocationIDShouldPlay.AddListener(this, PlayerLocationIDVoiceLine);
        EventManager.OnInteractableDetectedOnSurface.AddListener(this, PlayItemDetectedVoiceLine);
        EventManager.OnSurfaceIsEmpthy.AddListener(this, PlayEmptySurfaceVoiceLine);
        EventManager.OnAnyObjectPickUpObjectPickedUp.AddListener(this, PlayPickUpSuccesfulVoiceLine);
        EventManager.OnAnyPickUpObjectPlacedOnSurface.AddListener(this, PlayDroppedObjectVoiceline);
        EventManager.OnPlayerSpillAllWater.AddListener(this, PlayGeneralVOLine);
        CustomSnapTurnProviderWrapper.OnPlayerSnapTurn += HandlePlayerTurn;
    }

    private void OnDisable()
    {
        EventManager.OnPlayerShouldSayNumber.RemoveListener(this, PlayNumberVO);
        EventManager.OnGeneralVOShouldPlay.RemoveListener(this, PlayGeneralVOLine);
        EventManager.PlayerVO_ObjectDropOnFloorReaction.RemoveListener(this, PlayDroppedOnFloorReaction);
        EventManager.OnCantCarryObject.RemoveListener(this, PlayerSayCantCarry);
        EventManager.OnPlayerObjectIDVOShouldPlay.RemoveListener(this, PlayTouchIDVoiceLine);
        EventManager.OnPlayerBumpIDVOShouldPlay.RemoveListener(this, PlayBumpIDVoiceLine);
        EventManager.OnPlayerLocationIDShouldPlay.RemoveListener(this, PlayerLocationIDVoiceLine);
        EventManager.OnInteractableDetectedOnSurface.RemoveListener(this, PlayItemDetectedVoiceLine);
        EventManager.OnSurfaceIsEmpthy.RemoveListener(this, PlayEmptySurfaceVoiceLine);
        EventManager.OnAnyObjectPickUpObjectPickedUp.RemoveListener(this, PlayPickUpSuccesfulVoiceLine);
        EventManager.OnPlayerSpillAllWater.RemoveListener(this, PlayGeneralVOLine);
        CustomSnapTurnProviderWrapper.OnPlayerSnapTurn -= HandlePlayerTurn;
    }

    #endregion

    /// <summary>
    /// Handles VO reactions to player using snapturn with controller.
    /// </summary>
    /// <param name="isRightTurn"> Are we turning right or left.</param>
    private void HandlePlayerTurn(bool isRightTurn)
    {
        if (isRightTurn == false)
        {
            if (_leftTurnVO != null && _leftTurnVO.SoundArray != null && _leftTurnVO.SoundArray.Length > 0)
            {
                PlayPlayerInnerMonologueWithDelay(_leftTurnVO, _turnVODelay);
            }
            return;
        }

        if (_rightTurnVO != null && _rightTurnVO.SoundArray != null && _rightTurnVO.SoundArray.Length > 0)
        {
            PlayPlayerInnerMonologueWithDelay(_rightTurnVO, _turnVODelay);
        }

    }
    
    /// <summary>
    /// Called when the player drops an item on the floor.
    /// This is different from droppin an item on a valid surface
    /// </summary>
    /// <param name="value"></param>
    private void PlayDroppedOnFloorReaction(int value)
    {
        if (_droppedObjectOnFloorVO != null && _droppedObjectOnFloorVO.SoundArray != null && _droppedObjectOnFloorVO.SoundArray.Length > 0)
        {
            PlayPlayerInnerMonologueWithDelay(_droppedObjectOnFloorVO, _itemDroppedDelay);
        }
    }
    /// <summary>
    /// Called when the player drops an item on valid surface to 
    /// confirm that the object has left their hand.
    /// </summary>
    /// <param name="object"></param>
    private void PlayDroppedObjectVoiceline(PickUpObject @object)
    {
        if (_droppedObjectOnSurface != null && _droppedObjectOnSurface.SoundArray != null && _droppedObjectOnSurface.SoundArray.Length > 0)
        {
            PlayPlayerInnerMonologueWithDelay(_droppedObjectOnSurface, _itemDroppedDelay);
        }
    }
    private void PlayerSayCantCarry(int value)
    {
        PlayPlayerInnerMonologueWithDelay(_cantCarryVO, _cantCarryDelay);
    }

    /// <summary>
    /// Handles voiceline from an event to inform player what they are touching.
    /// Adds buffers and other logic if necessary.
    /// </summary>
    /// <param name="IDVOSound"> The corresponding voiceline to play</param>
    private void PlayTouchIDVoiceLine(Sound IDVOSound)
    {
        if (_previousObjectIDVO != IDVOSound)
        {
            // This is not the same as the last VO. Play/queue right away. 
            PlayPlayerInnerMonologueWithDelay(IDVOSound, PlayerSettings.Developer.IdentifyVODelay);
            _previousObjectIDVO = IDVOSound;
        }
        else if (_nextTimeAllowIDVoiceLine < Time.time)
        {
            // We played this VO previously.Only play if buffer has elapsed.
            PlayPlayerInnerMonologueWithDelay(IDVOSound, PlayerSettings.Developer.IdentifyVODelay);
            _nextTimeAllowIDVoiceLine = Time.time + _IDVoicelineRepeatBufferDuration;
            // DO I need to REASSIGN the previous VO? I shouldn't right?
        }
    }
    /// <summary>
    /// Playes a corresponsind voiceline from an event to inform player what they bumped into.
    /// </summary>
    /// <param name="IDVOSound">The corresponding voiceline to play</param>
    private void PlayBumpIDVoiceLine(Sound IDVOSound)
    {
        // This null check is here because the Sound comes from another object.
        if (IDVOSound != null && IDVOSound.Clip != null)
        {
            PlayPlayerInnerMonologueWithDelay(IDVOSound, PlayerSettings.Developer.IdentifyVODelay);
            return;
        }

        Debugger.LogWarning("Bump ID VO was called with null sound", Debugger.TextColor.Orange);
        PlayPlayerInnerMonologueWithDelay(_bumpIDUnknownObstacleVO, PlayerSettings.Developer.IdentifyVODelay);
    }
    /// <summary>
    /// Plays the inputted voiceline with no delay. Will not play
    /// the voiceline if it the same as the previous one.
    /// </summary>
    /// <param name="sound">Voiceline to play</param>
    private void PlayerLocationIDVoiceLine(Sound sound)
    {
        if (sound == _previousLocationVO) return;
        if (_nextTimeAllowLocationIDVO > Time.time) return;

        _previousLocationVO = sound;
        float buffer = 0.1f;
        PlayPlayerInnerMonologueWithDelay(sound, 0f);
    }
    /// <summary>
    /// Playes a generic "something on it" voiceline when player
    /// touches a TouchableSurface that has a PickUp object on it.
    /// </summary>
    /// <param name="obj"></param>
    private void PlayItemDetectedVoiceLine(int value)
    {
        PlayPlayerInnerMonologueWithDelay(_somethingHereVO, _itemDetectedDelay);
    }
    /// <summary>
    /// Play when player touches surface with no objects
    /// </summary>
    /// <param name="value"></param>
    private void PlayEmptySurfaceVoiceLine(int value)
    {
        PlayPlayerInnerMonologueWithDelay(_surfaceEmptyVO, _itemDetectedDelay);
    }
    /// <summary>
    /// Played when a pickup action is succesful.
    /// </summary>
    /// <param name="value"></param>
    private void PlayPickUpSuccesfulVoiceLine(int value)
    {
        PlayPlayerInnerMonologueWithDelay(_pickupSuccesfulVO, _pickUpSuccessDelay);
    }
    /// <summary>
    /// A general VO function for anything non specific or one-shot voicelines.
    /// </summary>
    /// <param name="IDVOSound"></param>
    private void PlayGeneralVOLine(Sound IDVOSound)
    {
        PlayPlayerInnerMonologueWithDelay(IDVOSound, PlayerSettings.Developer.IdentifyVODelay);
    }
    /// <summary>
    /// Playes "innermonologue" numbers from 1-12. Used only for the safe dial currently
    /// </summary>
    /// <param name="number">Int from 1-12 to speak</param>
    public void PlayNumberVO(int number)
    {
        string numberString = GetStringFromNumber(number);
        var clip = Resources.Load<AudioClip>(VO_NUMBERSPATH + numberString);
        if(clip == null)
        {
            Debugger.LogWarning($"VO Number was not found with value {number} with path '{VO_NUMBERSPATH + numberString}'");
            return;
        }
        Sound numberLine = new Sound(clip);
        numberLine.Mixergroup = AudioPlayer.GetMixerGroupWithSubPathString(PlayerSettings.INNER_MONOLOGUE_MIXERGROUP_STRING); // DIRTY, I Know...
        PlayGeneralVOLine(numberLine);

    }
    /// <summary>
    /// Playes one sound from an array with no spatialization as if they are players thoughts.
    /// All InnerMonologue goes through "with delay" and 0 delay = immediately.
    /// </summary>
    /// <param name="soundHolder">Soundholder with variations of sound to play.</param>
    /// <param name="delay">If set to 0 will play immediately</param>
    private void PlayPlayerInnerMonologueWithDelay(SoundArrayHolder soundHolder, float delay)
    {
        if (soundHolder != null && soundHolder.SoundArray != null && soundHolder.SoundArray.Length > 0)
        {
            var sound = AudioPlayer.GetRandomSoundFromArray(soundHolder.SoundArray, soundHolder.LastPlayedSound);
            PlayPlayerInnerMonologueWithDelay(sound, delay);
        }
        else
        {
            Debugger.Log("Inner monologue with delay was called with null or empty sound holder");
        }

    }
    /// <summary>
    /// Playes a single sound with no spatialization as if they are players thoughts.
    /// All InnerMonologue goes through "with delay" and 0 delay = immediately.
    /// </summary>
    /// <param name="soundHolder">Soundholder with variations of sound to play.</param>
    /// <param name="delay">If set to 0 will play immediately</param>
    private void PlayPlayerInnerMonologueWithDelay(Sound soundToPlay, float delay)
    {
        if (InnerMonologueIsBlocked() == true)
        {
            if (_queuedVoiceLines.Count >= _maxQueudVoicelines) return;
            _queuedVoiceLines.Enqueue(soundToPlay);
            return;
        }

        _nextTimeAllowInnerMonologue = Time.time + _innerMonologueBuffer;
        if (soundToPlay != null && soundToPlay.Clip != null)
        {
            this.CallWithDelay(() =>
            {
                bool spatialize = false;
                bool pitchVary = false;
                AudioPlayer.PlaySoundAtPoint(this,
                                            soundToPlay,
                                            transform.position,
                                            pitchVary,
                                            spatialize);
            }, delay);

            // if we have queued reactions, try play the next one.
            this.CallWithDelay(() =>
            {
                TryPlayedQueudVOReaction();
            }, delay + _innerMonologueBuffer);
        }
        else
        {
            Debugger.Log("Inner monologue was called with null or empty sound");
        }
    }

    #region Helpers etc.
    private void TryPlayedQueudVOReaction()
    {
        if (_queuedVoiceLines.Count > 0)
        {
            PlayPlayerInnerMonologueWithDelay(_queuedVoiceLines.Dequeue(), delay: 0f);
        }
    }
    private bool InnerMonologueIsBlocked()
    {
        if (_nextTimeAllowInnerMonologue > Time.time) return true;
        if (ConversationManager.PlayerIsSpeaking == true) return true;
        if (RadialMenuManager.Instance.MenuIsOpen == true) return true;
        return false;
    }
    private string GetStringFromNumber(int number)
    {
        if (number > 12)
        {
            return null;
        }

        switch (number)
        {
            case 0: return "Zero";
            case 1: return "One";
            case 2: return "Two";
            case 3: return "Three";
            case 4: return "Four";
            case 5: return "Five";
            case 6: return "Six";
            case 7: return "Seven";
            case 8: return "Eight";
            case 9: return "Nine";
            case 10: return "Ten";
            case 11: return "Eleven";
            case 12: return "Twelve";
            default:
                return string.Empty;
        }
    }

    #endregion region
}
