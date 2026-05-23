// CustomerAI.cs
using UnityEngine;

public enum CustomerIntent { SellToPlayer, BuyFromPlayer }

public class CustomerAI : MonoBehaviour
{
    public CustomerRace customerRace;
    public ItemData itemToSell;
    public float customerDesiredPrice;
    public CustomerIntent intent;

    [Header("Traits")]
    [Range(0f, 100f)] public float competence;
    [Range(0f, 100f)] public float greed;
    [Range(0f, 100f)] public float patience;
    public bool isCollector;

    public float greedMultiplier;

    public delegate void DealEvent(ItemData item, float finalPrice, string dialogue);
    public event DealEvent OnDealAccepted;

    public delegate void RejectEvent(string dialogue);
    public event RejectEvent OnDealRejected;

    public delegate void DialogueEvent(string text);
    public event DialogueEvent OnDialogueGenerated;

    public void ReceivePlayerOffer(float playerOffer)
    {
        float difference = Mathf.Abs(customerDesiredPrice - playerOffer);
        float percentDifference = difference / customerDesiredPrice;
        float tolerance = Random.Range(0.05f, 0.15f) / greedMultiplier;
        bool isAcceptable = false;

        if (intent == CustomerIntent.SellToPlayer)
        {
            if (playerOffer >= customerDesiredPrice * (1f - tolerance)) isAcceptable = true;
        }
        else
        {
            if (playerOffer <= customerDesiredPrice * (1f + tolerance)) isAcceptable = true;
        }

        if (isAcceptable)
        {
            string[] successLines = {
                "You know what? Close enough. It's a deal.",
                "Fair enough. We have an agreement.",
                "I won't argue over a few coins. Done.",
                "Alright, that works for me."
            };
            OnDealAccepted?.Invoke(itemToSell, playerOffer, successLines[Random.Range(0, successLines.Length)]);
            return;
        }

        float basePatienceDrop = Random.Range(15f, 25f) * greedMultiplier;
        if (percentDifference < 0.2f)
            basePatienceDrop *= 0.4f;
        else if (percentDifference > 0.6f)
            basePatienceDrop *= 1.2f;

        patience -= basePatienceDrop;

        if (patience <= 0)
        {
            OnDealRejected?.Invoke("We are too far apart on this. I'm leaving!");
            return;
        }

        float compromiseFactor = Mathf.Clamp(1.0f / greedMultiplier, 0.2f, 0.7f);
        float moveAmount = difference * compromiseFactor;

        if (intent == CustomerIntent.SellToPlayer)
        {
            customerDesiredPrice -= moveAmount;
            customerDesiredPrice = Mathf.Max(customerDesiredPrice, playerOffer + 1);
        }
        else
        {
            customerDesiredPrice += moveAmount;
            customerDesiredPrice = Mathf.Min(customerDesiredPrice, playerOffer - 1);
        }

        int roundedAsk = Mathf.RoundToInt(customerDesiredPrice);
        string[] haggleLines = {
            $"We are getting closer. How about {roundedAsk}?",
            $"I can't do that, but I can do {roundedAsk}.",
            $"Let's meet closer to the middle: {roundedAsk}.",
            $"Make it {roundedAsk} and we have a deal."
        };
        OnDialogueGenerated?.Invoke(haggleLines[Random.Range(0, haggleLines.Length)]);
    }
}