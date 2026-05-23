using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Day & Rent Settings")]
    public int currentDay = 1;
    public int rentPeriodDays = 3;
    public float rentAmount = 300f;

    [Header("Daily Traffic Configuration")]
    public int minCustomersPerDay = 3;
    public int maxCustomersPerDay = 6;
    [HideInInspector] public int totalCustomersToday;
    [HideInInspector] public int customersServedToday;

    [Header("UI References")]
    public TextMeshProUGUI dayText;
    public GameObject endOfDayPanel;

    [HideInInspector] public float goldEarnedToday = 0f;
    [HideInInspector] public int booksSoldToday = 0;
    [HideInInspector] public int booksBoughtToday = 0;
    [HideInInspector] public Dictionary<CustomerRace, int> customersByRace = new Dictionary<CustomerRace, int>();
    [HideInInspector] public Dictionary<CustomerRace, float> reputationEarnedToday = new Dictionary<CustomerRace, float>();

    private CustomerSpawner spawner;
    private TradingManager tradingManager;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        StartNewDay();
    }

    public void StartNewDay()
    {
        spawner = null;
        foreach (CustomerSpawner s in Resources.FindObjectsOfTypeAll<CustomerSpawner>())
        { spawner = s; break; }

        tradingManager = null;
        foreach (TradingManager t in Resources.FindObjectsOfTypeAll<TradingManager>())
        { tradingManager = t; break; }

        customersServedToday = 0;
        totalCustomersToday = Random.Range(minCustomersPerDay, maxCustomersPerDay + 1);

        goldEarnedToday = 0f;
        booksSoldToday = 0;
        booksBoughtToday = 0;
        customersByRace.Clear();
        reputationEarnedToday.Clear();

        if (endOfDayPanel != null) endOfDayPanel.SetActive(false);
        if (dayText != null) dayText.text = $"{currentDay}";

        Debug.Log($"Gün {currentDay} baþladý! Bugün dükkana {totalCustomersToday} müþteri uðrayacak.");

        if (spawner != null)
            StartCoroutine(SpawnAfterDelay(1.5f));
        else
            Debug.LogError("Spawner bulunamadý!");
    }

    public void OnCustomerLeft()
    {
        customersServedToday++;
        if (customersServedToday >= totalCustomersToday)
        {
            EndDay();
        }
        else
        {
            spawner = null;
            foreach (CustomerSpawner s in Resources.FindObjectsOfTypeAll<CustomerSpawner>())
            { spawner = s; break; }

            if (spawner != null)
                StartCoroutine(SpawnAfterDelay(2.5f));
            else
                Debug.LogError("Spawner bulunamadý!");
        }
    }

    private IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        spawner?.SpawnNextCustomer();
    }

    public void RegisterCustomerServed(CustomerRace race)
    {
        if (!customersByRace.ContainsKey(race))
            customersByRace[race] = 0;
        customersByRace[race]++;
    }

    public void RegisterGoldEarned(float amount) => goldEarnedToday += amount;
    public void RegisterBookSold() => booksSoldToday++;
    public void RegisterBookBought() => booksBoughtToday++;

    public void RegisterReputationEarned(CustomerRace race, float amount)
    {
        if (!reputationEarnedToday.ContainsKey(race))
            reputationEarnedToday[race] = 0f;
        reputationEarnedToday[race] += amount;
    }

    private void EndDay()
    {
        Debug.Log("Günün tüm müþterileri bitti!");

        Scene dayScene = SceneManager.GetSceneByName("DayScreen");
        if (dayScene.IsValid())
        {
            foreach (GameObject root in dayScene.GetRootGameObjects())
            {
                if (root.name == "DontDestroyOnLoad") continue;
                if (root.name == "EventSystem") continue;
                if (root.name == "CustomerTraitsManager") continue;
                if (root.name == "CustomerPoint") continue;
                if (root.name == "BookPoint") continue;
                root.SetActive(false);
            }
        }

        SceneManager.LoadScene("EndDayReport", LoadSceneMode.Additive);
    }

    public void AdvanceToNextDay()
    {
        currentDay++;
        if (currentDay % rentPeriodDays == 0)
            PayRent();
        StartNewDay();
    }

    private void PayRent()
    {
        tradingManager = null;
        foreach (TradingManager t in Resources.FindObjectsOfTypeAll<TradingManager>())
        { tradingManager = t; break; }

        if (tradingManager != null)
        {
            tradingManager.playerGold -= rentAmount;
            if (tradingManager.goldCounter != null)
                tradingManager.goldCounter.UpdateCounter(tradingManager.playerGold);
            Debug.Log($"Kira ödeme günü! {rentAmount} altýn kesildi.");
            if (tradingManager.playerGold < 0)
                Debug.LogError("Ýflas ettin! GAME OVER.");
        }
    }
}