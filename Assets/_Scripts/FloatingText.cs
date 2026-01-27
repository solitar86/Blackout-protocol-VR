using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private float _fadeDuration = 2f;
    [SerializeField] private float _posMultiplier = 1f;

    [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Color _defaultColor;

    private TextMeshProUGUI _textMesh;
    private Color _textColor;

    public static void Create(Vector3 position, string text, Color textColor = default, float textScale = 1f)
    {
        var floatingTextPrefab = Resources.Load<GameObject>("FloatingText");
        if (floatingTextPrefab == null)
        {
            Debug.LogError("FloatingTextPrefab not found in Resources folder!");
            return;
        }

        var instance = Instantiate(floatingTextPrefab, position, Quaternion.identity);

        var tmpro = instance.GetComponentInChildren<TextMeshProUGUI>();
        tmpro.text = text;
        tmpro.fontSize *= textScale;
        tmpro.color = textColor;

        var floatingText = instance.GetComponent<FloatingText>();
        floatingText._textMesh = tmpro;
        floatingText._textColor = textColor;
    }

    public static void CreateTextAtCursor(string message, Color color, float yOffset = 1f, float textScale = 1f)
    {
        var position = Vector3.zero; // TODO: ACTUALLY GET MOUSE POSITION
        Create(position, message, color, textScale);
    }

    private void Start()
    {
        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        var timer = 0f;

        var startPosition = transform.position;
        var endPosition = transform.position + Vector3.up * _posMultiplier;

        var initialColor = _textColor;
        var fadeColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0);

        var mainCamTransform = Camera.main.transform;

        while (timer < _fadeDuration)
        {
            timer += Time.deltaTime;
            var t = timer / _fadeDuration;
            var curveValue = _curve.Evaluate(t);

            _textMesh.color = Color.Lerp(initialColor, fadeColor, t);
            transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);

            transform.forward = mainCamTransform.forward;
            yield return null;
        }

        Destroy(gameObject);
    }
}
