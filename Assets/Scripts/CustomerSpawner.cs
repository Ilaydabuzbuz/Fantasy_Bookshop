using UnityEngine;
using System;
using System.Collections.Generic;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject[] customerPrefabs;
    public Transform spawnPoint;
    public TradingManager tradingManager;
    public List<ItemData> availableItems;

    private string[] names = { "Alaric", "Elowen", "Valerius", "Thalric", "Morgath", "Sariel", "Brumir", "Kaelen" };

    public void SpawnNextCustomer()
    {
        if (customerPrefabs.Length == 0 || availableItems.Count == 0) return;

        GameObject prefab = customerPrefabs[UnityEngine.Random.Range(0, customerPrefabs.Length)];
        GameObject obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
        CustomerAI ai = obj.GetComponent<CustomerAI>();

        if (ai != null)
        {
            ai.customerName = names[UnityEngine.Random.Range(0, names.Length)];
            Array races = Enum.GetValues(typeof(CustomerRace));
            ai.customerRace = (CustomerRace)races.GetValue(UnityEngine.Random.Range(0, races.Length));
            ai.itemToSell = availableItems[UnityEngine.Random.Range(0, availableItems.Count)];
            ai.greedMultiplier = UnityEngine.Random.Range(0.9f, 1.9f);
            ai.patience = UnityEngine.Random.Range(80f, 120f);

            tradingManager.SetupNewCustomer(ai);
        }
    }
}