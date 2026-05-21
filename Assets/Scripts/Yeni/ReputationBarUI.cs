using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ReputationBarUI : MonoBehaviour
{
    [System.Serializable]
    public class AffinityBar
    {
        public CustomerRace race;
        public TextMeshProUGUI raceNameText;
        public Slider affinitySlider;
        public Image fillImage;
    }

    [Header("UI Ayarlarý")]
    public Color badColor = Color.red;
    public Color neutralColor = Color.yellow;
    public Color goodColor = Color.green;

    [Header("Irk Barlarý")]
    public List<AffinityBar> affinityBars;

    private void Start()
    {
        UpdateAllBars();
    }

    public void UpdateAllBars()
    {
        if (ReputationManager.Instance == null) return;

        foreach (var item in affinityBars)
        {
            float rep = ReputationManager.Instance.GetReputation(item.race);
            Debug.Log($"[Affinities] {item.race} ýrký için okunan deðer: {rep}");

            if (item.raceNameText != null) item.raceNameText.text = $"{item.race}: %{rep:0}";
            if (item.affinitySlider != null) item.affinitySlider.value = rep;

            if (item.fillImage != null)
            {
                float t = rep / 100f;

                Color barColor;
                if (t < 0.5f)
                {
                    barColor = Color.Lerp(badColor, neutralColor, t * 2f);
                }
                else
                {
                    barColor = Color.Lerp(neutralColor, goodColor, (t - 0.5f) * 2f);
                }
                item.fillImage.color = barColor;
            }
        }
    }
}