// CustomerSpawner.cs
using UnityEngine;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    [System.Serializable]
    public class CustomerGroup
    {
        public CustomerRace race;
        public GameObject[] prefabs;
        public List<ItemData> raceSpecificBooks;
    }

    public List<CustomerGroup> customerGroups;
    public Transform spawnPoint;
    private GameObject currentCustomerObject;

    public void ClearCurrentCustomer()
    {
        if (currentCustomerObject != null)
        {
            Destroy(currentCustomerObject);
            currentCustomerObject = null;
        }
    }

    public void SpawnNextCustomer()
    {
        if (currentCustomerObject != null)
            Destroy(currentCustomerObject);

        if (customerGroups == null || customerGroups.Count == 0) return;

        CustomerGroup randomGroup = customerGroups[Random.Range(0, customerGroups.Count)];
        GameObject randomPrefab = randomGroup.prefabs[Random.Range(0, randomGroup.prefabs.Length)];
        currentCustomerObject = Instantiate(randomPrefab, spawnPoint.position, spawnPoint.rotation);

        CustomerAI ai = currentCustomerObject.GetComponent<CustomerAI>();

        TradingManager tm = null;
        foreach (TradingManager t in Resources.FindObjectsOfTypeAll<TradingManager>())
        { tm = t; break; }

        if (tm == null)
        {
            Debug.LogError("TradingManager bulunamadý!");
            return;
        }

        if (ai != null)
        {
            if (tm.purchasedItems.Count == 0)
                ai.intent = CustomerIntent.SellToPlayer;
            else
                ai.intent = (Random.value > 0.5f) ? CustomerIntent.SellToPlayer : CustomerIntent.BuyFromPlayer;

            if (ai.intent == CustomerIntent.SellToPlayer)
            {
                ai.itemToSell = GetRandomBookFromGroup(randomGroup);
                if (ai.itemToSell != null)
                    ai.customerDesiredPrice = ai.itemToSell.basePrice * Random.Range(0.5f, 1.8f);
            }
            else
            {
                int randomIndex = Random.Range(0, tm.purchasedItems.Count);
                ai.itemToSell = tm.purchasedItems[randomIndex];
                ai.customerDesiredPrice = ai.itemToSell.basePrice * Random.Range(0.5f, 1.5f);
            }

            if (ReputationManager.Instance != null)
            {
                float currentRep = ReputationManager.Instance.GetReputation(ai.customerRace);
                float reputationModifier = (50f - currentRep) * 0.01f;
                ai.greedMultiplier = Mathf.Clamp(Random.Range(0.8f, 2.0f) + reputationModifier, 0.5f, 2.5f);
            }
            else
            {
                ai.greedMultiplier = Random.Range(0.8f, 2.0f);
            }

            ai.patience = Random.Range(40f, 100f);
            ai.competence = Random.Range(10f, 100f);
            ai.greed = Mathf.Clamp(ai.greedMultiplier * 40f, 10f, 100f);
            ai.isCollector = Random.value < 0.2f;

            if (ai.isCollector && ai.intent == CustomerIntent.BuyFromPlayer)
                ai.customerDesiredPrice *= 1.3f;

            tm.SetupNewCustomer(ai);
        }
    }

    private ItemData GetRandomBookFromGroup(CustomerGroup group)
    {
        if (group.raceSpecificBooks == null || group.raceSpecificBooks.Count == 0) return null;
        return group.raceSpecificBooks[Random.Range(0, group.raceSpecificBooks.Count)];
    }
}