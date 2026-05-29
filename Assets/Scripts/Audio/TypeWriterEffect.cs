using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class TypeWriterEffect : MonoBehaviour
{
    [Header("Typewriter Settings")]
    [Tooltip("Saniyede kaç karakter yazılsın")]
    public float charsPerSecond = 30f;

    [Tooltip("Noktalama işaretlerinde ekstra bekleme (saniye)")]
    public float punctuationPause = 0.15f;

    [Header("Sound Settings")]
    [Tooltip("Ses tipi: Tick (tık tık) veya Typewriter (daktilo)")]
    public SoundStyle soundStyle = SoundStyle.Tick;

    [Tooltip("Her kaç karakterde bir ses çalsın (1 = her harf)")]
    [Range(1, 5)]
    public int soundInterval = 2;

    [Tooltip("Ses tonu varyasyonu (0 = sabit ton, 0.1 = hafif değişken)")]
    [Range(0f, 0.3f)]
    public float pitchVariation = 0.08f;

    [Tooltip("Ses yüksekliği")]
    [Range(0f, 1f)]
    public float volume = 0.4f;

    public enum SoundStyle { Tick, Typewriter, Soft }

    private TextMeshProUGUI _text;
    private AudioSource _audioSource;
    private Coroutine _typingCoroutine;
    private AudioClip _clickClip;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f; // 2D ses

        _clickClip = GenerateClickClip();
    }

    public void Play(string message)
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeRoutine(message));
    }


    public bool IsPlaying => _typingCoroutine != null;


    private IEnumerator TypeRoutine(string message)
    {
        if (_text == null) yield break;

        _text.text = "";
        float delay = 1f / charsPerSecond;
        int charCount = 0;

        for (int i = 0; i < message.Length; i++)
        {
            char c = message[i];
            _text.text += c;
            charCount++;

            if (charCount % soundInterval == 0 && c != ' ')
                PlayClick();

            if (c == '.' || c == '!' || c == '?' || c == ',')
                yield return new WaitForSeconds(punctuationPause);
            else
                yield return new WaitForSeconds(delay);
        }

        _typingCoroutine = null;
    }

    private void PlayClick()
    {
        if (_audioSource == null || _clickClip == null) return;

        float basePitch = soundStyle switch
        {
            SoundStyle.Tick => 2.2f,
            SoundStyle.Typewriter => 1.6f,
            SoundStyle.Soft => 1.0f,
            _ => 1.8f
        };

        _audioSource.pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
        _audioSource.volume = volume;
        _audioSource.PlayOneShot(_clickClip);
    }

    private AudioClip GenerateClickClip()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int durationMs = soundStyle == SoundStyle.Typewriter ? 18 : 10;
        int sampleCount = sampleRate * durationMs / 1000;

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Exp(-t * 30f); 

            float wave = soundStyle switch
            {
                SoundStyle.Tick => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 800f * t)),  // kare dalga
                SoundStyle.Typewriter => Mathf.Sin(2f * Mathf.PI * 400f * t),              // sinüs
                SoundStyle.Soft => Mathf.Sin(2f * Mathf.PI * 600f * t),
                _ => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 600f * t))
            };

            samples[i] = wave * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create("TypeWriterClick", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}