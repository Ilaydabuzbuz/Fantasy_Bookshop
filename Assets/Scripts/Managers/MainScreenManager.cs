using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreenManager : MonoBehaviour
{
    public void NewGame()
    {
        PlayerPrefs.SetInt("CurrentDay", 1);
        PlayerPrefs.SetInt("HasSave", 1);
        PlayerPrefs.Save();

        TradingManager.ResetGameSessionState();
        ReputationManager.ResetReputationsToDefault();
        PlayerSkillSession.ResetToDefaults();

        SceneManager.LoadScene("StartGameScreen");
    }

    public void Continue()
    {
        if (PlayerPrefs.GetInt("HasSave", 0) == 1)
        {
            SceneManager.LoadScene("DayScreen");
        }
        else
        {
            NewGame();
        }
    }

    public void Options()
    {
        SceneManager.LoadScene("OptionsScreen");
    }

    public void Exit()
    {
        Application.Quit();
    }
}