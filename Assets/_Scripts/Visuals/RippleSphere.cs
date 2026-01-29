using UnityEngine;

public class RippleSphere : MonoBehaviour
{
    [SerializeField] private float _spawnScale = 0.1f;
    [SerializeField] private Renderer _meshRenderer;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _duration = 1;

    private float _timer = 0f;
    private float _targetscale = 1f;
    private float _targetAlpha = 0f;
    private float _startAlpha = 1f;

    private void Start()
    {
        _targetscale = transform.localScale.x;
        transform.localScale = Vector3.one * _spawnScale;
        _startAlpha = _meshRenderer.material.GetFloat("_alpha");
    }

    public void ResetSphere()
    {
        gameObject.SetActive(false);
        transform.localScale = Vector3.one * _spawnScale;
        _meshRenderer.material.SetFloat("_alpha", _startAlpha);
        _timer = 0;
    }

    void Update()
    {
        // Alpha of material color;
        var lerpValue = _timer / _duration;
        var alpha = Mathf.Lerp(_startAlpha, _targetAlpha, lerpValue);
        _meshRenderer.material.SetFloat("_alpha", alpha);

        // Sphere scale
        transform.localScale = Vector3.Lerp(Vector3.one * _spawnScale, Vector3.one * _targetscale, lerpValue);

        _timer += Time.deltaTime;
        if (_timer > _duration + 1f)
        {
            ResetSphere();
            gameObject.SetActive(false);
        }
    }
}
