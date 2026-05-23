using UnityEngine;
using UnityEngine.SceneManagement;

public class CloseEndDayReport : MonoBehaviour
{
    public void Close()
    {
        SceneManager.UnloadSceneAsync("EndDayReport");

        // StartGameScreen yoksa yükle, varsa göster
        Scene startScene = SceneManager.GetSceneByName("StartGameScreen");
        if (startScene.IsValid())
        {
            foreach (GameObject root in startScene.GetRootGameObjects())
                root.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("StartGameScreen", LoadSceneMode.Additive);
        }
    }
}