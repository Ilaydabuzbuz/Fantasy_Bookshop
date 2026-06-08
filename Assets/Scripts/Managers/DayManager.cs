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

    [HideInInspector] public float goldEarnedToday = 0f;
    [HideInInspector] public int booksSoldToday = 0;
    [HideInInspector] public int booksBoughtToday = 0;

    [HideInInspector] public Dictionary<CustomerRace, int> customersByRace = new Dictionary<CustomerRace, int>();
    [HideInInspector] public Dictionary<CustomerRace, float> reputationEarnedToday = new Dictionary<CustomerRace, float>();

    public static bool pendingRentPopup = false;
    public static float lastRentPaid = 0f;
    public static int lastRentDay = 0;

    private CustomerSpawner spawner;
    private TradingManager tradingManager;

    private bool hasStarted = false;
    private bool dayEnded = false;
    private int normalCustomersSpawnedToday = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        currentDay = PlayerPrefs.GetInt("CurrentDay", 1);

        if (!hasStarted)
        {
            hasStarted = true;
            StartNewDay();
        }
    }

    public void StartNewDay()
    {
        dayEnded = false;

        pendingRentPopup = false;
        lastRentPaid = 0f;
        lastRentDay = 0;

        spawner = FindDayScreenObject<CustomerSpawner>();
        tradingManager = FindDayScreenObject<TradingManager>();

        customersServedToday = 0;
        normalCustomersSpawnedToday = 0;
        totalCustomersToday = Random.Range(minCustomersPerDay, maxCustomersPerDay + 1);

        goldEarnedToday = 0f;
        booksSoldToday = 0;
        booksBoughtToday = 0;

        customersByRace.Clear();
        reputationEarnedToday.Clear();

        if (spawner != null)
            spawner.ClearDeferredCustomers();

        UpdateDayUI();

        Debug.Log($"Day {currentDay} started. {totalCustomersToday} customers will visit today.");

        SpawnNextScheduledCustomer();
    }

    private T FindDayScreenObject<T>() where T : MonoBehaviour
    {
        T[] objects = FindObjectsOfType<T>(true);

        foreach (T obj in objects)
        {
            if (obj != null && obj.gameObject.scene.name == "DayScreen")
                return obj;
        }

        return null;
    }

    private void UpdateDayUI()
    {
        TextMeshProUGUI[] texts = FindObjectsOfType<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI tmp in texts)
        {
            if (tmp != null && tmp.name == "Day" && tmp.gameObject.scene.name == "DayScreen")
                tmp.text = $"{currentDay}";
        }
    }

    public void OnCustomerLeft()
    {
        if (dayEnded)
            return;

        customersServedToday++;
        SpawnNextScheduledCustomer();
    }

    public void OnCustomerDeferred()
    {
        if (dayEnded)
            return;

        SpawnNextScheduledCustomer();
    }

    private void SpawnNextScheduledCustomer()
    {
        if (dayEnded)
            return;

        spawner = FindDayScreenObject<CustomerSpawner>();

        if (spawner == null)
        {
            Debug.LogError("CustomerSpawner could not be found.");
            return;
        }

        if (normalCustomersSpawnedToday < totalCustomersToday)
        {
            normalCustomersSpawnedToday++;
            StartCoroutine(SpawnNormalAfterDelay(0f));
            return;
        }

        if (spawner.HasDeferredCustomers)
        {
            StartCoroutine(SpawnDeferredAfterDelay(0f));
            return;
        }

        if (customersServedToday >= totalCustomersToday)
            EndDay();
    }

    private IEnumerator SpawnNormalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!dayEnded && spawner != null)
            spawner.SpawnNextCustomer();
    }

    private IEnumerator SpawnDeferredAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!dayEnded && spawner != null)
            spawner.SpawnDeferredCustomer();
    }

    public void RegisterCustomerServed(CustomerRace race)
    {
        if (!customersByRace.ContainsKey(race))
            customersByRace[race] = 0;

        customersByRace[race]++;
    }

    public void RegisterGoldEarned(float amount)
    {
        goldEarnedToday += amount;
    }

    public void RegisterBookSold()
    {
        booksSoldToday++;
    }

    public void RegisterBookBought()
    {
        booksBoughtToday++;
    }

    public void RegisterReputationEarned(CustomerRace race, float amount)
    {
        if (!reputationEarnedToday.ContainsKey(race))
            reputationEarnedToday[race] = 0f;

        reputationEarnedToday[race] += amount;
        ReputationManager.ModifyReputationValue(race, amount);
    }

    private void EndDay()
    {
        if (dayEnded)
            return;

        dayEnded = true;
        Debug.Log("All customers for the day have been completed.");

        spawner = FindDayScreenObject<CustomerSpawner>();

        if (spawner != null)
            spawner.ClearCurrentCustomer();

        SceneManager.LoadScene("EndDayReport", LoadSceneMode.Additive);
    }

    public void AdvanceToNextDay()
    {
        pendingRentPopup = false;
        lastRentPaid = 0f;
        lastRentDay = 0;

        // Rent is paid at the end of Day 3, Day 6, Day 9, etc.
        if (rentPeriodDays > 0 && currentDay % rentPeriodDays == 0)
            PayRent();

        currentDay++;

        PlayerPrefs.SetInt("CurrentDay", currentDay);
        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.Save();
    }

    private void PayRent()
    {
        tradingManager = FindDayScreenObject<TradingManager>();

        if (tradingManager != null)
        {
            tradingManager.playerGold -= rentAmount;
            tradingManager.SaveGameSessionState();

            MarkRentPopupNeeded(rentAmount, currentDay);

            if (tradingManager.goldCounter != null &&
                tradingManager.goldCounter.gameObject.activeInHierarchy)
            {
                tradingManager.goldCounter.UpdateCounter(tradingManager.playerGold);
            }

            Debug.Log($"Rent day: {rentAmount} gold was deducted.");

            if (tradingManager.playerGold < 0)
                Debug.LogError("You are bankrupt. GAME OVER.");
        }
        else
        {
            float savedGold = TradingManager.GetSavedGold() - rentAmount;
            TradingManager.SetSavedGold(savedGold);

            MarkRentPopupNeeded(rentAmount, currentDay);
            Debug.Log($"Rent day: {rentAmount} gold was deducted.");

            if (savedGold < 0)
                Debug.LogError("You are bankrupt. GAME OVER.");
        }
    }

    private void MarkRentPopupNeeded(float paidAmount, int day)
    {
        pendingRentPopup = true;
        lastRentPaid = paidAmount;
        lastRentDay = day;
    }

    public static void ClearRentPopup()
    {
        pendingRentPopup = false;
        lastRentPaid = 0f;
        lastRentDay = 0;
    }
}
