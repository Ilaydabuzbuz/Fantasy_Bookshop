using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CloseEndDayReport : MonoBehaviour
{
    [Header("Rent Popup")]
    public GameObject rentPopupPanel;
    public TextMeshProUGUI rentPopupMessageText;

    [Header("Bankruptcy Popup")]
    public GameObject bankruptcyPopupPanel;
    public Button restartButton;

    [Header("Scene Settings")]
    public string startGameSceneName = "StartGameScreen";
    public string mainScreenSceneName = "MainScreen";

    private bool dayAdvanced = false;
    private bool rentPopupOpen = false;
    private bool bankruptcyPopupOpen = false;

    private CanvasGroup bankruptcyCanvasGroup;

    private void Start()
    {
        if (rentPopupPanel != null)
            rentPopupPanel.SetActive(false);

        if (bankruptcyPopupPanel != null)
        {
            bankruptcyCanvasGroup =
                bankruptcyPopupPanel.GetComponent<CanvasGroup>();

            if (bankruptcyCanvasGroup == null)
            {
                bankruptcyCanvasGroup =
                    bankruptcyPopupPanel.AddComponent<CanvasGroup>();
            }

            bankruptcyPopupPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    public void Close()
    {
        if (bankruptcyPopupOpen)
            return;

        if (rentPopupOpen)
        {
            CloseRentPopupAndGoToStart();
            return;
        }

        if (dayAdvanced)
            return;

        dayAdvanced = true;

        if (DayManager.Instance == null)
        {
            Debug.LogError("DayManager could not be found.");
            dayAdvanced = false;
            return;
        }

        DayManager.Instance.AdvanceToNextDay();

        if (IsPlayerBankrupt())
        {
            ShowBankruptcyPopup();
            return;
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

    private bool IsPlayerBankrupt()
    {
        return TradingManager.GetSavedGold() < 0f;
    }

    private void ShowRentPopup()
    {
        if (rentPopupPanel == null)
        {
            Debug.LogWarning(
                "Rent Popup Panel is not assigned. Going to StartGameScreen."
            );

            DayManager.ClearRentPopup();
            GoToStartGameScreen();
            return;
        }

        rentPopupOpen = true;

        rentPopupPanel.SetActive(true);
        rentPopupPanel.transform.SetAsLastSibling();

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

    private void ShowBankruptcyPopup()
    {
        bankruptcyPopupOpen = true;
        rentPopupOpen = false;

        if (rentPopupPanel != null)
            rentPopupPanel.SetActive(false);

        if (bankruptcyPopupPanel == null)
        {
            Debug.LogError(
                "Bankruptcy Popup Panel is not assigned."
            );

            bankruptcyPopupOpen = false;
            return;
        }

        bankruptcyPopupPanel.SetActive(true);
        bankruptcyPopupPanel.transform.SetAsLastSibling();

        SetupBankruptcyInteraction();

        Debug.Log("Bankruptcy popup opened.");
    }

    private void SetupBankruptcyInteraction()
    {
        if (bankruptcyPopupPanel == null)
            return;

        if (bankruptcyCanvasGroup == null)
        {
            bankruptcyCanvasGroup =
                bankruptcyPopupPanel.GetComponent<CanvasGroup>();

            if (bankruptcyCanvasGroup == null)
            {
                bankruptcyCanvasGroup =
                    bankruptcyPopupPanel.AddComponent<CanvasGroup>();
            }
        }

        bankruptcyCanvasGroup.alpha = 1f;
        bankruptcyCanvasGroup.interactable = true;
        bankruptcyCanvasGroup.blocksRaycasts = true;
        bankruptcyCanvasGroup.ignoreParentGroups = true;

        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            restartButton.interactable = true;

            Graphic targetGraphic = restartButton.targetGraphic;

            if (targetGraphic != null)
                targetGraphic.raycastTarget = true;

            Image buttonImage = restartButton.GetComponent<Image>();

            if (buttonImage != null)
                buttonImage.raycastTarget = true;
        }
        else
        {
            Debug.LogError(
                "Restart Button is not assigned in CloseEndDayReport."
            );
        }

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);

            if (restartButton != null)
            {
                EventSystem.current.SetSelectedGameObject(
                    restartButton.gameObject
                );
            }
        }
        else
        {
            Debug.LogError(
                "No active EventSystem was found. UI buttons cannot be clicked."
            );
        }
    }

    public void RestartGame()
    {
        Debug.Log("Restart button clicked.");

        bankruptcyPopupOpen = false;
        rentPopupOpen = false;
        dayAdvanced = false;

        if (bankruptcyPopupPanel != null)
            bankruptcyPopupPanel.SetActive(false);

        if (rentPopupPanel != null)
            rentPopupPanel.SetActive(false);

        TradingManager.ResetGameSessionState();
        DayManager.ClearRentPopup();

        PlayerPrefs.SetInt("CurrentDay", 1);
        PlayerPrefs.SetInt("HasSave", 0);
        PlayerPrefs.Save();

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            mainScreenSceneName,
            LoadSceneMode.Single
        );
    }

    private void GoToStartGameScreen()
    {
        SceneManager.LoadScene(
            startGameSceneName,
            LoadSceneMode.Single
        );
    }
}