using UnityEngine;

public static class PlayerSkillSession
{
    private static int _competenceLevel = 0;
    private static int _charismaLevel = 0;
    private static int _insightLevel = 0;

    private static bool _initialized = false;

    public static int CompetenceLevel
    {
        get
        {
            EnsureLoaded();
            return _competenceLevel;
        }
        set
        {
            _competenceLevel = Mathf.Clamp(value, 0, PlayerSkillEffects.MaxSkillLevel);
        }
    }

    public static int CharismaLevel
    {
        get
        {
            EnsureLoaded();
            return _charismaLevel;
        }
        set
        {
            _charismaLevel = Mathf.Clamp(value, 0, PlayerSkillEffects.MaxSkillLevel);
        }
    }

    public static int InsightLevel
    {
        get
        {
            EnsureLoaded();
            return _insightLevel;
        }
        set
        {
            _insightLevel = Mathf.Clamp(value, 0, PlayerSkillEffects.MaxSkillLevel);
        }
    }

    public static void EnsureLoaded()
    {
        if (!_initialized)
            LoadFromPrefs();
    }

    public static void ResetToDefaults()
    {
        _competenceLevel = 0;
        _charismaLevel = 0;
        _insightLevel = 0;
        _initialized = true;

        SaveToPrefs();
    }

    public static void LoadFromPrefs()
    {
        _competenceLevel = Mathf.Clamp(PlayerPrefs.GetInt("Skill_Competence", 0), 0, PlayerSkillEffects.MaxSkillLevel);
        _charismaLevel = Mathf.Clamp(PlayerPrefs.GetInt("Skill_Charisma", 0), 0, PlayerSkillEffects.MaxSkillLevel);
        _insightLevel = Mathf.Clamp(PlayerPrefs.GetInt("Skill_Insight", 0), 0, PlayerSkillEffects.MaxSkillLevel);

        _initialized = true;
    }

    public static void SaveToPrefs()
    {
        PlayerPrefs.SetInt("Skill_Competence", _competenceLevel);
        PlayerPrefs.SetInt("Skill_Charisma", _charismaLevel);
        PlayerPrefs.SetInt("Skill_Insight", _insightLevel);
        PlayerPrefs.Save();
    }
}