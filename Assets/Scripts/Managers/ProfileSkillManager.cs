using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileSkillManager : MonoBehaviour
{
    [Header("Gold UI")]
    public TextMeshProUGUI goldText;

    [Header("Circle Colors")]
    public Color activeColor = Color.yellow;
    public Color inactiveColor = new Color(1f, 1f, 1f, 0.25f);

    [Header("Competence Skill")]
    public Image[] competenceCircles = new Image[6];
    public TextMeshProUGUI competenceCostText;
    public Button competenceUpgradeButton;

    [Header("Charisma Skill")]
    public Image[] charismaCircles = new Image[6];
    public TextMeshProUGUI charismaCostText;
    public Button charismaUpgradeButton;

    [Header("Insight Skill")]
    public Image[] insightCircles = new Image[6];
    public TextMeshProUGUI insightCostText;
    public Button insightUpgradeButton;

    private void OnEnable()
    {
        PlayerSkillSession.EnsureLoaded();
        RefreshUI();
    }

    private void Start()
    {
        if (competenceUpgradeButton != null)
            competenceUpgradeButton.onClick.AddListener(UpgradeCompetence);

        if (charismaUpgradeButton != null)
            charismaUpgradeButton.onClick.AddListener(UpgradeCharisma);

        if (insightUpgradeButton != null)
            insightUpgradeButton.onClick.AddListener(UpgradeInsight);

        PlayerSkillSession.EnsureLoaded();
        RefreshUI();
    }

    private void UpgradeCompetence()
    {
        TryUpgradeSkill("Competence");
    }

    private void UpgradeCharisma()
    {
        TryUpgradeSkill("Charisma");
    }

    private void UpgradeInsight()
    {
        TryUpgradeSkill("Insight");
    }

    private void TryUpgradeSkill(string skillName)
    {
        int currentLevel = GetSkillLevel(skillName);

        if (currentLevel >= PlayerSkillEffects.MaxSkillLevel)
        {
            Debug.Log($"{skillName} zaten maksimum seviyede.");
            RefreshUI();
            return;
        }

        int cost = SkillUpgradeCost.GetCostForLevel(currentLevel);
        float currentGold = TradingManager.GetSavedGold();

        if (currentGold < cost)
        {
            Debug.Log($"{skillName} upgrade için yeterli gold yok. Gerekli: {cost}, Mevcut: {currentGold}");
            return;
        }

        TradingManager.SetSavedGold(currentGold - cost);

        SetSkillLevel(skillName, currentLevel + 1);
        PlayerSkillSession.SaveToPrefs();

        Debug.Log($"{skillName} level yükseldi: {currentLevel} -> {currentLevel + 1}. Harcanan gold: {cost}");

        RefreshUI();
    }

    private int GetSkillLevel(string skillName)
    {
        switch (skillName)
        {
            case "Competence":
                return PlayerSkillSession.CompetenceLevel;

            case "Charisma":
                return PlayerSkillSession.CharismaLevel;

            case "Insight":
                return PlayerSkillSession.InsightLevel;

            default:
                return 0;
        }
    }

    private void SetSkillLevel(string skillName, int value)
    {
        value = Mathf.Clamp(value, 0, PlayerSkillEffects.MaxSkillLevel);

        switch (skillName)
        {
            case "Competence":
                PlayerSkillSession.CompetenceLevel = value;
                break;

            case "Charisma":
                PlayerSkillSession.CharismaLevel = value;
                break;

            case "Insight":
                PlayerSkillSession.InsightLevel = value;
                break;
        }
    }

    private void RefreshUI()
    {
        if (goldText != null)
            goldText.text = $"{TradingManager.GetSavedGold():0}";

        RefreshSkillUI(
            PlayerSkillSession.CompetenceLevel,
            competenceCircles,
            competenceCostText,
            competenceUpgradeButton
        );

        RefreshSkillUI(
            PlayerSkillSession.CharismaLevel,
            charismaCircles,
            charismaCostText,
            charismaUpgradeButton
        );

        RefreshSkillUI(
            PlayerSkillSession.InsightLevel,
            insightCircles,
            insightCostText,
            insightUpgradeButton
        );
    }

    private void RefreshSkillUI(Image[] circles, int level)
    {
        if (circles == null)
            return;

        for (int i = 0; i < circles.Length; i++)
        {
            if (circles[i] == null)
                continue;

            circles[i].color = i <= level ? activeColor : inactiveColor;
        }
    }

    private void RefreshSkillUI(int level, Image[] circles, TextMeshProUGUI costText, Button upgradeButton)
    {
        RefreshSkillUI(circles, level);

        if (level >= PlayerSkillEffects.MaxSkillLevel)
        {
            if (costText != null)
                costText.text = "MAX";

            if (upgradeButton != null)
                upgradeButton.interactable = false;

            return;
        }

        int cost = SkillUpgradeCost.GetCostForLevel(level);

        if (costText != null)
            costText.text = $"{cost}";

        if (upgradeButton != null)
            upgradeButton.interactable = TradingManager.GetSavedGold() >= cost;
    }
}