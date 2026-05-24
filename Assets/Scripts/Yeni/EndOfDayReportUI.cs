using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EndOfDayReportUI : MonoBehaviour
{
    [Header("General")]
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI earnedGoldText;
    public TextMeshProUGUI totalCustomersText;

    [Header("Customers by Race")]
    public TextMeshProUGUI dwarfCountText;
    public TextMeshProUGUI vampireCountText;
    public TextMeshProUGUI elfCountText;
    public TextMeshProUGUI humanCountText;
    public TextMeshProUGUI wizardCountText;

    [Header("Reputation by Race")]
    public TextMeshProUGUI dwarfRepText;
    public TextMeshProUGUI vampireRepText;
    public TextMeshProUGUI elfRepText;
    public TextMeshProUGUI humanRepText;
    public TextMeshProUGUI wizardRepText;

    [Header("Books")]
    public TextMeshProUGUI booksSoldText;
    public TextMeshProUGUI booksBoughtText;

    [Header("Objects That Should Be Hidden On Report")]
    public List<GameObject> objectsToHide = new List<GameObject>();

    private readonly HashSet<string> unwantedTextContents = new HashSet<string>
    {
        "BOOK TITLE",
        "EDITION",
        "CONDITION",
        "RARITY",
        "MAGIC LEVEL",
        "AGE",
        "CURSE",
        "VALUE:",
        "VALUE"
    };

    private void OnEnable()
    {
        HideDayScreenObjects();
        HideUnwantedReportTexts();
        Populate();
    }

    private void HideDayScreenObjects()
    {
        Scene dayScene = SceneManager.GetSceneByName("DayScreen");

        if (!dayScene.IsValid())
            return;

        foreach (GameObject root in dayScene.GetRootGameObjects())
        {
            if (root == null)
                continue;

            if (root.name == "GameManager")
                continue;

            if (root.name == "EventSystem")
                continue;

            if (root.name == "CustomerPoint")
                continue;

            if (root.name == "BookPoint")
                continue;

            root.SetActive(false);
        }
    }

    private void HideUnwantedReportTexts()
    {
        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        TextMeshProUGUI[] allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text == null)
                continue;

            string cleanedText = text.text.Trim().ToUpper();

            if (unwantedTextContents.Contains(cleanedText))
            {
                text.gameObject.SetActive(false);
            }
        }
    }

    private void Populate()
    {
        DayManager dm = DayManager.Instance;

        if (dm == null)
        {
            Debug.LogError("EndOfDayReportUI: DayManager bulunamadý.");
            return;
        }

        if (dayText != null)
            dayText.text = $"{dm.currentDay}";

        if (earnedGoldText != null)
            earnedGoldText.text = $"{dm.goldEarnedToday:0}";

        if (totalCustomersText != null)
            totalCustomersText.text = $"{dm.customersServedToday}";

        if (booksSoldText != null)
            booksSoldText.text = $"{dm.booksSoldToday}";

        if (booksBoughtText != null)
            booksBoughtText.text = $"{dm.booksBoughtToday}";

        SetRaceCount(dwarfCountText, CustomerRace.Dwarf, dm.customersByRace);
        SetRaceCount(vampireCountText, CustomerRace.Vampire, dm.customersByRace);
        SetRaceCount(elfCountText, CustomerRace.Elf, dm.customersByRace);
        SetRaceCount(humanCountText, CustomerRace.Human, dm.customersByRace);
        SetRaceCount(wizardCountText, CustomerRace.Wizard, dm.customersByRace);

        SetRaceRep(dwarfRepText, CustomerRace.Dwarf, dm.reputationEarnedToday);
        SetRaceRep(vampireRepText, CustomerRace.Vampire, dm.reputationEarnedToday);
        SetRaceRep(elfRepText, CustomerRace.Elf, dm.reputationEarnedToday);
        SetRaceRep(humanRepText, CustomerRace.Human, dm.reputationEarnedToday);
        SetRaceRep(wizardRepText, CustomerRace.Wizard, dm.reputationEarnedToday);
    }

    private void SetRaceCount(TextMeshProUGUI text, CustomerRace race, Dictionary<CustomerRace, int> dict)
    {
        if (text == null)
            return;

        if (dict != null && dict.ContainsKey(race))
            text.text = dict[race].ToString();
        else
            text.text = "0";
    }

    private void SetRaceRep(TextMeshProUGUI text, CustomerRace race, Dictionary<CustomerRace, float> dict)
    {
        if (text == null)
            return;

        if (dict != null && dict.ContainsKey(race))
        {
            float value = dict[race];

            text.text = value > 0 ? $"+{value:0}" : $"{value:0}";

            if (value > 0)
                text.color = Color.green;
            else if (value < 0)
                text.color = Color.red;
            else
                text.color = Color.white;
        }
        else
        {
            text.text = "0";
            text.color = Color.white;
        }
    }
}