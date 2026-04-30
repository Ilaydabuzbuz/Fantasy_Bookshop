using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TradingManager : MonoBehaviour
{
    [Header("UI References")]
    public AnimatedCounter goldCounter;
    public TextMeshProUGUI bookTitleText;
    public TextMeshProUGUI bookStatsText;
    public Image bookIconImage;

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

        if (offerInputField != null)
            offerInputField.text = Mathf.RoundToInt(item.basePrice * 0.6f).ToString();

        if (bookTitleText != null)
            bookTitleText.text = item.itemName;

        if (bookStatsText != null)
            bookStatsText.text = $"{item.basePrice:0}";

        if (bookIconImage != null)
        {
            bookIconImage.sprite = item.itemIcon;
            bookIconImage.gameObject.SetActive(true);
        }

        ShowDialogue("I brought this book. What is your offer?");
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

        float currentOffer = GetCurrentOffer();
        currentOffer -= offerIncrement;

        if (currentOffer < 0)
            currentOffer = 0;

        offerInputField.text = currentOffer.ToString("0");
    }

    public void SubmitOffer()
    {
        if (currentCustomer == null || currentCustomer.itemToSell == null || waitingForNextCustomer)
            return;

        float offer = GetCurrentOffer();
        ItemData item = currentCustomer.itemToSell;

        if (offer > playerGold)
        {
            ShowDialogue("You don't have enough gold.");
            return;
        }

        float minimumAcceptPrice = item.basePrice * currentCustomer.greedMultiplier * 0.75f;

        if (offer >= minimumAcceptPrice)
        {
            playerGold -= offer;
            purchasedItems.Add(item);

            if (goldCounter != null)
                goldCounter.UpdateCounter(playerGold);

            ShowDialogue($"Deal! You bought {item.itemName} for {offer:0} G.");
            FinishCustomer();
        }
        else
        {
            ShowDialogue("No way. That offer is too low.");
        }
    }

    public void RejectCustomer()
    {
        if (currentCustomer == null || waitingForNextCustomer)
            return;

        ShowDialogue("Maybe another time.");
        FinishCustomer();
    }

    private float GetCurrentOffer()
    {
        if (offerInputField == null)
            return 0;

        if (float.TryParse(offerInputField.text, out float value))
            return value;

        return 0;
    }

    private void FinishCustomer()
    {
        waitingForNextCustomer = true;
        SetButtonsInteractable(false);

        currentCustomer = null;

        Invoke(nameof(ClearUI), 2.5f);

        if (customerSpawner != null)
            customerSpawner.Invoke("SpawnNextCustomer", 3f);
    }

    private void ShowDialogue(string text)
    {
        if (dialogueGroup != null)
            dialogueGroup.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = text;
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
        if (bookTitleText != null)
            bookTitleText.text = "Book Title";

        if (bookStatsText != null)
            bookStatsText.text = "Estimated Value:";

        if (bookIconImage != null)
            bookIconImage.gameObject.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        if (dialogueGroup != null)
            dialogueGroup.SetActive(false);

        if (offerInputField != null)
            offerInputField.text = "";
    }
}