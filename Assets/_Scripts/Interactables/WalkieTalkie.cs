using UnityEngine;

public class WalkieTalkie : PickUpObject
{
    [Header("Walkie Talkie specific settings")]
    [SerializeField] Sound _pressCallButtonSound;
    [SerializeField] Sound _releaseCallButtonSound;
    [SerializeField] Sound _transmisisonEndSound;
    public override void Activate()
    {
        AudioPlayer.PlaySoundAtPoint(this, _pressCallButtonSound, transform.position, false, true);

        float delay = 1f;
        this.CallWithDelay(() =>
        {
            AudioPlayer.PlaySoundAtPoint(this, _releaseCallButtonSound, transform.position, false, true);
        }, delay);
    }

}
