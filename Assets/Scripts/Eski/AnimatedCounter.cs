using UnityEngine;
using TMPro;
using System.Collections;

public class AnimatedCounter : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationDuration = 0.5f;

    private TextMeshProUGUI textComponent;
    private float currentDisplayedValue = 0f;
    private Coroutine countCoroutine;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    public void UpdateCounter(float targetValue)
    {
        if (countCoroutine != null)
        {
            StopCoroutine(countCoroutine);
        }

        countCoroutine = StartCoroutine(CountToValue(targetValue));
    }

    public void SetValueInstant(float value)
    {
        currentDisplayedValue = value;
        if (textComponent != null)
            textComponent.text = value.ToString("0");
    }

    private IEnumerator CountToValue(float targetValue)
    {
        float startValue = currentDisplayedValue;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            currentDisplayedValue = Mathf.Lerp(startValue, targetValue, elapsed / animationDuration);
            textComponent.text = currentDisplayedValue.ToString("0");
            yield return null;
        }

        currentDisplayedValue = targetValue;
        textComponent.text = targetValue.ToString("0");
    }
}