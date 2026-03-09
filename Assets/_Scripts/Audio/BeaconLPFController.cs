using UnityEngine;

public class BeaconLPFController : MonoBehaviour
{
    [SerializeField] private AudioLowPassFilter _lowPassFilter;
    [SerializeField] private float _lowPassHighestValue = 0f;

    private float _lpfMaxValue = 220000f;
    private AudioSource _source;

    private bool _hasFilter = false;
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _hasFilter = InitLowPassFilter();

        if (_hasFilter == true)
        {
            
        }
    }

    private void Update()
    {
        if (_source.isVirtual) return;

        Vector3 directionToPlayerHead =
            (Player.Instance.GetPlayerHeadTransform().position - transform.position);
        directionToPlayerHead.y = 0f;
        directionToPlayerHead.Normalize();

        var playerLookDirection = Player.Instance.GetPlayerLookingDirection();
        playerLookDirection.y = 0;
        playerLookDirection.Normalize();


        //TODO: Should I zero out the Y component???
        float dotProduct = Vector3.Dot(directionToPlayerHead,
                                        playerLookDirection);

        var lerpValue = Mathf.InverseLerp(1f, -1f, dotProduct);
         _lowPassFilter.cutoffFrequency = Mathf.Lerp(_lowPassHighestValue, _lpfMaxValue, lerpValue);
        
        //Debugger.Log("ToPlayer: " + directionToPlayerHeadNormalized.ToString() + " LookDIR: " + playerLookDirection.ToString());
        Debugger.Log("DOT:" + dotProduct.ToString("F2")+" LERP: " + lerpValue.ToString("F2"), Debugger.TextColor.Yellow);
    }

    private bool InitLowPassFilter()
    {
        TryGetComponent<AudioSource>(out _source);
        if (_source != null)
        {
            if (_lowPassFilter != null)
            {
                return true;
            }
            else
            {
                Debugger.Log("Filter is null");
                if (TryGetComponent<AudioLowPassFilter>(out var filter))
                {
                    _lowPassFilter = filter;
                    Debugger.Log("Found filter");
                    return true;
                }
                else
                {
                    _lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
                    Debugger.Log("Added filter");
                    return true;
                }
            }
        }
        {
            Debugger.LogWarning("Can't assign LowPassFilter, no audiosource found on object; " + gameObject.name);
            return false;
        }
    }
}
