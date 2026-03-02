using System;
using UnityEngine;

public class WalkieTalkie : PickUpObject
{
    [Header("Walkie Talkie specific settings")]
    [SerializeField] Sound _pressCallButtonSound;
    [SerializeField] Sound _releaseCallButtonSound;
    [SerializeField] Sound _transmissionStartSound;
    [SerializeField] Sound _transmisisonEndSound;
    [SerializeField] Sound _radioStaticLoop;

    private AudioSource _staticLoopSource;
    private float _radioStaticDefaultVolume;

    [SerializeField] ConversationSO _callPlayerConversation;
    [SerializeField] ConversationSO _response;


    #region Unity Callbacks
    private void OnEnable()
    {
        EventManager.OnDialogueStart_Radio.AddListener(this, OnRadioVOStart);
        EventManager.OnDialogueStop_Radio.AddListener(this, OnRadioVOStop);
        EventManager.OnDialogueStart_Player.AddListener(this, OnPlayerVOStart);
        EventManager.OnDialogueStop_Player.AddListener(this, OnPlayerVOStop);
    }
    private void OnDisable()
    {
        EventManager.OnDialogueStart_Radio.RemoveListener(this, OnRadioVOStart);
        EventManager.OnDialogueStop_Radio.RemoveListener(this, OnRadioVOStop);
        EventManager.OnDialogueStart_Player.RemoveListener(this, OnPlayerVOStart);
        EventManager.OnDialogueStop_Player.RemoveListener(this, OnPlayerVOStop);
    }
    private void Start()
    {
        PlayRadioStaticBeaconLoop();
        PlayConversationOnLoop(_callPlayerConversation);
    }
    #endregion
    public override void Activate()
    {
        // FOr now:
        AudioPlayer.PlaySoundAtPoint(this, _pressCallButtonSound, transform.position, false, true);
        // Handle context sensitive conversation triggering somehow
        // maybe from a Quest manager or hint system or both?
        ConversationManager.OverrideCurrentConversationWith(_response);
    }
    public void PlayConversationOnLoop(ConversationSO convoToLoop)
    {
        ConversationManager.PlayConversationOnLoop(convoToLoop);
    }
    private void PlayRadioStaticBeaconLoop()
    {
        if(_staticLoopSource == null)
        {
            InitStaticLoopAudioSource();
            return;
        }

        _staticLoopSource.Play();
    }
    private void InitStaticLoopAudioSource()
    {
        _staticLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _radioStaticLoop, true);
        _radioStaticDefaultVolume = _radioStaticLoop.Volume;
        _staticLoopSource.transform.position = transform.position;
        _staticLoopSource.transform.SetParent(transform);
    }
    private void DuckRadioStaticVolumeTo(float ratio) =>_staticLoopSource.volume *= ratio;
    private void ResetRadioStaticVolume() => _staticLoopSource.volume = _radioStaticDefaultVolume;

    #region EventCallbacks for SFX handling
    private void OnRadioVOStart(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _transmissionStartSound, transform.position, false, true);
        DuckRadioStaticVolumeTo(0.1f);
    }
    private void OnRadioVOStop(int value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _transmisisonEndSound, transform.position, false, true);
        ResetRadioStaticVolume();
    }
    private void OnPlayerVOStart(DialogueSO dialogueSO)
    {
        if (dialogueSO.IsMonologue) return;
        AudioPlayer.PlaySoundAtPoint(this, _pressCallButtonSound, transform.position, false, true);
        DuckRadioStaticVolumeTo(0f);
    }
    private void OnPlayerVOStop(float value)
    {
        AudioPlayer.PlaySoundAtPoint(this, _releaseCallButtonSound, transform.position, false, true);
        ResetRadioStaticVolume();
    }
    #endregion
}
