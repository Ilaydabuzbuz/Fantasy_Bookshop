using UnityEngine;

public static class PlayerSkillEffects
{
    public const int MaxSkillLevel = 5;

    // Competence düþükse kitap deðeri daha sýk ??? görünür.
    // Level 0: %70 gizli
    // Level 5: %0 gizli
    private static readonly float[] hiddenBookValueChanceByCompetence =
    {
        0.70f, // level 0
        0.50f, // level 1
        0.35f, // level 2
        0.20f, // level 3
        0.08f, // level 4
        0.00f  // level 5
    };

    // Charisma müþterinin mevcut patience deðerine çarpan uygular.
    // Müþterinin kendi patience deðeri yine var, sadece oyuncunun charisma'sý onu etkiler.
    private static readonly float[] patienceMultiplierByCharisma =
    {
        0.75f, // level 0 - müþteri daha sabýrsýz
        0.90f, // level 1
        1.00f, // level 2 - normal
        1.15f, // level 3
        1.30f, // level 4
        1.50f  // level 5 - müþteri daha sabýrlý
    };

    // Insight düþükse müþteri özellikleri daha sýk gizlenir.
    // Level 0: %20 görme ihtimali
    // Level 5: %100 görme ihtimali
    private static readonly float[] traitRevealChanceByInsight =
    {
        0.20f, // level 0
        0.40f, // level 1
        0.60f, // level 2
        0.75f, // level 3
        0.90f, // level 4
        1.00f  // level 5
    };

    public static bool ShouldHideBookValue()
    {
        PlayerSkillSession.EnsureLoaded();

        int level = Mathf.Clamp(PlayerSkillSession.CompetenceLevel, 0, MaxSkillLevel);
        float hideChance = hiddenBookValueChanceByCompetence[level];

        return Random.value < hideChance;
    }

    public static float ApplyCharismaToPatience(float basePatience)
    {
        PlayerSkillSession.EnsureLoaded();

        int level = Mathf.Clamp(PlayerSkillSession.CharismaLevel, 0, MaxSkillLevel);
        float multiplier = patienceMultiplierByCharisma[level];

        return Mathf.Clamp(basePatience * multiplier, 5f, 100f);
    }

    public static bool ShouldRevealCustomerTrait()
    {
        PlayerSkillSession.EnsureLoaded();

        int level = Mathf.Clamp(PlayerSkillSession.InsightLevel, 0, MaxSkillLevel);
        float revealChance = traitRevealChanceByInsight[level];

        return Random.value < revealChance;
    }
}