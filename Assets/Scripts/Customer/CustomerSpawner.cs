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

    [Header("Customer Groups")]
    public List<CustomerGroup> customerGroups;

    [Header("Spawn Settings")]
    public Transform spawnPoint;

    [Header("Competence Price Settings")]

    [Tooltip(
        "Competence deðeri en düþük olduðunda gerçek fiyattan oluþabilecek " +
        "maksimum sapma. 0.50 deðeri yüzde 50 anlamýna gelir."
    )]
    [Range(0f, 1f)]
    public float lowCompetenceMaxDeviation = 0.50f;

    [Tooltip(
        "Competence deðeri 100 olduðunda kalacak maksimum doðal sapma. " +
        "0.05 deðeri yüzde 5 anlamýna gelir."
    )]
    [Range(0f, 1f)]
    public float highCompetenceMaxDeviation = 0.05f;

    [Tooltip(
        "Hesaplanan maksimum sapmanýn ne kadarýnýn minimum olarak " +
        "uygulanacaðýný belirler. 0.60 deðeri yüzde 60 anlamýna gelir."
    )]
    [Range(0f, 1f)]
    public float minimumDeviationRatio = 0.60f;

    [Header("Collector Settings")]

    [Tooltip(
        "Collector buyer müþterilerin ilk teklifine eklenecek bonus. " +
        "0.30 deðeri yüzde 30 anlamýna gelir."
    )]
    [Range(0f, 1f)]
    public float collectorBuyBonus = 0.30f;

    private readonly Queue<DeferredCustomerData> deferredCustomers =
        new Queue<DeferredCustomerData>();

    private GameObject currentCustomerObject;
    private GameObject currentCustomerPrefab;

    public bool HasDeferredCustomers => deferredCustomers.Count > 0;

    public void ClearCurrentCustomer()
    {
        if (currentCustomerObject == null)
            return;

        Destroy(currentCustomerObject);

        currentCustomerObject = null;
        currentCustomerPrefab = null;
    }

    public void ClearDeferredCustomers()
    {
        deferredCustomers.Clear();
    }

    public bool QueueCurrentCustomerForLater(CustomerAI customer)
    {
        if (customer == null ||
            currentCustomerObject == null ||
            currentCustomerPrefab == null)
        {
            Debug.LogWarning(
                "[CustomerSpawner] Customer could not be deferred " +
                "because its data is incomplete."
            );

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

        Debug.Log(
            $"[CustomerSpawner] {customer.customerRace} customer " +
            "was moved to the end of today's queue."
        );

        return true;
    }

    public void SpawnNextCustomer()
    {
        ClearCurrentCustomer();

        if (customerGroups == null || customerGroups.Count == 0)
        {
            Debug.LogWarning(
                "[CustomerSpawner] Customer group list is empty."
            );

            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "[CustomerSpawner] Spawn Point is not assigned."
            );

            return;
        }

        CustomerGroup randomGroup =
            customerGroups[Random.Range(0, customerGroups.Count)];

        if (randomGroup.prefabs == null ||
            randomGroup.prefabs.Length == 0)
        {
            Debug.LogWarning(
                $"[CustomerSpawner] No prefab is assigned for " +
                $"{randomGroup.race}."
            );

            return;
        }

        currentCustomerPrefab =
            randomGroup.prefabs[
                Random.Range(0, randomGroup.prefabs.Length)
            ];

        currentCustomerObject = Instantiate(
            currentCustomerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CustomerAI ai =
            currentCustomerObject.GetComponent<CustomerAI>();

        TradingManager tradingManager =
            Object.FindFirstObjectByType<TradingManager>();

        if (ai == null)
        {
            Debug.LogError(
                "[CustomerSpawner] Spawned customer prefab does not " +
                "contain CustomerAI."
            );

            ClearCurrentCustomer();
            return;
        }

        if (tradingManager == null)
        {
            Debug.LogError(
                "[CustomerSpawner] TradingManager could not be found."
            );

            ClearCurrentCustomer();
            return;
        }

        ai.customerRace = randomGroup.race;

        DetermineCustomerIntent(ai, tradingManager);
        AssignCustomerBook(ai, randomGroup, tradingManager);

        if (ai.itemToSell == null)
        {
            Debug.LogWarning(
                $"[CustomerSpawner] No book could be assigned to " +
                $"{ai.customerRace} customer."
            );

            ClearCurrentCustomer();
            return;
        }

        GenerateCustomerTraits(ai);

        ai.customerDesiredPrice = CalculateInitialDesiredPrice(
            ai.itemToSell,
            ai.intent,
            ai.competence,
            ai.isCollector
        );

        LogSpawnedCustomer(ai);

        tradingManager.SetupNewCustomer(ai, false);
    }

    private void DetermineCustomerIntent(
        CustomerAI ai,
        TradingManager tradingManager)
    {
        if (tradingManager.purchasedItems == null ||
            tradingManager.purchasedItems.Count == 0)
        {
            ai.intent = CustomerIntent.SellToPlayer;
            return;
        }

        ai.intent = Random.value > 0.5f
            ? CustomerIntent.SellToPlayer
            : CustomerIntent.BuyFromPlayer;
    }

    private void AssignCustomerBook(
        CustomerAI ai,
        CustomerGroup group,
        TradingManager tradingManager)
    {
        if (ai.intent == CustomerIntent.SellToPlayer)
        {
            ai.itemToSell = GetRandomBookFromGroup(group);
            return;
        }

        int randomIndex =
            Random.Range(0, tradingManager.purchasedItems.Count);

        ai.itemToSell =
            tradingManager.purchasedItems[randomIndex];
    }

    private void GenerateCustomerTraits(CustomerAI ai)
    {
        
        ai.competence = Random.Range(10f, 100f);

        
        ai.isCollector = Random.value < 0.20f;

        
        float currentReputation =
            ReputationManager.GetReputationValue(ai.customerRace);

        float reputationModifier =
            (50f - currentReputation) * 0.01f;

        ai.greedMultiplier = Mathf.Clamp(
            Random.Range(0.8f, 2.0f) + reputationModifier,
            0.5f,
            2.5f
        );

        ai.greed = Mathf.Clamp(
            ai.greedMultiplier * 40f,
            10f,
            100f
        );

        
        float basePatience = Random.Range(40f, 100f);

        ai.patience =
            PlayerSkillEffects.ApplyCharismaToPatience(basePatience);
    }

    private float CalculateInitialDesiredPrice(
        ItemData item,
        CustomerIntent intent,
        float competence,
        bool isCollector)
    {
        if (item == null)
            return 0f;

        float realPrice = Mathf.Max(1f, item.basePrice);

        
        float normalizedCompetence =
            Mathf.Clamp01(competence / 100f);

        
        float maximumDeviation = Mathf.Lerp(
            lowCompetenceMaxDeviation,
            highCompetenceMaxDeviation,
            normalizedCompetence
        );

        maximumDeviation = Mathf.Clamp01(maximumDeviation);

        
        float safeMinimumRatio =
            Mathf.Clamp01(minimumDeviationRatio);

        float minimumDeviation =
            maximumDeviation * safeMinimumRatio;

        float selectedDeviation = Random.Range(
            minimumDeviation,
            maximumDeviation
        );

        float priceMultiplier;

        if (intent == CustomerIntent.SellToPlayer)
        {
            
            priceMultiplier = 1f - selectedDeviation;
        }
        else
        {
            
            priceMultiplier = 1f + selectedDeviation;
        }

        float calculatedPrice =
            realPrice * priceMultiplier;

      
        if (isCollector &&
            intent == CustomerIntent.BuyFromPlayer)
        {
            calculatedPrice *= 1f + collectorBuyBonus;
        }

        return Mathf.Max(1f, calculatedPrice);
    }

    private void LogSpawnedCustomer(CustomerAI ai)
    {
        if (ai == null || ai.itemToSell == null)
            return;

        float realPrice = ai.itemToSell.basePrice;

        float difference =
            ai.customerDesiredPrice - realPrice;

        float differencePercent =
            realPrice > 0f
                ? difference / realPrice * 100f
                : 0f;

        Debug.Log(
            $"[CustomerSpawner] Spawned {ai.customerRace} | " +
            $"Intent: {ai.intent} | " +
            $"Book: {ai.itemToSell.itemName} | " +
            $"Real Price: {realPrice:0} | " +
            $"Initial Price: {ai.customerDesiredPrice:0} | " +
            $"Difference: {differencePercent:+0.0;-0.0;0}% | " +
            $"Competence: {ai.competence:0} | " +
            $"Greed: {ai.greed:0} | " +
            $"Greed Multiplier: {ai.greedMultiplier:0.00} | " +
            $"Patience: {ai.patience:0} | " +
            $"Collector: {ai.isCollector} | " +
            $"Charisma Level: {PlayerSkillSession.CharismaLevel}"
        );
    }

    public void SpawnDeferredCustomer()
    {
        ClearCurrentCustomer();

        if (deferredCustomers.Count == 0)
        {
            Debug.LogWarning(
                "[CustomerSpawner] There is no deferred customer to spawn."
            );

            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "[CustomerSpawner] Spawn Point is not assigned."
            );

            return;
        }

        DeferredCustomerData data = deferredCustomers.Dequeue();

        if (data.prefab == null)
        {
            Debug.LogError(
                "[CustomerSpawner] Deferred customer prefab is missing."
            );

            return;
        }

        currentCustomerPrefab = data.prefab;

        currentCustomerObject = Instantiate(
            data.prefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CustomerAI ai =
            currentCustomerObject.GetComponent<CustomerAI>();

        TradingManager tradingManager =
            Object.FindFirstObjectByType<TradingManager>();

        if (ai == null || tradingManager == null)
        {
            Debug.LogError(
                "[CustomerSpawner] Deferred customer could not be restored."
            );

            ClearCurrentCustomer();
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
            (
                ai.itemToSell != null &&
                tradingManager.purchasedItems.Contains(ai.itemToSell)
            );

        tradingManager.SetupNewCustomer(
            ai,
            true,
            requestedBookStillAvailable
        );
    }

    private ItemData GetRandomBookFromGroup(CustomerGroup group)
    {
        if (group == null ||
            group.raceSpecificBooks == null ||
            group.raceSpecificBooks.Count == 0)
        {
            string raceName =
                group != null
                    ? group.race.ToString()
                    : "Unknown";

            Debug.LogWarning(
                $"[CustomerSpawner] Book list for {raceName} is empty."
            );

            return null;
        }

        return group.raceSpecificBooks[
            Random.Range(0, group.raceSpecificBooks.Count)
        ];
    }
}