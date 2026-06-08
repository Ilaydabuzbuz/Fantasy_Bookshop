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

    private class DeferredCustomerData
    {
        public GameObject prefab;
        public CustomerRace race;
        public ItemData item;
        public float desiredPrice;
        public CustomerIntent intent;
        public float competence;
        public float greed;
        public float patience;
        public bool isCollector;
        public float greedMultiplier;
    }

    public List<CustomerGroup> customerGroups;
    public Transform spawnPoint;

    private readonly Queue<DeferredCustomerData> deferredCustomers = new Queue<DeferredCustomerData>();

    private GameObject currentCustomerObject;
    private GameObject currentCustomerPrefab;

    public bool HasDeferredCustomers => deferredCustomers.Count > 0;

    public void ClearCurrentCustomer()
    {
        if (currentCustomerObject != null)
        {
            Destroy(currentCustomerObject);
            currentCustomerObject = null;
            currentCustomerPrefab = null;
        }
    }

    public void ClearDeferredCustomers()
    {
        deferredCustomers.Clear();
    }

    public bool QueueCurrentCustomerForLater(CustomerAI customer)
    {
        if (customer == null || currentCustomerObject == null || currentCustomerPrefab == null)
        {
            Debug.LogWarning("[CustomerSpawner] Customer could not be deferred because its data is incomplete.");
            return false;
        }

        deferredCustomers.Enqueue(new DeferredCustomerData
        {
            prefab = currentCustomerPrefab,
            race = customer.customerRace,
            item = customer.itemToSell,
            desiredPrice = customer.customerDesiredPrice,
            intent = customer.intent,
            competence = customer.competence,
            greed = customer.greed,
            patience = customer.patience,
            isCollector = customer.isCollector,
            greedMultiplier = customer.greedMultiplier
        });

        Debug.Log($"[CustomerSpawner] {customer.customerRace} customer was moved to the end of today's queue.");
        return true;
    }

    public void SpawnNextCustomer()
    {
        ClearCurrentCustomer();

        if (customerGroups == null || customerGroups.Count == 0)
        {
            Debug.LogWarning("[CustomerSpawner] Customer group list is empty.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("[CustomerSpawner] Spawn Point is not assigned.");
            return;
        }

        CustomerGroup randomGroup = customerGroups[Random.Range(0, customerGroups.Count)];

        if (randomGroup.prefabs == null || randomGroup.prefabs.Length == 0)
        {
            Debug.LogWarning($"[CustomerSpawner] No prefab is assigned for {randomGroup.race}.");
            return;
        }

        currentCustomerPrefab = randomGroup.prefabs[Random.Range(0, randomGroup.prefabs.Length)];
        currentCustomerObject = Instantiate(currentCustomerPrefab, spawnPoint.position, spawnPoint.rotation);

        CustomerAI ai = currentCustomerObject.GetComponent<CustomerAI>();
        TradingManager tm = Object.FindFirstObjectByType<TradingManager>();

        if (ai == null)
        {
            Debug.LogError("[CustomerSpawner] Spawned customer prefab does not contain CustomerAI.");
            return;
        }

        if (tm == null)
        {
            Debug.LogError("[CustomerSpawner] TradingManager could not be found.");
            return;
        }

        ai.customerRace = randomGroup.race;

        if (tm.purchasedItems.Count == 0)
            ai.intent = CustomerIntent.SellToPlayer;
        else
            ai.intent = Random.value > 0.5f ? CustomerIntent.SellToPlayer : CustomerIntent.BuyFromPlayer;

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

            if (ai.itemToSell != null)
                ai.customerDesiredPrice = ai.itemToSell.basePrice * Random.Range(0.5f, 1.5f);
        }

        float currentRep = ReputationManager.GetReputationValue(ai.customerRace);
        float reputationModifier = (50f - currentRep) * 0.01f;

        ai.greedMultiplier = Mathf.Clamp(
            Random.Range(0.8f, 2.0f) + reputationModifier,
            0.5f,
            2.5f
        );

        float basePatience = Random.Range(40f, 100f);
        ai.patience = PlayerSkillEffects.ApplyCharismaToPatience(basePatience);
        ai.competence = Random.Range(10f, 100f);
        ai.greed = Mathf.Clamp(ai.greedMultiplier * 40f, 10f, 100f);
        ai.isCollector = Random.value < 0.2f;

        if (ai.isCollector && ai.intent == CustomerIntent.BuyFromPlayer)
            ai.customerDesiredPrice *= 1.3f;

        Debug.Log($"[CustomerSpawner] Spawned {ai.customerRace}. Base patience: {basePatience:0}, Final patience: {ai.patience:0}, Charisma Level: {PlayerSkillSession.CharismaLevel}");

        tm.SetupNewCustomer(ai, false);
    }

    public void SpawnDeferredCustomer()
    {
        ClearCurrentCustomer();

        if (deferredCustomers.Count == 0)
        {
            Debug.LogWarning("[CustomerSpawner] There is no deferred customer to spawn.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("[CustomerSpawner] Spawn Point is not assigned.");
            return;
        }

        DeferredCustomerData data = deferredCustomers.Dequeue();

        if (data.prefab == null)
        {
            Debug.LogError("[CustomerSpawner] Deferred customer prefab is missing.");
            return;
        }

        currentCustomerPrefab = data.prefab;
        currentCustomerObject = Instantiate(data.prefab, spawnPoint.position, spawnPoint.rotation);

        CustomerAI ai = currentCustomerObject.GetComponent<CustomerAI>();
        TradingManager tm = Object.FindFirstObjectByType<TradingManager>();

        if (ai == null || tm == null)
        {
            Debug.LogError("[CustomerSpawner] Deferred customer could not be restored.");
            return;
        }

        ai.customerRace = data.race;
        ai.itemToSell = data.item;
        ai.customerDesiredPrice = data.desiredPrice;
        ai.intent = data.intent;
        ai.competence = data.competence;
        ai.greed = data.greed;
        ai.patience = data.patience;
        ai.isCollector = data.isCollector;
        ai.greedMultiplier = data.greedMultiplier;

        bool requestedBookStillAvailable =
            ai.intent != CustomerIntent.BuyFromPlayer ||
            (ai.itemToSell != null && tm.purchasedItems.Contains(ai.itemToSell));

        tm.SetupNewCustomer(ai, true, requestedBookStillAvailable);
    }

    private ItemData GetRandomBookFromGroup(CustomerGroup group)
    {
        if (group.raceSpecificBooks == null || group.raceSpecificBooks.Count == 0)
        {
            Debug.LogWarning($"[CustomerSpawner] Book list for {group.race} is empty.");
            return null;
        }

        return group.raceSpecificBooks[Random.Range(0, group.raceSpecificBooks.Count)];
    }
}
