using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerClickHandler : MonoBehaviour
{
    public static Sprite selectedCustomerSprite;
    public static CustomerAI selectedCustomerAI;
    public static bool traitsOpen = false;

    private void OnMouseDown()
    {
        if (traitsOpen) return;
        selectedCustomerSprite = GetComponent<SpriteRenderer>().sprite;
        selectedCustomerAI = GetComponent<CustomerAI>();
        traitsOpen = true;
        SceneManager.LoadScene("CustomerTraitsScreen", LoadSceneMode.Additive);
    }
}