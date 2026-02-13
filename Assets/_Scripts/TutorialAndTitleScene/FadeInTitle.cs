using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class FadeInTitle : MonoBehaviour
{
    [SerializeField] private float _duration = 3f;

    public void FadeIn()
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeInCoroutine());
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    private IEnumerator FadeInCoroutine()
    {
        gameObject.SetActive(true);
        var text = GetComponent<TextMeshPro>();
        var startColor = new Color(0, 0, 0, 0);
        var endColor = text.color;
        text.color = startColor;
        yield return new WaitForSeconds(2f);

        float timer = 0;
        while(timer < _duration)
        {
            timer += Time.deltaTime;
            text.color = Color.Lerp(startColor, endColor, timer / _duration);
            yield return null;
        }

        text.color = endColor;
    }
}
