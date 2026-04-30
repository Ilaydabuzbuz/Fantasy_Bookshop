using UnityEngine;
using UnityEngine.EventSystems;

public class SpriteButtonHover : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    SpriteRenderer sr;
    Color normal = Color.white;
    Color hover = new Color(1f, 0.9f, 0.7f, 1f);
    Color pressed = new Color(0.6f, 0.6f, 0.6f, 1f);

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        sr.color = hover;
    }

    public void OnPointerExit(PointerEventData e)
    {
        sr.color = normal;
    }

    public void OnPointerDown(PointerEventData e)
    {
        sr.color = pressed;
    }

    public void OnPointerUp(PointerEventData e)
    {
        sr.color = hover;
    }
}