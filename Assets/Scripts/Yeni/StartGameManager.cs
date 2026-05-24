using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameManager : MonoBehaviour
{
    public void StartDay()
    {
        // DayScreen zaten yüklüyse göster, yoksa yükle
        Scene dayScene = SceneManager.GetSceneByName("DayScreen");
        if (dayScene.IsValid())
        {
            foreach (GameObject root in dayScene.GetRootGameObjects())
            {
                if (root.name == "DontDestroyOnLoad") continue;
                if (root.name == "EventSystem") continue;
                root.SetActive(true);
            }

            // StartGameScreen'i gizle
            Scene startScene = SceneManager.GetSceneByName("StartGameScreen");
            if (startScene.IsValid())
            {
                foreach (GameObject root in startScene.GetRootGameObjects())
                {
                    if (root.name == "DontDestroyOnLoad") continue;
                    if (root.name == "EventSystem") continue;
                    root.SetActive(false);
                }
            }

            DayManager.Instance?.AdvanceToNextDay();
        }
        else
        {
            SceneManager.LoadScene("DayScreen");
        }
    }

    public void Inventory()
    {
        Scene inventoryScene = SceneManager.GetSceneByName("InventoryScreen");
        if (!inventoryScene.IsValid())
            SceneManager.LoadScene("InventoryScreen", LoadSceneMode.Additive);

        Scene startScene = SceneManager.GetSceneByName("StartGameScreen");
        if (startScene.IsValid())
            foreach (GameObject root in startScene.GetRootGameObjects())
            {
                if (root.name == "DontDestroyOnLoad") continue;
                if (root.name == "EventSystem") continue;
                root.SetActive(false);
            }
    }

    public void Profile()
    {
        SceneManager.LoadScene("ProfileScreen");
    }
}