using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TypewriterEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    public float typingSpeed = 0.03f; // Delay between each letter (lower is faster)

    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    // Call this method from other scripts instead of setting textComponent.text directly
    public void ShowText(string fullText)
    {
        // If a text is currently typing, stop it before starting the new one
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeTextCoroutine(fullText));
    }

    private IEnumerator TypeTextCoroutine(string textToType)
    {
        textComponent.text = ""; // Clear the text field

        // Loop through each character and add it one by one
        foreach (char letter in textToType.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed); // Wait for a fraction of a second
        }
    }
}