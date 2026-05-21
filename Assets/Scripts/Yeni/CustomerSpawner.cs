using UnityEngine;
using System;

[Serializable]
public class RaceSpawnGroup
{
    public CustomerRace race;
    public GameObject[] customerPrefabs;
    public GameObject[] bookPrefabs;
}

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform customerSpawnPoint;
    public Transform bookSpawnPoint;

    [Header("Manager")]
    public TradingManager tradingManager;

    [Header("Race Groups")]
    public RaceSpawnGroup[] raceGroups;

    private GameObject currentCustomerObject;
    private GameObject currentBookObject;

    private string[] names =
    {
        "Alaric", "Elowen", "Valerius", "Thalric",
        "Morgath", "Sariel", "Brumir", "Kaelen"
    };

    public void SpawnNextCustomer()
    {
        ClearCurrentObjects();

        if (raceGroups == null || raceGroups.Length == 0)
        {
            Debug.LogWarning("No race groups assigned.");
            return;
        }

        RaceSpawnGroup group = raceGroups[UnityEngine.Random.Range(0, raceGroups.Length)];

        if (group.customerPrefabs == null || group.customerPrefabs.Length == 0)
        {
            Debug.LogWarning($"No customer prefabs assigned for {group.race}");
            return;
        }

        if (group.bookPrefabs == null || group.bookPrefabs.Length == 0)
        {
            Debug.LogWarning($"No book prefabs assigned for {group.race}");
            return;
        }

        GameObject customerPrefab = group.customerPrefabs[UnityEngine.Random.Range(0, group.customerPrefabs.Length)];
        GameObject bookPrefab = group.bookPrefabs[UnityEngine.Random.Range(0, group.bookPrefabs.Length)];

        currentCustomerObject = Instantiate(customerPrefab, customerSpawnPoint.position, customerSpawnPoint.rotation);
        currentBookObject = Instantiate(bookPrefab, bookSpawnPoint.position, bookSpawnPoint.rotation);

        CustomerAI ai = currentCustomerObject.GetComponent<CustomerAI>();
        BookVisual bookVisual = currentBookObject.GetComponent<BookVisual>();

        if (ai == null)
        {
            Debug.LogError("Spawned customer does not have CustomerAI component.");
            return;
        }

        if (bookVisual == null || bookVisual.itemData == null)
        {
            Debug.LogError("Spawned book does not have BookVisual or ItemData assigned.");
            return;
        }

        ai.customerName = names[UnityEngine.Random.Range(0, names.Length)];
        ai.customerRace = group.race;
        ai.itemToSell = bookVisual.itemData;
        ai.greedMultiplier = UnityEngine.Random.Range(0.5f, 1.9f);
        ai.patience = UnityEngine.Random.Range(10f, 120f);

        if (ReputationManager.Instance != null)
        {
            float currentRep = ReputationManager.Instance.GetReputation(group.race);

            float reputationModifier = (50f - currentRep) * 0.01f;
            ai.greedMultiplier = Mathf.Clamp(UnityEngine.Random.Range(0.9f, 1.9f) + reputationModifier, 0.6f, 2.5f);
        }

        if (tradingManager != null)
            tradingManager.SetupNewCustomer(ai);
    }

    private void ClearCurrentObjects()
    {
        if (currentCustomerObject != null)
            Destroy(currentCustomerObject);

        if (currentBookObject != null)
            Destroy(currentBookObject);
    }
}