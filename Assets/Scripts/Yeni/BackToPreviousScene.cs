using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToPreviousScene : MonoBehaviour
{
    public void GoBack()
    {
        SceneManager.LoadScene("MainScreen");
    }
}