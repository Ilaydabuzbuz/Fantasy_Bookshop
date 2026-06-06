using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINatureLeafEffect : MonoBehaviour
{
    [Header("Leaf Settings")]
    public Sprite leafSprite;
    public int leafCount = 8;
    public Color leafColor = new Color(0.55f, 1f, 0.65f, 0.75f);

    [Header("Area")]
    public Vector2 spawnArea = new Vector2(120f, 80f);
    public float minSpeed = 18f;
    public float maxSpeed = 35f;
    public float minSize = 10f;
    public float maxSize = 18f;

    private readonly List<RectTransform> leaves = new List<RectTransform>();
    private readonly List<float> speeds = new List<float>();
    private readonly List<float> sideMotion = new List<float>();
    private readonly List<float> startX = new List<float>();

    private void OnEnable()
    {
        CreateLeavesIfNeeded();
        ResetLeaves();
    }

    private void Update()
    {
        for (int i = 0; i < leaves.Count; i++)
        {
            RectTransform leaf = leaves[i];

            if (leaf == null)
                continue;

            Vector2 pos = leaf.anchoredPosition;

            pos.y += speeds[i] * Time.deltaTime;
            pos.x = startX[i] + Mathf.Sin(Time.time * sideMotion[i] + i) * 12f;

            leaf.anchoredPosition = pos;
            leaf.Rotate(0f, 0f, 35f * Time.deltaTime);

            if (pos.y > spawnArea.y / 2f)
                ResetLeaf(i);
        }
    }

    private void CreateLeavesIfNeeded()
    {
        if (leaves.Count > 0)
            return;

        for (int i = 0; i < leafCount; i++)
        {
            GameObject leafObj = new GameObject("NatureLeaf_" + i, typeof(RectTransform), typeof(Image));
            leafObj.transform.SetParent(transform, false);

            Image image = leafObj.GetComponent<Image>();
            image.sprite = leafSprite;
            image.color = leafColor;
            image.raycastTarget = false;

            RectTransform rect = leafObj.GetComponent<RectTransform>();
            float size = Random.Range(minSize, maxSize);
            rect.sizeDelta = new Vector2(size, size);

            leaves.Add(rect);
            speeds.Add(Random.Range(minSpeed, maxSpeed));
            sideMotion.Add(Random.Range(1f, 2.5f));
            startX.Add(0f);
        }
    }

    private void ResetLeaves()
    {
        for (int i = 0; i < leaves.Count; i++)
            ResetLeaf(i);
    }

    private void ResetLeaf(int index)
    {
        if (leaves[index] == null)
            return;

        float x = Random.Range(-spawnArea.x / 2f, spawnArea.x / 2f);
        float y = Random.Range(-spawnArea.y / 2f, 0f);

        startX[index] = x;
        leaves[index].anchoredPosition = new Vector2(x, y);

        float size = Random.Range(minSize, maxSize);
        leaves[index].sizeDelta = new Vector2(size, size);

        speeds[index] = Random.Range(minSpeed, maxSpeed);
        sideMotion[index] = Random.Range(1f, 2.5f);
    }
}