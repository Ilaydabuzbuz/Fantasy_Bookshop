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

    [Header("UI Colors")]
    public Color badColor = Color.red;
    public Color neutralColor = Color.yellow;
    public Color goodColor = Color.green;

    [Header("Race Bars")]
    public List<AffinityBar> affinityBars = new List<AffinityBar>();

    private void OnEnable()
    {
        ReputationManager.OnReputationChanged += UpdateAllBars;

        ReputationManager.ForceReloadFromPlayerPrefs();
        UpdateAllBars();
    }

    private void Start()
    {
        UpdateAllBars();
    }

    private void OnDisable()
    {
        ReputationManager.OnReputationChanged -= UpdateAllBars;
    }

    public void UpdateAllBars()
    {
        foreach (AffinityBar item in affinityBars)
        {
            if (item == null)
                continue;

            float reputation = ReputationManager.GetReputationValue(item.race);

            Debug.Log($"[Profile Reputation UI] {item.race} için okunan deðer: {reputation}");

            if (item.raceNameText != null)
                item.raceNameText.text = $"{item.race}: %{reputation:0}";

            if (item.affinitySlider != null)
            {
                item.affinitySlider.minValue = 0f;
                item.affinitySlider.maxValue = 100f;
                item.affinitySlider.value = reputation;
            }

            if (item.fillImage != null)
            {
                float t = reputation / 100f;

                if (t < 0.5f)
                    item.fillImage.color = Color.Lerp(badColor, neutralColor, t * 2f);
                else
                    item.fillImage.color = Color.Lerp(neutralColor, goodColor, (t - 0.5f) * 2f);
            }
        }
    }
}