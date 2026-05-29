using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CustomerTraitsUI : MonoBehaviour
{
    [Header("Character")]
    public Image customerImage;

    [Header("Trait Bars")]
    public Slider competenceBar;
    public Slider greedBar;
    public Slider patienceBar;

    [Header("Unknown Texts")]
    public TextMeshProUGUI competenceUnknownText;
    public TextMeshProUGUI greedUnknownText;
    public TextMeshProUGUI patienceUnknownText;

    [Header("Collector")]
    public Image collectorIcon;
    public Sprite collectorTrueSprite;
    public Sprite collectorFalseSprite;
    public Sprite questionMarkSprite;

    private void Start()
    {
        if (customerImage != null && CustomerClickHandler.selectedCustomerSprite != null)
        {
            customerImage.sprite = CustomerClickHandler.selectedCustomerSprite;
            customerImage.preserveAspect = true;
        }

        CustomerAI ai = CustomerClickHandler.selectedCustomerAI;

        if (ai != null)
        {
            ApplyTraitBar(competenceBar, competenceUnknownText, ai.competence, "Competence");
            ApplyTraitBar(greedBar, greedUnknownText, ai.greed, "Greed");
            ApplyTraitBar(patienceBar, patienceUnknownText, ai.patience, "Patience");
            ApplyCollectorInfo(ai);
        }
        else
        {
            Debug.LogWarning("[CustomerTraitsUI] Seçili CustomerAI bulunamadý.");
        }

        SetDayScreenVisible(false);
    }

    private void ApplyTraitBar(Slider bar, TextMeshProUGUI unknownText, float realValue, string traitName)
    {
        bool shouldReveal = PlayerSkillEffects.ShouldRevealCustomerTrait();

        if (bar != null)
        {
            bar.minValue = 0f;
            bar.maxValue = 100f;
            bar.value = shouldReveal ? realValue : 0f;
        }

        if (unknownText != null)
        {
            unknownText.gameObject.SetActive(!shouldReveal);
            unknownText.text = "???";
        }

        Debug.Log($"[Insight] Level: {PlayerSkillSession.InsightLevel}, {traitName} visible: {shouldReveal}");
    }

    private void ApplyCollectorInfo(CustomerAI ai)
    {
        bool shouldReveal = PlayerSkillEffects.ShouldRevealCustomerTrait();

        if (collectorIcon == null)
            return;

        if (!shouldReveal)
        {
            if (questionMarkSprite != null)
                collectorIcon.sprite = questionMarkSprite;

            Debug.Log($"[Insight] Level: {PlayerSkillSession.InsightLevel}, Collector visible: false");
            return;
        }

        collectorIcon.sprite = ai.isCollector ? collectorTrueSprite : collectorFalseSprite;

        Debug.Log($"[Insight] Level: {PlayerSkillSession.InsightLevel}, Collector visible: true");
    }

    private void SetDayScreenVisible(bool visible)
    {
        Scene dayScene = SceneManager.GetSceneByName("DayScreen");

        if (!dayScene.IsValid())
            return;

        foreach (GameObject root in dayScene.GetRootGameObjects())
        {
            if (root == null)
                continue;

            if (root.name == "EventSystem")
                continue;

            if (root.name == "GameManager")
                continue;

            root.SetActive(visible);
        }
    }
}