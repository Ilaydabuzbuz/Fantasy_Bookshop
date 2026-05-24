using UnityEngine;
using UnityEngine.SceneManagement;

public class CloseEndDayReport : MonoBehaviour
{
    private bool alreadyClosed = false;

    public void Close()
    {
        if (alreadyClosed)
            return;

        alreadyClosed = true;

        if (DayManager.Instance != null)
        {
            DayManager.Instance.AdvanceToNextDay();
        }
        else
        {
            int currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
            PlayerPrefs.SetInt("CurrentDay", currentDay + 1);
            PlayerPrefs.SetInt("HasSave", 1);
            PlayerPrefs.Save();
        }

        SceneManager.LoadScene("StartGameScreen");
    }
}