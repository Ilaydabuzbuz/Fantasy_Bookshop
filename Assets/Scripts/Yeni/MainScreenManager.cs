using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreenManager : MonoBehaviour
{
    public void NewGame()
    {
        // Her þeyi sýfýrla
        if (DayManager.Instance != null)
            DayManager.Instance.currentDay = 1;

        TradingManager tm = null;
        foreach (TradingManager t in Resources.FindObjectsOfTypeAll<TradingManager>())
        { tm = t; break; }
        if (tm != null)
        {
            tm.playerGold = 1000f;
            tm.purchasedItems.Clear();
        }

        // StartGameScreen'i additive yükle
        Scene startScene = SceneManager.GetSceneByName("StartGameScreen");
        if (!startScene.IsValid())
            SceneManager.LoadScene("StartGameScreen", LoadSceneMode.Additive);
        else
            foreach (GameObject root in startScene.GetRootGameObjects())
                root.SetActive(true);

        // MainScreen'i gizle
        HideMainScreen();
    }

    public void Continue()
    {
        Scene dayScene = SceneManager.GetSceneByName("DayScreen");
        if (dayScene.IsValid())
        {
            // DayScreen zaten yüklü, sadece göster
            foreach (GameObject root in dayScene.GetRootGameObjects())
            {
                if (root.name == "DontDestroyOnLoad") continue;
                if (root.name == "EventSystem") continue;
                root.SetActive(true);
            }
            HideMainScreen();
            // Yeni gün baþlat
            DayManager.Instance?.AdvanceToNextDay();
        }
        else
        {
            // DayScreen hiç yüklenmemiþ, additive yükle
            // Start() içinde StartNewDay() çalýþacak
            SceneManager.LoadScene("DayScreen", LoadSceneMode.Additive);
            HideMainScreen();
        }
    }

    private void HideMainScreen()
    {
        Scene mainScene = SceneManager.GetSceneByName("MainScreen");
        if (!mainScene.IsValid()) return;
        foreach (GameObject root in mainScene.GetRootGameObjects())
        {
            if (root.name == "DontDestroyOnLoad") continue;
            if (root.name == "EventSystem") continue;
            root.SetActive(false);
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