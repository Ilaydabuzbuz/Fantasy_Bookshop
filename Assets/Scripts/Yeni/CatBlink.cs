using UnityEngine;
using System.Collections;

public class CatBlink : MonoBehaviour
{
    public SpriteRenderer eyes;
    public Sprite openEyes;
    public Sprite closedEyes;

    void Start()
    {
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            eyes.sprite = closedEyes;
            yield return new WaitForSeconds(0.12f);

            eyes.sprite = openEyes;
        }
    }
}