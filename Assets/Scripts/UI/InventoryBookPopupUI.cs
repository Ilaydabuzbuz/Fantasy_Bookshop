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

    [Header("Economy Value Texts")]
    public TextMeshProUGUI boughtForValueText;
    public TextMeshProUGUI valueText;

    private void Start()
    {
        Hide();

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    public void Show(ItemData item)
    {
        if (item == null)
            return;

        if (popupPanel != null)
            popupPanel.SetActive(true);

        SetBookInfo(item);
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

        if (boughtForValueText != null)
            boughtForValueText.text = $"{GetBoughtForPrice(item):0}";

        if (valueText != null)
        {
            bool hideValue = PlayerSkillEffects.ShouldHideBookValue();

            if (hideValue)
                valueText.text = "???";
            else
                valueText.text = $"{item.basePrice:0}";
        }
    }

    private float GetBoughtForPrice(ItemData item)
    {
        TradingManager tradingManager = FindObjectOfType<TradingManager>();

        if (tradingManager != null && tradingManager.TryGetPurchasePrice(item, out float priceFromCurrentManager))
            return priceFromCurrentManager;

        if (TradingManager.TryGetSavedPurchasePrice(item, out float priceFromSave))
            return priceFromSave;

        return item.basePrice;
    }
}