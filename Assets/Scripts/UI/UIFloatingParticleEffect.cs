using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFloatingParticleEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    public Sprite particleSprite;
    public int particleCount = 10;
    public Color particleColor = new Color(0.75f, 0.25f, 1f, 0.85f);

    [Header("Area")]
    public Vector2 spawnArea = new Vector2(120f, 100f);
    public float minSpeed = 10f;
    public float maxSpeed = 25f;
    public float minSize = 6f;
    public float maxSize = 12f;
    public float sideWaveAmount = 10f;

    private readonly List<RectTransform> particles = new List<RectTransform>();
    private readonly List<Image> images = new List<Image>();
    private readonly List<float> speeds = new List<float>();
    private readonly List<float> startX = new List<float>();
    private readonly List<float> waveSpeed = new List<float>();

    private void OnEnable()
    {
        CreateParticlesIfNeeded();
        ResetParticles();
    }

    private void Update()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            RectTransform particle = particles[i];

            if (particle == null)
                continue;

            Vector2 pos = particle.anchoredPosition;

            pos.y += speeds[i] * Time.deltaTime;
            pos.x = startX[i] + Mathf.Sin(Time.time * waveSpeed[i] + i) * sideWaveAmount;

            particle.anchoredPosition = pos;
            particle.Rotate(0f, 0f, 45f * Time.deltaTime);

            if (images[i] != null)
            {
                Color c = images[i].color;
                float fade = Mathf.InverseLerp(spawnArea.y / 2f, -spawnArea.y / 2f, pos.y);
                c.a = Mathf.Clamp01(fade) * particleColor.a;
                images[i].color = c;
            }

            if (pos.y > spawnArea.y / 2f)
                ResetParticle(i);
        }
    }

    private void CreateParticlesIfNeeded()
    {
        if (particles.Count > 0)
            return;

        for (int i = 0; i < particleCount; i++)
        {
            GameObject obj = new GameObject("FloatingParticle_" + i, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(transform, false);

            Image image = obj.GetComponent<Image>();
            image.sprite = particleSprite;
            image.color = particleColor;
            image.raycastTarget = false;

            RectTransform rect = obj.GetComponent<RectTransform>();
            float size = Random.Range(minSize, maxSize);
            rect.sizeDelta = new Vector2(size, size);

            particles.Add(rect);
            images.Add(image);
            speeds.Add(Random.Range(minSpeed, maxSpeed));
            startX.Add(0f);
            waveSpeed.Add(Random.Range(1f, 2.5f));
        }
    }

    private void ResetParticles()
    {
        for (int i = 0; i < particles.Count; i++)
            ResetParticle(i);
    }

    private void ResetParticle(int index)
    {
        if (particles[index] == null)
            return;

        float x = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
        float y = Random.Range(-spawnArea.y / 2f, 0f);

        startX[index] = x;
        particles[index].anchoredPosition = new Vector2(x, y);

        float size = Random.Range(minSize, maxSize);
        particles[index].sizeDelta = new Vector2(size, size);

        speeds[index] = Random.Range(minSpeed, maxSpeed);
        waveSpeed[index] = Random.Range(1f, 2.5f);

        if (images[index] != null)
            images[index].color = particleColor;
    }
}