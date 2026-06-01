using System;
using System.Collections.Generic;
using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance;

    private static Dictionary<CustomerRace, float> raceReputations = new Dictionary<CustomerRace, float>();
    private static bool initialized = false;

    public static event Action OnReputationChanged;

    private const float DefaultReputation = 20f;
    private const float HumanDefaultReputation = 50f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeIfNeeded();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private static void InitializeIfNeeded()
    {
        if (initialized)
            return;

        initialized = true;
        raceReputations.Clear();

        LoadRace(CustomerRace.Dwarf);
        LoadRace(CustomerRace.Elf);
        LoadRace(CustomerRace.Vampire);
        LoadRace(CustomerRace.Human);
        LoadRace(CustomerRace.Wizard);
    }

    private static void LoadRace(CustomerRace race)
    {
        float defaultValue = GetDefaultValue(race);
        raceReputations[race] = PlayerPrefs.GetFloat(GetKey(race), defaultValue);
    }

    private static float GetDefaultValue(CustomerRace race)
    {
        if (race == CustomerRace.Human)
            return HumanDefaultReputation;

        return DefaultReputation;
    }

    private static string GetKey(CustomerRace race)
    {
        return $"Reputation_{race}";
    }

    public static float GetReputationValue(CustomerRace race)
    {
        InitializeIfNeeded();

        if (!raceReputations.ContainsKey(race))
        {
            float defaultValue = GetDefaultValue(race);
            raceReputations[race] = PlayerPrefs.GetFloat(GetKey(race), defaultValue);
        }

        return raceReputations[race];
    }

    public float GetReputation(CustomerRace race)
    {
        return GetReputationValue(race);
    }

    public static void ModifyReputationValue(CustomerRace race, float amount)
    {
        InitializeIfNeeded();

        float oldValue = GetReputationValue(race);
        float newValue = Mathf.Clamp(oldValue + amount, 0f, 100f);

        raceReputations[race] = newValue;

        PlayerPrefs.SetFloat(GetKey(race), newValue);
        PlayerPrefs.Save();

        Debug.Log($"{race} itibarý deðiþti: {amount}. Eski Skor: {oldValue}, Yeni Skor: {newValue}");

        OnReputationChanged?.Invoke();
    }

    public void ModifyReputation(CustomerRace race, float amount)
    {
        ModifyReputationValue(race, amount);
    }

    public static void ResetReputationsToDefault()
    {
        initialized = true;
        raceReputations.Clear();

        raceReputations[CustomerRace.Dwarf] = DefaultReputation;
        raceReputations[CustomerRace.Elf] = DefaultReputation;
        raceReputations[CustomerRace.Vampire] = DefaultReputation;
        raceReputations[CustomerRace.Human] = HumanDefaultReputation;
        raceReputations[CustomerRace.Wizard] = DefaultReputation;

        foreach (KeyValuePair<CustomerRace, float> pair in raceReputations)
        {
            PlayerPrefs.SetFloat(GetKey(pair.Key), pair.Value);
        }

        PlayerPrefs.Save();

        Debug.Log("Tüm reputation deðerleri baþlangýç deðerlerine sýfýrlandý.");

        OnReputationChanged?.Invoke();
    }

    public static void ForceReloadFromPlayerPrefs()
    {
        initialized = false;
        InitializeIfNeeded();
        OnReputationChanged?.Invoke();
    }
}