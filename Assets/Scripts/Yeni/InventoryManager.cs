using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    [Header("Grid Slots")]
    public List<InventorySlot> slots;

    [Header("Category Buttons")]
    public Button allBooksButton;
    public Button humanButton;
    public Button dwarfButton;
    public Button elfButton;
    public Button vampireButton;
    public Button wizardButton;

    [Header("Pagination")]
    public Button prevButton;
    public Button nextButton;
    public List<Button> pageButtons;

    [Header("Info")]
    public TextMeshProUGUI totalBooksText;

    private List<ItemData> filteredItems = new List<ItemData>();
    private int currentPage = 0;
    private int itemsPerPage = 10;
    private string currentFilter = "All";

    private void OnEnable()
    {
        RefreshInventory();
    }

    private void Start()
    {
        allBooksButton?.onClick.AddListener(() => SetFilter("All"));
        humanButton?.onClick.AddListener(() => SetFilter("Human"));
        dwarfButton?.onClick.AddListener(() => SetFilter("Dwarf"));
        elfButton?.onClick.AddListener(() => SetFilter("Elf"));
        vampireButton?.onClick.AddListener(() => SetFilter("Vampire"));
        wizardButton?.onClick.AddListener(() => SetFilter("Wizard"));

        prevButton?.onClick.AddListener(PrevPage);
        nextButton?.onClick.AddListener(NextPage);

        if (pageButtons != null)
        {
            for (int i = 0; i < pageButtons.Count; i++)
            {
                int pageIndex = i;
                pageButtons[i].onClick.AddListener(() => GoToPage(pageIndex));
            }
        }

        RefreshInventory();
    }

    public void SetFilter(string filter)
    {
        currentFilter = filter;
        currentPage = 0;
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        List<ItemData> sourceItems = GetInventoryItems();

        if (currentFilter == "All")
        {
            filteredItems = new List<ItemData>(sourceItems);
        }
        else
        {
            filteredItems = sourceItems
                .Where(item => item != null && item.race.ToString() == currentFilter)
                .ToList();
        }

        if (totalBooksText != null)
            totalBooksText.text = $"{filteredItems.Count}";

        UpdatePageButtons();
        ShowPage(currentPage);
    }

    private List<ItemData> GetInventoryItems()
    {
        TradingManager tm = FindObjectOfType<TradingManager>();

        if (tm != null)
            return new List<ItemData>(tm.purchasedItems);

        return TradingManager.GetSavedPurchasedItems();
    }

    private void ShowPage(int page)
    {
        if (slots == null)
            return;

        int startIndex = page * itemsPerPage;

        for (int i = 0; i < slots.Count; i++)
        {
            int itemIndex = startIndex + i;

            if (itemIndex < filteredItems.Count)
                slots[i].SetItem(filteredItems[itemIndex]);
            else
                slots[i].ClearSlot();
        }
    }

    private void UpdatePageButtons()
    {
        int totalPages = Mathf.CeilToInt((float)filteredItems.Count / itemsPerPage);
        totalPages = Mathf.Max(totalPages, 1);

        if (pageButtons != null)
        {
            for (int i = 0; i < pageButtons.Count; i++)
            {
                pageButtons[i].gameObject.SetActive(i < totalPages);
            }
        }

        UpdatePageButtonHighlight();
    }

    private void UpdatePageButtonHighlight()
    {
        if (pageButtons == null)
            return;

        for (int i = 0; i < pageButtons.Count; i++)
        {
            ColorBlock cb = pageButtons[i].colors;
            cb.normalColor = (i == currentPage) ? Color.yellow : Color.white;
            pageButtons[i].colors = cb;
        }
    }

    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowPage(currentPage);
            UpdatePageButtonHighlight();
        }
    }

    private void NextPage()
    {
        int totalPages = Mathf.CeilToInt((float)filteredItems.Count / itemsPerPage);
        totalPages = Mathf.Max(totalPages, 1);

        if (currentPage < totalPages - 1)
        {
            currentPage++;
            ShowPage(currentPage);
            UpdatePageButtonHighlight();
        }
    }

    private void GoToPage(int page)
    {
        int totalPages = Mathf.CeilToInt((float)filteredItems.Count / itemsPerPage);
        totalPages = Mathf.Max(totalPages, 1);

        currentPage = Mathf.Clamp(page, 0, totalPages - 1);

        ShowPage(currentPage);
        UpdatePageButtonHighlight();
    }
}