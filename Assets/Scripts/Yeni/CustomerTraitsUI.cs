using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CustomerTraitsUI : MonoBehaviour
{
    [Header("Character")]
    public Image customerImage;

    [Header("Trait Bars")]
    public Slider competenceBar;
    public Slider greedBar;
    public Slider patienceBar;

    [Header("Collector")]
    public Image collectorIcon;
    public Sprite collectorTrueSprite;
    public Sprite collectorFalseSprite;

    private void Start()
    {
        customerImage.sprite = CustomerClickHandler.selectedCustomerSprite;
        customerImage.preserveAspect = true;

        CustomerAI ai = CustomerClickHandler.selectedCustomerAI;
        if (ai != null)
        {
            if (competenceBar != null)
            {
                competenceBar.minValue = 0;
                competenceBar.maxValue = 100;
                competenceBar.value = ai.competence;
            }
            if (greedBar != null)
            {
                greedBar.minValue = 0;
                greedBar.maxValue = 100;
                greedBar.value = ai.greed;
            }
            if (patienceBar != null)
            {
                patienceBar.minValue = 0;
                patienceBar.maxValue = 100;
                patienceBar.value = ai.patience;
            }

            if (collectorIcon != null)
            {
                collectorIcon.sprite = ai.isCollector ? collectorTrueSprite : collectorFalseSprite;
            }
        }

        SetDayScreenVisible(false);
    }

    private void SetDayScreenVisible(bool visible)
    {
        Scene dayScene = SceneManager.GetSceneByName("DayScreen");
        if (!dayScene.IsValid()) return;

        foreach (GameObject root in dayScene.GetRootGameObjects())
        {
            if (root.name == "DontDestroyOnLoad") continue;
            if (root.name == "EventSystem") continue;
            if (root.name == "GameManager") continue;
            root.SetActive(visible);
        }
    }
}