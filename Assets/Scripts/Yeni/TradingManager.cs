using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TradingManager : MonoBehaviour
{
    [Header("UI References - Core")]
    public AnimatedCounter goldCounter;
    public TextMeshProUGUI bookTitleText;
    public TextMeshProUGUI bookStatsText;
    public Image bookIconImage;

    [Header("UI References - Book Details")]
    public TextMeshProUGUI editionText;
    public TextMeshProUGUI conditionText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI magicLevelText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI curseText;

    [Header("UI References - Dialogue")]
    public GameObject dialogueGroup;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField offerInputField;

    [Header("Buttons")]
    public Button offerButton;
    public Button increaseButton;
    public Button decreaseButton;
    public Button refuseButton;

    [Header("Economy Settings")]
    public float playerGold = 1000f;
    public float offerIncrement = 10f;
    public List<ItemData> purchasedItems = new List<ItemData>();

    [Header("System References")]
    public CustomerSpawner customerSpawner;

    private CustomerAI currentCustomer;
    private bool waitingForNextCustomer = false;

    private void Start()
    {
        if (goldCounter != null)
            goldCounter.SetValueInstant(playerGold);

        ClearUI();

        if (offerButton != null) offerButton.onClick.AddListener(SubmitOffer);
        if (increaseButton != null) increaseButton.onClick.AddListener(IncreaseOffer);
        if (decreaseButton != null) decreaseButton.onClick.AddListener(DecreaseOffer);
        if (refuseButton != null) refuseButton.onClick.AddListener(RejectCustomer);

        if (customerSpawner != null)
            customerSpawner.Invoke("SpawnNextCustomer", 1f);
    }

    public void SetupNewCustomer(CustomerAI newCustomer)
    {
        currentCustomer = newCustomer;
        waitingForNextCustomer = false;

        SetButtonsInteractable(true);

        if (dialogueGroup != null)
            dialogueGroup.SetActive(true);

        if (newCustomer.itemToSell == null)
        {
            ShowDialogue("This customer has no book.");
            return;
        }

        ItemData item = newCustomer.itemToSell;

        // --- SOL TARAF (Temel Deðerler) ---
        if (offerInputField != null)
            offerInputField.text = Mathf.RoundToInt(item.basePrice * 0.6f).ToString();

        if (bookTitleText != null) bookTitleText.text = item.itemName;
        if (bookStatsText != null) bookStatsText.text = $"{item.basePrice:0}";

        if (bookIconImage != null)
        {
            bookIconImage.sprite = item.itemIcon;
            bookIconImage.gameObject.SetActive(true);
        }

        // --- SAÐ TARAF (Fantastik Kitap Detaylarý) ---
        if (editionText != null) editionText.text = item.edition;
        if (conditionText != null) conditionText.text = item.conditionString;
        if (rarityText != null) rarityText.text = item.rarity.ToString();
        if (magicLevelText != null) magicLevelText.text = item.magicLevel;
        if (ageText != null) ageText.text = item.age;
        if (curseText != null) curseText.text = item.curse;

        // --- YAPAY ZEKA BAÐLANTISI ---
        currentCustomer.OnDealAccepted += HandleDealAccepted;
        currentCustomer.OnDealRejected += HandleDealRejected;
        currentCustomer.OnDialogueGenerated += HandleCustomerDialogue;
    }

    public void IncreaseOffer()
    {
        if (offerInputField == null || waitingForNextCustomer) return;
        float currentOffer = GetCurrentOffer();
        currentOffer += offerIncrement;
        offerInputField.text = currentOffer.ToString("0");
    }

    public void DecreaseOffer()
    {
        if (offerInputField == null || waitingForNextCustomer) return;
        float currentOffer = GetCurrentOffer() - offerIncrement;
        if (currentOffer < 0) currentOffer = 0;
        offerInputField.text = currentOffer.ToString("0");
    }

    public void SubmitOffer()
    {
        if (currentCustomer == null || currentCustomer.itemToSell == null || waitingForNextCustomer)
            return;

        float offer = GetCurrentOffer();

        if (offer > playerGold)
        {
            ShowDialogue("You don't have enough gold.");
            return;
        }

        currentCustomer.ReceivePlayerOffer(offer);
    }

    public void RejectCustomer()
    {
        if (currentCustomer == null || waitingForNextCustomer) return;

        if (ReputationManager.Instance != null)
            ReputationManager.Instance.ModifyReputation(currentCustomer.customerRace, -3f);

        ShowDialogue("Maybe another time.");
        FinishCustomer();
    }

    private float GetCurrentOffer()
    {
        if (offerInputField == null) return 0;
        if (float.TryParse(offerInputField.text, out float value)) return value;
        return 0;
    }

    // --- YAPAY ZEKADAN GELEN YANITLAR ---
    private void HandleCustomerDialogue(string text) => ShowDialogue(text);

    private void HandleDealAccepted(ItemData item, float finalPrice, string dialogue)
    {
        playerGold -= finalPrice;
        purchasedItems.Add(item);

        if (goldCounter != null) goldCounter.UpdateCounter(playerGold);

        if (currentCustomer != null && ReputationManager.Instance != null)
            ReputationManager.Instance.ModifyReputation(currentCustomer.customerRace, 8f);

        ShowDialogue(dialogue);
        FinishCustomer();
    }

    private void HandleDealRejected(string dialogue)
    {
        if (currentCustomer != null && ReputationManager.Instance != null)
            ReputationManager.Instance.ModifyReputation(currentCustomer.customerRace, -5f);

        ShowDialogue(dialogue);
        FinishCustomer();
    }

    private void FinishCustomer()
    {
        waitingForNextCustomer = true;
        SetButtonsInteractable(false);

        if (currentCustomer != null)
        {
            currentCustomer.OnDealAccepted -= HandleDealAccepted;
            currentCustomer.OnDealRejected -= HandleDealRejected;
            currentCustomer.OnDialogueGenerated -= HandleCustomerDialogue;
        }

        currentCustomer = null;
        Invoke(nameof(ClearUI), 2.5f);

        if (customerSpawner != null)
            customerSpawner.Invoke("SpawnNextCustomer", 3f);
    }

    private void ShowDialogue(string text)
    {
        if (dialogueGroup != null) dialogueGroup.SetActive(true);
        if (dialogueText != null) dialogueText.text = text;
    }

    private void SetButtonsInteractable(bool value)
    {
        if (offerButton != null) offerButton.interactable = value;
        if (increaseButton != null) increaseButton.interactable = value;
        if (decreaseButton != null) decreaseButton.interactable = value;
        if (refuseButton != null) refuseButton.interactable = value;
    }

    private void ClearUI()
    {
        if (bookTitleText != null) bookTitleText.text = "Book Title";
        if (bookStatsText != null) bookStatsText.text = "Value:";
        if (bookIconImage != null) bookIconImage.gameObject.SetActive(false);

        if (editionText != null) editionText.text = "Edition";
        if (conditionText != null) conditionText.text = "Condition";
        if (rarityText != null) rarityText.text = "Rarity";
        if (magicLevelText != null) magicLevelText.text = "Magic Level";
        if (ageText != null) ageText.text = "Age";
        if (curseText != null) curseText.text = "Curse";

        if (dialogueText != null) dialogueText.text = "";
        if (dialogueGroup != null) dialogueGroup.SetActive(false);
        if (offerInputField != null) offerInputField.text = "";
    }
}