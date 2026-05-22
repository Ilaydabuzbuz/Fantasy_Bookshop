using UnityEngine;
using TMPro;

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

    private CustomerSpawner spawner;
    private TradingManager tradingManager;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        spawner = UnityEngine.Object.FindAnyObjectByType<CustomerSpawner>();
        tradingManager = UnityEngine.Object.FindAnyObjectByType<TradingManager>();
        StartNewDay();
    }

    public void StartNewDay()
    {
        customersServedToday = 0;
        totalCustomersToday = Random.Range(minCustomersPerDay, maxCustomersPerDay + 1);

        if (endOfDayPanel != null) endOfDayPanel.SetActive(false);
        if (dayText != null) dayText.text = $"Day: {currentDay}";

        Debug.Log($"Gün {currentDay} baþladý! Bugün dükkana {totalCustomersToday} müþteri uðrayacak.");

        if (spawner != null) spawner.Invoke("SpawnNextCustomer", 1.5f);
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
            if (spawner != null) spawner.Invoke("SpawnNextCustomer", 2.5f);
        }
    }

    private void EndDay()
    {
        Debug.Log("Günün tüm müþterileri bitti! Gün sonu raporu hazýrlanýyor...");
        if (endOfDayPanel != null) endOfDayPanel.SetActive(true);

    }

    public void AdvanceToNextDay()
    {
        currentDay++;

        if (currentDay % rentPeriodDays == 0)
        {
            PayRent();
        }

        StartNewDay();
    }

    private void PayRent()
    {
        if (tradingManager != null)
        {
            tradingManager.playerGold -= rentAmount;
            if (tradingManager.goldCounter != null)
                tradingManager.goldCounter.UpdateCounter(tradingManager.playerGold);

            Debug.Log($"Kira ödeme günü! {rentAmount} altýn kesildi.");

            if (tradingManager.playerGold < 0)
            {
                Debug.LogError("Ýflas ettin! GAME OVER.");
            }
        }
    }
}