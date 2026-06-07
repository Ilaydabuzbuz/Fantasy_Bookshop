using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image bookIcon;
    public TextMeshProUGUI priceText;
    public GameObject emptyOverlay;

    [Header("Click")]
    public Button slotButton;

    private ItemData currentItem;
    private InventoryManager inventoryManager;

    private void Awake()
    {
        SetupButton();
        DisableChildRaycasts();
    }

    private void OnEnable()
    {
        SetupButton();
        DisableChildRaycasts();
    }

    private void SetupButton()
    {
        if (slotButton == null)
            slotButton = GetComponent<Button>();

        if (slotButton != null)
        {
            slotButton.onClick.RemoveListener(OnSlotClicked);
            slotButton.onClick.AddListener(OnSlotClicked);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} üzerinde Button component yok.");
        }
    }

    private void DisableChildRaycasts()
    {
        if (bookIcon != null)
            bookIcon.raycastTarget = false;

        if (priceText != null)
            priceText.raycastTarget = false;
    }

    public void SetItem(ItemData item, InventoryManager manager)
    {
        currentItem = item;
        inventoryManager = manager;

        SetupButton();
        DisableChildRaycasts();

        if (bookIcon != null)
        {
            bookIcon.sprite = item.itemIcon;
            bookIcon.gameObject.SetActive(true);
        }

        if (priceText != null)
            priceText.text = $"{item.basePrice:0}";

        if (emptyOverlay != null)
            emptyOverlay.SetActive(false);

        if (slotButton != null)
            slotButton.interactable = true;
    }

    public void ClearSlot()
    {
        currentItem = null;
        inventoryManager = null;

        if (bookIcon != null)
        {
            bookIcon.sprite = null;
            bookIcon.gameObject.SetActive(false);
        }

        if (priceText != null)
            priceText.text = "";

        if (emptyOverlay != null)
            emptyOverlay.SetActive(true);

        if (slotButton != null)
            slotButton.interactable = false;
    }

    private void OnSlotClicked()
    {
        Debug.Log($"{gameObject.name} clicked.");

        if (currentItem == null)
        {
            Debug.LogWarning($"{gameObject.name} týklandý ama currentItem null.");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogWarning($"{gameObject.name} týklandý ama inventoryManager null.");
            return;
        }

        inventoryManager.OpenBookPopup(currentItem);
    }
}