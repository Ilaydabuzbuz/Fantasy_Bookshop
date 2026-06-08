using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryBookPopupUI : MonoBehaviour
{
    [Header("Popup Root")]
    public GameObject popupPanel;

    [Header("Close Button")]
    public Button closeButton;

    [Header("Book Main Info")]
    public TextMeshProUGUI bookTitleText;
    public Image bookIconImage;

    [Header("Book Detail Value Texts")]
    public TextMeshProUGUI editionValueText;
    public TextMeshProUGUI conditionValueText;
    public TextMeshProUGUI rarityValueText;
    public TextMeshProUGUI magicLevelValueText;
    public TextMeshProUGUI ageValueText;
    public TextMeshProUGUI curseValueText;

    [Header("Economy Texts")]
    public TextMeshProUGUI boughtForText;
    public TextMeshProUGUI valueText;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
            closeButton.onClick.AddListener(Hide);
        }

        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    public void Show(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Book popup could not open because item is null.");
            return;
        }

        SetBookInfo(item);

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            popupPanel.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning("Popup Panel is not assigned.");
        }
    }

    public void Hide()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    private void SetBookInfo(ItemData item)
    {
        if (bookTitleText != null)
            bookTitleText.text = item.itemName.ToUpper();

        if (bookIconImage != null)
        {
            bookIconImage.sprite = item.itemIcon;
            bookIconImage.gameObject.SetActive(item.itemIcon != null);
        }

        if (editionValueText != null)
            editionValueText.text = item.edition.ToUpper();

        if (conditionValueText != null)
            conditionValueText.text = item.conditionString.ToUpper();

        if (rarityValueText != null)
            rarityValueText.text = item.rarity.ToString().ToUpper();

        if (magicLevelValueText != null)
            magicLevelValueText.text = item.magicLevel.ToUpper();

        if (ageValueText != null)
            ageValueText.text = item.age.ToUpper();

        if (curseValueText != null)
            curseValueText.text = item.curse.ToUpper();

        if (valueText != null)
            valueText.text = $"{item.basePrice:0}";

        if (boughtForText != null)
        {
            if (TryGetBoughtForPrice(item, out float purchasePrice))
                boughtForText.text = $"Bought for: {purchasePrice:0}";
            else
                boughtForText.text = "Bought for: -";
        }
    }

    private bool TryGetBoughtForPrice(ItemData item, out float price)
    {
        TradingManager tradingManager = FindObjectOfType<TradingManager>();

        if (tradingManager != null &&
            tradingManager.TryGetPurchasePrice(item, out price))
        {
            return true;
        }

        if (TradingManager.TryGetSavedPurchasePrice(item, out price))
        {
            return true;
        }

        price = 0f;
        return false;
    }
}