using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class EndOfDayReportUI : MonoBehaviour
{
    [Header("Genel")]
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

    [Header("Kitaplar")]
    public TextMeshProUGUI booksSoldText;
    public TextMeshProUGUI booksBoughtText;

    private void OnEnable()
    {
        Populate();
    }

    private void Populate()
    {
        DayManager dm = DayManager.Instance;
        if (dm == null) return;

        if (dayText != null) dayText.text = $"{dm.currentDay}";
        if (earnedGoldText != null) earnedGoldText.text = $"{dm.goldEarnedToday:0}";
        if (totalCustomersText != null) totalCustomersText.text = $"{dm.customersServedToday}";
        if (booksSoldText != null) booksSoldText.text = $"{dm.booksSoldToday}";
        if (booksBoughtText != null) booksBoughtText.text = $"{dm.booksBoughtToday}";

        // Customers by Race
        SetRaceCount(dwarfCountText, CustomerRace.Dwarf, dm.customersByRace);
        SetRaceCount(vampireCountText, CustomerRace.Vampire, dm.customersByRace);
        SetRaceCount(elfCountText, CustomerRace.Elf, dm.customersByRace);
        SetRaceCount(humanCountText, CustomerRace.Human, dm.customersByRace);
        SetRaceCount(wizardCountText, CustomerRace.Wizard, dm.customersByRace);

        // Reputation by Race
        SetRaceRep(dwarfRepText, CustomerRace.Dwarf, dm.reputationEarnedToday);
        SetRaceRep(vampireRepText, CustomerRace.Vampire, dm.reputationEarnedToday);
        SetRaceRep(elfRepText, CustomerRace.Elf, dm.reputationEarnedToday);
        SetRaceRep(humanRepText, CustomerRace.Human, dm.reputationEarnedToday);
        SetRaceRep(wizardRepText, CustomerRace.Wizard, dm.reputationEarnedToday);
    }

    private void SetRaceCount(TextMeshProUGUI text, CustomerRace race,
        Dictionary<CustomerRace, int> dict)
    {
        if (text == null) return;
        text.text = dict.ContainsKey(race) ? dict[race].ToString() : "0";
    }

    private void SetRaceRep(TextMeshProUGUI text, CustomerRace race,
        Dictionary<CustomerRace, float> dict)
    {
        if (text == null) return;
        if (dict.ContainsKey(race))
        {
            float val = dict[race];
            text.text = val >= 0 ? $"+{val:0}" : $"{val:0}";
            text.color = val >= 0 ? Color.green : Color.red;
        }
        else
        {
            text.text = "0";
            text.color = Color.white;
        }
    }
}