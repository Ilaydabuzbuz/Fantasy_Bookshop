using UnityEngine;

public class TailWag : MonoBehaviour
{
    void Update()
    {
        transform.localRotation =
            Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 3f) * 15f);
    }
}