using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameManager : MonoBehaviour
{
    public void StartDay()
    {
        SceneManager.LoadScene("DayScreen");
    }

    public void Inventory()
    {
        SceneManager.LoadScene("InventoryScreen");
    }

    public void Profile()
    {
        SceneManager.LoadScene("ProfileScreen");
    }
}