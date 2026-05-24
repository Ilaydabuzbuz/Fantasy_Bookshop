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
        if (dayText != null && DayManager.Instance != null)
            dayText.text = $"{DayManager.Instance.currentDay}";

        if (goldText != null)
        {
            TradingManager tm = null;
            foreach (TradingManager t in Resources.FindObjectsOfTypeAll<TradingManager>())
            { tm = t; break; }

            if (tm != null)
                goldText.text = $"{tm.playerGold:0}";
        }
    }
}