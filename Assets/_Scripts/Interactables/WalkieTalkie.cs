using System;
using UnityEngine;
using UnityEngine.Events;

public class WalkieTalkie : PickUpObject
{
    [Header("Default Convo for Tutorial")]
    [SerializeField] ConversationSO _tutorialConvo;
    [Space(5)]
    [Header("Walkie Talkie specific settings")]
    [SerializeField] Sound _pressCallButtonSound;
    [SerializeField] Sound _releaseCallButtonSound;
    [SerializeField] Sound _transmissionStartSound;
    [SerializeField] Sound _transmisisonEndSound;
    [SerializeField] Sound _radioStaticLoop;

    private AudioSource _staticLoopSource;
    private float _radioStaticDefaultVolume;

    public UnityEvent OnRadioActivated_UnityEvent;


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
    }
    #endregion

    [ContextMenu("Activate")]
    public override void Activate()
    {
        // THis is only for the tutorial
        if (_tutorialConvo != null)
        {
            ConversationManager.PlayConversation(_tutorialConvo);
            return;
        }

        EventManager.OnPlayerTryStartConversation.Raise(this, -1);
        OnRadioActivated_UnityEvent?.Invoke();
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

    #region EventCallbacks for SFX handling during conversations
    private void OnRadioVOStart(DialogueSO dialogueSO)
    {
        AudioPlayer.PlaySoundAtPoint(this, _transmissionStartSound, transform.position, false, true);
        DuckRadioStaticVolumeTo(0.1f);
    }
    private void OnRadioVOStop(DialogueSO dialogueSO)
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
    private void OnPlayerVOStop(DialogueSO dialogueSO)
    {
        if (dialogueSO.IsMonologue) return;
        AudioPlayer.PlaySoundAtPoint(this, _releaseCallButtonSound, transform.position, false, true);
        ResetRadioStaticVolume();
    }
    #endregion
}
