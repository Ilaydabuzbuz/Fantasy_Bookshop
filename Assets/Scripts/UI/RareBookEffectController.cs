using System.Collections.Generic;
using UnityEngine;

public class RareBookEffectController : MonoBehaviour
{
    [System.Serializable]
    public class RareEffectEntry
    {
        public RareBookEffectType effectType;
        public List<GameObject> effectObjects = new List<GameObject>();
    }

    [Header("Effect Entries")]
    public List<RareEffectEntry> effects = new List<RareEffectEntry>();

    [Header("Book Floating Animation")]
    public RectTransform bookRect;
    public bool enableFloating = true;
    public float floatSpeed = 1.8f;
    public float floatAmount = 8f;

    private Vector2 startPosition;
    private RareBookEffectType activeEffectType = RareBookEffectType.None;

    private void Awake()
    {
        if (bookRect == null)
            bookRect = GetComponent<RectTransform>();

        if (bookRect != null)
            startPosition = bookRect.anchoredPosition;

        StopAllEffects();
    }

    private void OnEnable()
    {
        if (bookRect != null)
            startPosition = bookRect.anchoredPosition;
    }

    private void Update()
    {
        AnimateBookFloating();
    }

    private void AnimateBookFloating()
    {
        if (!enableFloating)
            return;

        if (bookRect == null)
            return;

        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        bookRect.anchoredPosition = startPosition + new Vector2(0f, yOffset);
    }

    public void PlayEffect(ItemData item)
    {
        if (item == null)
        {
            StopAllEffects();
            return;
        }

        Debug.Log($"[RareBookEffectController] Book: {item.itemName}, Effect: {item.rareBookEffectType}");

        PlayEffect(item.rareBookEffectType);
    }

    public void PlayEffect(RareBookEffectType effectType)
    {
        StopAllEffects();

        activeEffectType = effectType;

        if (effectType == RareBookEffectType.None)
            return;

        foreach (RareEffectEntry entry in effects)
        {
            if (entry == null)
                continue;

            if (entry.effectType != effectType)
                continue;

            foreach (GameObject effectObject in entry.effectObjects)
            {
                if (effectObject == null)
                    continue;

                Debug.Log($"[RareBookEffectController] Activating effect object: {effectObject.name}");

                effectObject.SetActive(true);

                PlayParticles(effectObject);
                PlayAnimators(effectObject);
            }

            return;
        }

        Debug.LogWarning($"RareBookEffectController: No effect object assigned for {effectType}");
    }

    public void StopAllEffects()
    {
        activeEffectType = RareBookEffectType.None;

        foreach (RareEffectEntry entry in effects)
        {
            if (entry == null)
                continue;

            foreach (GameObject effectObject in entry.effectObjects)
            {
                if (effectObject == null)
                    continue;

                StopParticles(effectObject);
                effectObject.SetActive(false);
            }
        }
    }

    private void PlayParticles(GameObject root)
    {
        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.gameObject.SetActive(true);
            ps.Clear();
            ps.Play();
        }
    }

    private void StopParticles(GameObject root)
    {
        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void PlayAnimators(GameObject root)
    {
        Animator[] animators = root.GetComponentsInChildren<Animator>(true);

        foreach (Animator animator in animators)
        {
            animator.gameObject.SetActive(true);
            animator.Rebind();
            animator.Update(0f);
        }
    }
}