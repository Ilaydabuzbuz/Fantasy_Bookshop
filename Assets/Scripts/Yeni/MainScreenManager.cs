using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreenManager : MonoBehaviour
{
    public void NewGame()
    {
        SceneManager.LoadScene("StartGameScreen");
    }

    public void Continue()
    {
        SceneManager.LoadScene("");
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