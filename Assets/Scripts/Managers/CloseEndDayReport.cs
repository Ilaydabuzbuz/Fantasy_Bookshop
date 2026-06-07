using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CloseEndDayReport : MonoBehaviour
{
    [Header("Rent Popup UI")]
    public GameObject rentPopupPanel;
    public TextMeshProUGUI rentPopupTitleText;
    public TextMeshProUGUI rentPopupMessageText;

    [Header("Scene Settings")]
    public string startGameSceneName = "StartGameScreen";

    private bool dayAdvanced = false;
    private bool rentPopupOpen = false;

    private void Start()
    {
        if (rentPopupPanel != null)
            rentPopupPanel.SetActive(false);
    }

    public void Close()
    {
        if (rentPopupOpen)
        {
            CloseRentPopupAndGoToStart();
            return;
        }

        if (dayAdvanced)
            return;

        dayAdvanced = true;

        if (DayManager.Instance != null)
        {
            DayManager.Instance.AdvanceToNextDay();
        }
        else
        {
            int currentDay = PlayerPrefs.GetInt("CurrentDay", 1);
            int nextDay = currentDay + 1;

            PlayerPrefs.SetInt("CurrentDay", nextDay);
            PlayerPrefs.SetInt("HasSave", 1);
            PlayerPrefs.Save();
        }

        if (DayManager.pendingRentPopup)
        {
            ShowRentPopup();
        }
        else
        {
            GoToStartGameScreen();
        }
    }

    private void ShowRentPopup()
    {
        if (rentPopupPanel == null)
        {
            Debug.LogWarning("Rent Popup Panel is not assigned. Going to StartGameScreen.");
            DayManager.ClearRentPopup();
            GoToStartGameScreen();
            return;
        }

        rentPopupOpen = true;
        rentPopupPanel.SetActive(true);

        if (rentPopupTitleText != null)
            rentPopupTitleText.text = "Rent Day";

        if (rentPopupMessageText != null)
        {
            rentPopupMessageText.text =
                "Rent payment day has arrived.\n" +
                $"Rent paid: {DayManager.lastRentPaid:0} gold.";
        }
    }

    private void CloseRentPopupAndGoToStart()
    {
        rentPopupOpen = false;

        if (rentPopupPanel != null)
            rentPopupPanel.SetActive(false);

        DayManager.ClearRentPopup();
        GoToStartGameScreen();
    }

    private void GoToStartGameScreen()
    {
        SceneManager.LoadScene(startGameSceneName);
    }
}