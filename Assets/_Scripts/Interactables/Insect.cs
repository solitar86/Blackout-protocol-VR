using UnityEditor;
using UnityEngine;

public class Insect : PickUpObject
{
    [Space(15)]
    [Header("Insect specific settings")]
    [Tooltip("If checked then this insect will respawn after a random delay which is set below.")]
    [SerializeField] private bool _respawnAfterDeath = false;
    [Tooltip("Min and max values for insect to respawn after death. ")]
    [SerializeField] private Vector2 _respawnMindAndMaxDelay = Vector2.one;
    [SerializeField] private Sound _insectLoopSound;
    [SerializeField] private float _velocityRequiredToKill = 1.7f;

    AudioSource _insectLoopSource;

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
        GetComponent<Collider>().enabled = true;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        if (_insectLoopSound != null)
        {
            Gizmos.color = Color.lightBlue;
            Gizmos.DrawWireSphere(transform.position, _insectLoopSound.MinDistance);
            Gizmos.DrawWireSphere(transform.position, _insectLoopSound.MaxDistance);
        }
#if UNITY_EDITOR
        Handles.Label(
            transform.position + Vector3.up * 0.15f, gameObject.name);
#endif
    }
}
