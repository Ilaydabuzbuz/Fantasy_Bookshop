using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class TradingManager : MonoBehaviour
{
    [Header("UI References")]
    public AnimatedCounter goldCounter;
    public TextMeshProUGUI blackboardStats;
    public Image blackboardItemIcon;
    public GameObject dialogueGroup;
    public TypewriterEffect dialogueTypewriter;
    public TMP_InputField offerInputField;

    [Header("Economy Settings")]
    public float playerGold = 1000f;
    public float offerIncrement = 10f;
    public List<ItemData> purchasedItems = new List<ItemData>();

    [Header("System References")]
    public CustomerSpawner customerSpawner;
    private CustomerAI currentCustomer;

    private void Start()
    {
        if (goldCounter != null) goldCounter.SetValueInstant(playerGold);
        ClearUI();

        // Add this line to call the first customer 1 second after game starts
        if (customerSpawner != null)
        {
            customerSpawner.Invoke("SpawnNextCustomer", 1.0f);
        }
    }

    public void IncreaseOffer()
    {
        if (float.TryParse(offerInputField.text, out float val))
            offerInputField.text = (val + offerIncrement).ToString("0");
    }

    public void DecreaseOffer()
    {
        if (float.TryParse(offerInputField.text, out float val) && val >= offerIncrement)
            offerInputField.text = (val - offerIncrement).ToString("0");
    }

    public void SubmitOffer()
    {
        if (currentCustomer != null && float.TryParse(offerInputField.text, out float offer))
            currentCustomer.ReceivePlayerOffer(offer);
    }

    public void RejectCustomer()
    {
        if (currentCustomer != null)
        {
            HandleCustomerDialogue("Get out of my shop!");
            CleanUpAndCallNext();
        }
    }

    public void SetupNewCustomer(CustomerAI newCustomer)
    {
        currentCustomer = newCustomer;
        if (dialogueGroup != null) dialogueGroup.SetActive(true);
        offerInputField.text = (newCustomer.itemToSell.basePrice * 0.6f).ToString("0");

        blackboardStats.text = $"CUSTOMER: {newCustomer.customerName} ({newCustomer.customerRace})\n" +
                               $"RARITY: {newCustomer.itemToSell.rarity}\n" +
                               $"CONDITION: {newCustomer.itemToSell.conditionString}\n" +
                               $"EDITION: {newCustomer.itemToSell.edition}\n" +
                               $"ESTIMATED: {newCustomer.itemToSell.basePrice} G";

        if (blackboardItemIcon != null)
        {
            blackboardItemIcon.sprite = newCustomer.itemToSell.itemIcon;
            blackboardItemIcon.gameObject.SetActive(true);
        }

        currentCustomer.OnDealAccepted += HandleDealAccepted;
        currentCustomer.OnDealRejected += HandleDealRejected;
        currentCustomer.OnDialogueGenerated += HandleCustomerDialogue;
    }

    private void HandleCustomerDialogue(string t) => dialogueTypewriter?.ShowText(t);

    private void HandleDealAccepted(ItemData item, float p, string d)
    {
        playerGold -= p;
        purchasedItems.Add(item);
        goldCounter?.UpdateCounter(playerGold);
        HandleCustomerDialogue(d);
        CleanUpAndCallNext();
    }

    private void HandleDealRejected(string d)
    {
        HandleCustomerDialogue(d);
        CleanUpAndCallNext();
    }

    private void CleanUpAndCallNext()
    {
        if (currentCustomer != null)
        {
            currentCustomer.OnDealAccepted -= HandleDealAccepted;
            currentCustomer.OnDealRejected -= HandleDealRejected;
            currentCustomer.OnDialogueGenerated -= HandleCustomerDialogue;
        }
        currentCustomer = null;
        if (customerSpawner != null) customerSpawner.Invoke("SpawnNextCustomer", 3.0f);
        Invoke("ClearUI", 2.8f);
    }

    private void ClearUI()
    {
        if (blackboardStats != null) blackboardStats.text = "";
        if (blackboardItemIcon != null) blackboardItemIcon.gameObject.SetActive(false);
        if (dialogueGroup != null) dialogueGroup.SetActive(false);
    }
}