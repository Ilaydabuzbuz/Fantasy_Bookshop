using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image bookIcon;
    public TextMeshProUGUI priceText;
    public GameObject emptyOverlay; // kutucuk boþsa gösterilecek obje (opsiyonel)

    private ItemData currentItem;

    public void SetItem(ItemData item)
    {
        currentItem = item;

        if (bookIcon != null)
        {
            bookIcon.sprite = item.itemIcon;
            bookIcon.gameObject.SetActive(true);
        }

        if (priceText != null)
            priceText.text = $"{item.basePrice:0}";

        if (emptyOverlay != null)
            emptyOverlay.SetActive(false);
    }

    public void ClearSlot()
    {
        currentItem = null;

        if (bookIcon != null)
        {
            bookIcon.sprite = null;
            bookIcon.gameObject.SetActive(false);
        }

        if (priceText != null)
            priceText.text = "";

        if (emptyOverlay != null)
            emptyOverlay.SetActive(true);
    }
}