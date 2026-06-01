using UnityEngine;
using UnityEngine.SceneManagement;

public class CloseCustomerTraits : MonoBehaviour
{
    public void Close()
    {
        Scene dayScene = SceneManager.GetSceneByName("DayScreen");
        if (dayScene.IsValid())
        {
            foreach (GameObject root in dayScene.GetRootGameObjects())
            {
                if (root.name == "DontDestroyOnLoad") continue;
                if (root.name == "EventSystem") continue;
                if (root.name == "GameManager") continue;
                root.SetActive(true);
            }
        }

        CustomerClickHandler.traitsOpen = false;
        SceneManager.UnloadSceneAsync("CustomerTraitsScreen");
    }
}