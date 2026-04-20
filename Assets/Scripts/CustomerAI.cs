using UnityEngine;
using System;

public class CustomerAI : MonoBehaviour
{
    public enum CustomerState { Entering, WaitingForOffer, Negotiating, Leaving }
    public CustomerState currentState;

    [Header("Identity")]
    public string customerName;
    public CustomerRace customerRace;

    [Header("Stats")]
    public float patience = 100f;
    [Range(0.5f, 2.5f)] public float greedMultiplier = 1.0f;

    [Header("Item to Sell")]
    public ItemData itemToSell;

    private float currentAskingPrice;
    private float absoluteMinimum;
    private bool hasGreeted = false;

    public event Action<CustomerAI> OnCustomerArrived;
    public event Action<ItemData, float, string> OnDealAccepted;
    public event Action<string> OnDealRejected;
    public event Action<string> OnDialogueGenerated;

    private void Start() => ChangeState(CustomerState.Entering);

    public void ChangeState(CustomerState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case CustomerState.Entering:
                InitializeNegotiation();
                ChangeState(CustomerState.WaitingForOffer);
                break;
            case CustomerState.WaitingForOffer:
                OnCustomerArrived?.Invoke(this);
                if (!hasGreeted)
                {
                    OnDialogueGenerated?.Invoke($"Hello! I'm {customerName}. I have this {itemToSell.itemName} for {currentAskingPrice.ToString("0")} gold.");
                    hasGreeted = true;
                }
                break;
            case CustomerState.Leaving:
                Destroy(gameObject, 1.5f);
                break;
        }
    }

    private void InitializeNegotiation()
    {
        if (itemToSell == null) return;
        currentAskingPrice = itemToSell.basePrice * greedMultiplier * itemToSell.condition;
        absoluteMinimum = itemToSell.basePrice * itemToSell.condition * (greedMultiplier * 0.75f);
    }

    public void ReceivePlayerOffer(float playerOffer)
    {
        if (playerOffer >= currentAskingPrice * 0.98f)
        {
            AcceptOffer(playerOffer);
            return;
        }

        float offerRatio = playerOffer / currentAskingPrice;
        patience -= (1.0f - offerRatio) * 40f;

        if (patience <= 0) RejectAndLeave();
        else
        {
            float flexibility = (100f - patience) / 100f;
            currentAskingPrice -= (currentAskingPrice - absoluteMinimum) * flexibility * 0.25f;
            OnDialogueGenerated?.Invoke(GetMoodMsg(offerRatio) + $" I can't go lower than {currentAskingPrice.ToString("0")}.");
        }
    }

    private string GetMoodMsg(float r)
    {
        if (r < 0.4f) return "Are you kidding me?";
        if (r < 0.7f) return "That's not enough.";
        return "Almost there, make it better.";
    }

    private void AcceptOffer(float p)
    {
        OnDealAccepted?.Invoke(itemToSell, p, "We have a deal!");
        ChangeState(CustomerState.Leaving);
    }

    private void RejectAndLeave()
    {
        OnDealRejected?.Invoke("I'm wasting my time here. Goodbye!");
        ChangeState(CustomerState.Leaving);
    }
}