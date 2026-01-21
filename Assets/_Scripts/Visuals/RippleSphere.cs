using UnityEngine;

public class RippleSphere : MonoBehaviour
{
    [SerializeField] private Renderer _meshRenderer;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _duration = 1;

    private float timer = 0f;
    private float _targetscale = 1f;
    private float _targetAlpha = 0f;
    private float _startAlpha = 1f;

    private void Start()
    {
        _targetscale = transform.localScale.x;
        transform.localScale *= 0f;
        _startAlpha = _meshRenderer.material.GetFloat("_alpha");
    }


    void Update()
    {
        // Alpha of material color;
        var lerpValue = Mathf.Lerp(_startAlpha, _targetAlpha, _duration / timer);
        _meshRenderer.material.SetFloat("_alpha", lerpValue);
        // Sphere scale
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * _targetscale, lerpValue);


        if (timer > _duration + 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
