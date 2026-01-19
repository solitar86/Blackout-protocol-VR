using System;
using UnityEngine;

public class TouchableObject : MonoBehaviour
{
    [SerializeField] AudioClip _objectDescriptionClip;

    private bool _hasBeenTriggered = false;
    private float _delayBetweenTriggers = 1f;
    private void OnTriggerEnter(Collider other)
    {
        AudioSource.PlayClipAtPoint(_objectDescriptionClip, transform.position, 0.3f);
        if (_hasBeenTriggered = false && other.TryGetComponent<ITouchAudio>(out _))
        {
            _hasBeenTriggered = true;
            AudioSource.PlayClipAtPoint(_objectDescriptionClip, transform.position, 0.3f);
            Invoke(nameof(ResetHasBeenTriggered), _delayBetweenTriggers);
        }
    }

    private void ResetHasBeenTriggered()
    {
        _hasBeenTriggered = false;
    }
}
