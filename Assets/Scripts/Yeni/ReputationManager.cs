using System.Collections.Generic;
using UnityEngine;

public class ReputationManager : MonoBehaviour
{
    public static ReputationManager Instance;

    private Dictionary<CustomerRace, float> raceReputations = new Dictionary<CustomerRace, float>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeReputations();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeReputations()
    {
        raceReputations[CustomerRace.Dwarf] = 20f;
        raceReputations[CustomerRace.Vampire] = 20f;
        raceReputations[CustomerRace.Elf] = 20f;
        raceReputations[CustomerRace.Human] = 50f;
        raceReputations[CustomerRace.Wizard] = 20f;

        if (!raceReputations.ContainsKey(CustomerRace.Orc))
            raceReputations[CustomerRace.Orc] = 50f;
    }

    public float GetReputation(CustomerRace race)
    {
        if (raceReputations.ContainsKey(race))
            return raceReputations[race];
        return 50f;
    }

    public void ModifyReputation(CustomerRace race, float amount)
    {
        if (raceReputations.ContainsKey(race))
        {
            raceReputations[race] = Mathf.Clamp(raceReputations[race] + amount, 0f, 100f);
            Debug.Log($"{race} itibarý deðiþti: {amount}. Yeni Skor: {raceReputations[race]}");
        }
    }
}