using UnityEngine;

public class Insect : PickUpObject
{
    [Space(15)]
    [Header("Insect specific settings")]
    [SerializeField] private bool _respawnAfterDeath = false;
    [SerializeField] private Vector2 _respawnMindAndMaxDelay = Vector2.one;
    [SerializeField] private Sound _insectLoopSound;
    AudioSource _insectLoopSource;
    [SerializeField] private bool _rightEar = true;
    [SerializeField] private float _velocityRequiredToKill = 1.7f;

    private void Start()
    {
        _insectLoopSource = AudioPlayer.CreateLoopingAudioSource(this, _insectLoopSound, true);
        _insectLoopSource.transform.position = transform.position;
        _insectLoopSource.transform.SetParent(transform);
        _insectLoopSource.gameObject.AddComponent<BeaconLPFController>();
    }
    public override void Touch(PlayerHand hand)
    {
        base.Touch(hand);

        if(hand.GetCurrentVelocity() > _velocityRequiredToKill)
        {
            // This kills the insect
            PickUp(null, hand);
        }
    }
    public override void PickUp(Transform parent, PlayerHand hand)
    {
        base.PickUp(parent, hand);
        //If we pickup the insect it will die.
        KillInsect(hand);
    }
    private void KillInsect(PlayerHand hand)
    {
        ForceRemoveObjectFromHandAndReturnToStartPosition(hand);
        GetComponent<Collider>().enabled = false;
        gameObject.SetActive(false);

        if(_respawnAfterDeath == true)
        {
            float delay = Random.Range(_respawnMindAndMaxDelay.x, _respawnMindAndMaxDelay.y);
            Invoke(nameof(Respawn),delay);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Respawn()
    {
        gameObject.SetActive(true);
    }
    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false) return;
        Gizmos.DrawCube(Player.Instance.GetPlayerEarPosition(_rightEar), Vector3.one * 0.1f);
    }
}
