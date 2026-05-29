using UnityEngine;
using TMPro;

public class SceneUIUpdater : MonoBehaviour
{
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI goldText;

    private void OnEnable()
    {
        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (dayText != null)
        {
            if (DayManager.Instance != null)
                dayText.text = $"{DayManager.Instance.currentDay}";
            else
                dayText.text = $"{PlayerPrefs.GetInt("CurrentDay", 1)}";
        }

        if (goldText != null)
        {
            TradingManager tm = FindFirstObjectByType<TradingManager>();

            if (tm != null)
                goldText.text = $"{tm.playerGold:0}";
            else
                goldText.text = $"{TradingManager.GetSavedGold():0}";
        }
    }
}