using UnityEngine;
using UnityEngine.SceneManagement;

public class CloseTraits : MonoBehaviour
{
    public void GoBack()
    {
        SceneManager.LoadScene("OpenDay");
    }
}