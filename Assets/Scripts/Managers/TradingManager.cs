using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TradingManager : MonoBehaviour
{
    [Header("UI References - Core")]
    public AnimatedCounter goldCounter;
    public TextMeshProUGUI bookTitleText;
    public TextMeshProUGUI bookStatsText;
    public Image bookIconImage;

    [Header("UI References - Labels")]
    public TextMeshProUGUI intentLabelText;
    public TextMeshProUGUI boughtForText;

    [Header("UI References - Book Details")]
    public TextMeshProUGUI editionText;
    public TextMeshProUGUI conditionText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI magicLevelText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI curseText;

    [Header("UI References - Dialogue")]
    public GameObject dialogueGroup;
    public TextMeshProUGUI dialogueText;
    public TMP_InputField offerInputField;

    [Header("Buttons")]
    public Button offerButton;
    public Button increaseButton;
    public Button decreaseButton;
    public Button refuseButton;
    public Button laterButton;

    [Header("Economy Settings")]
    public float playerGold = 1000f;
    public float offerIncrement = 10f;
    public float mouseScrollIncrement = 1f;
    public List<ItemData> purchasedItems = new List<ItemData>();

    private Dictionary<ItemData, float> purchasePrices =
        new Dictionary<ItemData, float>();

    private static bool sessionInitialized = false;
    private static float savedPlayerGold = 1000f;

    private static List<ItemData> savedPurchasedItems =
        new List<ItemData>();

    private static Dictionary<ItemData, float> savedPurchasePrices =
        new Dictionary<ItemData, float>();

    [Header("System References")]
    public CustomerSpawner customerSpawner;

    [Header("Rare Book Effects")]
    public RareBookEffectController rareBookEffectController;

    private CustomerAI currentCustomer;

    private bool waitingForNextCustomer = false;
    private bool currentBookValueHidden = false;

    private bool currentCustomerRequestedBookUnavailable = false;

    private AudioSource _sfxSource;
    private AudioClip _sellGoldClip;
    private AudioClip _buyBookClip;
    private AudioClip _rejectClip;

    private void Start()
    {
        LoadGameSessionState();

        if (goldCounter != null)
            goldCounter.SetValueInstant(playerGold);

        ClearUI();

        if (offerButton != null)
            offerButton.onClick.AddListener(SubmitOffer);

        if (increaseButton != null)
            increaseButton.onClick.AddListener(IncreaseOffer);

        if (decreaseButton != null)
            decreaseButton.onClick.AddListener(DecreaseOffer);

        if (refuseButton != null)
            refuseButton.onClick.AddListener(RejectCustomer);

        if (laterButton != null)
            laterButton.onClick.AddListener(LaterCustomer);

        InitAudio();
    }

    private void Update()
    {
        HandleOfferMouseScroll();
    }

    private void InitAudio()
    {
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.spatialBlend = 0f;
        _sfxSource.volume = 1f;
        _sfxSource.priority = 0;

        _sellGoldClip = GenerateSellGoldClip();
        _buyBookClip = GenerateBuyBookClip();
        _rejectClip = GenerateRejectClip();
    }

    private void PlaySfx(
        AudioClip clip,
        float volume = 1f,
        float pitch = 1f)
    {
        if (_sfxSource == null || clip == null)
            return;

        _sfxSource.pitch = pitch;

        float boostedVolume = volume * 2.5f;

        _sfxSource.PlayOneShot(clip, boostedVolume);
    }

    private AudioClip GenerateSellGoldClip()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int totalSamples = sampleRate * 6 / 10;

        float[] samples = new float[totalSamples];

        float[] freqs = { 880f, 1100f, 1320f };

        float coinDur = 0.12f;
        int coinSamp = Mathf.RoundToInt(sampleRate * coinDur);

        for (int c = 0; c < freqs.Length; c++)
        {
            int offset =
                c * Mathf.RoundToInt(sampleRate * 0.15f);

            for (
                int i = 0;
                i < coinSamp && offset + i < totalSamples;
                i++)
            {
                float t = (float)i / coinSamp;
                float envelope = Mathf.Exp(-t * 12f);

                float wave =
                    Mathf.Sin(2f * Mathf.PI * freqs[c] * t);

                wave +=
                    0.3f *
                    Mathf.Sin(
                        2f *
                        Mathf.PI *
                        freqs[c] *
                        2f *
                        t
                    );

                samples[offset + i] +=
                    wave * envelope * 0.8f;
            }
        }

        AudioClip clip = AudioClip.Create(
            "SellGold",
            totalSamples,
            1,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);

        return clip;
    }

    private AudioClip GenerateBuyBookClip()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int totalSamples = sampleRate * 5 / 10;

        float[] samples = new float[totalSamples];

        int thudSamples =
            Mathf.RoundToInt(sampleRate * 0.08f);

        for (
            int i = 0;
            i < thudSamples && i < totalSamples;
            i++)
        {
            float t = (float)i / thudSamples;
            float envelope = Mathf.Exp(-t * 20f);

            float wave =
                Mathf.Sin(2f * Mathf.PI * 180f * t);

            wave +=
                0.5f *
                Mathf.Sin(2f * Mathf.PI * 90f * t);

            samples[i] += wave * envelope * 0.9f;
        }

        int pageOffset =
            Mathf.RoundToInt(sampleRate * 0.10f);

        int pageSamples =
            Mathf.RoundToInt(sampleRate * 0.18f);

        for (
            int i = 0;
            i < pageSamples &&
            pageOffset + i < totalSamples;
            i++)
        {
            float t = (float)i / pageSamples;
            float envelope = Mathf.Exp(-t * 18f);

            float wave = Random.value * 2f - 1f;

            samples[pageOffset + i] +=
                wave * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create(
            "BuyBook",
            totalSamples,
            1,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);

        return clip;
    }

    private AudioClip GenerateRejectClip()
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int totalSamples = sampleRate * 5 / 10;

        float[] samples = new float[totalSamples];

        float[] freqs = { 350f, 260f };

        float noteDur = 0.18f;
        int noteSamp = Mathf.RoundToInt(sampleRate * noteDur);

        for (int n = 0; n < freqs.Length; n++)
        {
            int offset =
                n * Mathf.RoundToInt(sampleRate * 0.20f);

            for (
                int i = 0;
                i < noteSamp &&
                offset + i < totalSamples;
                i++)
            {
                float t = (float)i / noteSamp;
                float envelope = Mathf.Exp(-t * 8f);

                float wave =
                    Mathf.Sin(
                        2f *
                        Mathf.PI *
                        freqs[n] *
                        t
                    );

                wave +=
                    0.4f *
                    Mathf.Sin(
                        2f *
                        Mathf.PI *
                        freqs[n] *
                        0.5f *
                        t
                    );

                samples[offset + i] +=
                    wave * envelope * 0.85f;
            }
        }

        AudioClip clip = AudioClip.Create(
            "Reject",
            totalSamples,
            1,
            sampleRate,
            false
        );

        clip.SetData(samples, 0);

        return clip;
    }

    private void LoadGameSessionState()
    {
        if (!sessionInitialized)
        {
            sessionInitialized = true;

            savedPlayerGold = playerGold;

            savedPurchasedItems =
                new List<ItemData>(purchasedItems);

            savedPurchasePrices =
                new Dictionary<ItemData, float>(
                    purchasePrices
                );
        }

        playerGold = savedPlayerGold;

        purchasedItems =
            new List<ItemData>(savedPurchasedItems);

        purchasePrices =
            new Dictionary<ItemData, float>(
                savedPurchasePrices
            );
    }

    public void SaveGameSessionState()
    {
        sessionInitialized = true;

        savedPlayerGold = playerGold;

        savedPurchasedItems =
            new List<ItemData>(purchasedItems);

        savedPurchasePrices =
            new Dictionary<ItemData, float>(
                purchasePrices
            );
    }

    public static void ResetGameSessionState()
    {
        sessionInitialized = true;
        savedPlayerGold = 1000f;

        savedPurchasedItems =
            new List<ItemData>();

        savedPurchasePrices =
            new Dictionary<ItemData, float>();
    }

    public static float GetSavedGold()
    {
        return savedPlayerGold;
    }

    public static void SetSavedGold(float value)
    {
        sessionInitialized = true;
        savedPlayerGold = value;
    }

    public static List<ItemData> GetSavedPurchasedItems()
    {
        return new List<ItemData>(savedPurchasedItems);
    }

    public bool TryGetPurchasePrice(
        ItemData item,
        out float price)
    {
        if (
            item != null &&
            purchasePrices.ContainsKey(item)
        )
        {
            price = purchasePrices[item];
            return true;
        }

        price = 0f;
        return false;
    }

    public static bool TryGetSavedPurchasePrice(
        ItemData item,
        out float price)
    {
        if (
            item != null &&
            savedPurchasePrices.ContainsKey(item)
        )
        {
            price = savedPurchasePrices[item];
            return true;
        }

        price = 0f;
        return false;
    }

    public void SetupNewCustomer(CustomerAI newCustomer)
    {
        SetupNewCustomer(
            newCustomer,
            false,
            true
        );
    }

    public void SetupNewCustomer(
        CustomerAI newCustomer,
        bool isReturningCustomer)
    {
        SetupNewCustomer(
            newCustomer,
            isReturningCustomer,
            true
        );
    }

    public void SetupNewCustomer(
        CustomerAI newCustomer,
        bool isReturningCustomer,
        bool requestedBookStillAvailable)
    {
        if (newCustomer == null)
        {
            Debug.LogWarning(
                "SetupNewCustomer received a null customer."
            );

            return;
        }

        currentCustomer = newCustomer;

        waitingForNextCustomer = false;

        currentCustomerRequestedBookUnavailable = false;

        SetButtonsInteractable(true);

        if (laterButton != null)
            laterButton.interactable = !isReturningCustomer;

        if (dialogueGroup != null)
            dialogueGroup.SetActive(true);

        if (newCustomer.itemToSell == null)
        {
            ShowDialogue(
                "This customer has no book."
            );

            return;
        }

        ItemData item = newCustomer.itemToSell;

        if (offerInputField != null)
            offerInputField.text = "";

        string greeting = "";

        int roundedPrice =
            Mathf.RoundToInt(
                newCustomer.customerDesiredPrice
            );

        if (
            newCustomer.intent ==
            CustomerIntent.SellToPlayer
        )
        {
            string[] sellerDialogues =
            {
                $"I found this old book. I'm willing to part with it for {roundedPrice} gold.",
                $"Would you be interested in purchasing this? I'm asking {roundedPrice} gold.",
                $"I need some quick coin. Take this off my hands for {roundedPrice} gold.",
                $"A rare find! It can be yours for just {roundedPrice} gold."
            };

            greeting =
                sellerDialogues[
                    Random.Range(
                        0,
                        sellerDialogues.Length
                    )
                ];

            if (boughtForText != null)
                boughtForText.gameObject.SetActive(false);
        }
        else
        {
            string[] buyerDialogues =
            {
                $"I've been looking everywhere for '{item.itemName}'. I can pay {roundedPrice} gold for it!",
                $"Ah, '{item.itemName}'! I'll give you {roundedPrice} gold for it. Deal?",
                $"That '{item.itemName}' catches my eye. How about {roundedPrice} gold?",
                $"I must have '{item.itemName}' for my collection. Here is {roundedPrice} gold."
            };

            greeting =
                buyerDialogues[
                    Random.Range(
                        0,
                        buyerDialogues.Length
                    )
                ];

            if (boughtForText != null)
            {
                if (purchasePrices.ContainsKey(item))
                {
                    boughtForText.text =
                        $"Bought For: {purchasePrices[item]:0}";

                    boughtForText.gameObject.SetActive(true);
                }
                else
                {
                    boughtForText.gameObject.SetActive(false);
                }
            }
        }

        ShowDialogue(greeting);

        if (intentLabelText != null)
        {
            string raceName =
                newCustomer
                    .customerRace
                    .ToString()
                    .ToUpper();

            if (
                newCustomer.intent ==
                CustomerIntent.SellToPlayer
            )
            {
                intentLabelText.text =
                    $"{raceName} SELLER";

                intentLabelText.color = Color.red;
            }
            else
            {
                intentLabelText.text =
                    $"{raceName} BUYER";

                intentLabelText.color = Color.green;
            }

            intentLabelText.gameObject.SetActive(true);
        }

        if (offerInputField != null)
        {
            offerInputField.text =
                roundedPrice.ToString();
        }

        if (bookTitleText != null)
            bookTitleText.text = item.itemName;

        currentBookValueHidden =
            PlayerSkillEffects.ShouldHideBookValue();

        if (bookStatsText != null)
        {
            if (currentBookValueHidden)
            {
                bookStatsText.text = "???";
            }
            else
            {
                bookStatsText.text =
                    $"{item.basePrice:0}";
            }
        }

        Debug.Log(
            $"[Competence] Level: " +
            $"{PlayerSkillSession.CompetenceLevel}, " +
            $"Book value hidden: " +
            $"{currentBookValueHidden}"
        );

        if (bookIconImage != null)
        {
            bookIconImage.sprite = item.itemIcon;
            bookIconImage.gameObject.SetActive(true);
        }

        if (rareBookEffectController != null)
            rareBookEffectController.PlayEffect(item);

        if (editionText != null)
            editionText.text = item.edition;

        if (conditionText != null)
            conditionText.text = item.conditionString;

        if (rarityText != null)
            rarityText.text = item.rarity.ToString();

        if (magicLevelText != null)
            magicLevelText.text = item.magicLevel;

        if (ageText != null)
            ageText.text = item.age;

        if (curseText != null)
            curseText.text = item.curse;

        currentCustomer.OnDealAccepted +=
            HandleDealAccepted;

        currentCustomer.OnDealRejected +=
            HandleDealRejected;

        currentCustomer.OnDialogueGenerated +=
            HandleCustomerDialogue;

        bool returningBuyerBookUnavailable =
            isReturningCustomer &&
            newCustomer.intent ==
            CustomerIntent.BuyFromPlayer &&
            !requestedBookStillAvailable;

        if (returningBuyerBookUnavailable)
        {
            currentCustomerRequestedBookUnavailable = true;

            ShowDialogue(
                "The book I wanted has already been sold. " +
                "I have no reason to stay."
            );

            if (offerInputField != null)
            {
                offerInputField.text = "";
                offerInputField.interactable = false;
            }

            if (offerButton != null)
                offerButton.interactable = true;

            if (refuseButton != null)
                refuseButton.interactable = true;

            if (increaseButton != null)
                increaseButton.interactable = false;

            if (decreaseButton != null)
                decreaseButton.interactable = false;

            if (laterButton != null)
                laterButton.interactable = true;
        }
        else
        {
            if (offerInputField != null)
                offerInputField.interactable = true;
        }
    }

    private void HandleOfferMouseScroll()
    {
        if (
            offerInputField == null ||
            !offerInputField.interactable ||
            currentCustomer == null ||
            waitingForNextCustomer ||
            currentCustomerRequestedBookUnavailable
        )
        {
            return;
        }

        float scrollAmount = Input.mouseScrollDelta.y;

        if (Mathf.Approximately(scrollAmount, 0f))
            return;

        float currentOffer = GetCurrentOffer();

        if (scrollAmount > 0f)
            currentOffer += mouseScrollIncrement;
        else
            currentOffer -= mouseScrollIncrement;

        currentOffer = Mathf.Max(0f, currentOffer);

        offerInputField.text = currentOffer.ToString("0");
    }

    public void IncreaseOffer()
    {
        if (
            offerInputField == null ||
            waitingForNextCustomer ||
            currentCustomerRequestedBookUnavailable
        )
        {
            return;
        }

        float currentOffer =
            GetCurrentOffer() + offerIncrement;

        offerInputField.text =
            currentOffer.ToString("0");
    }

    public void DecreaseOffer()
    {
        if (
            offerInputField == null ||
            waitingForNextCustomer ||
            currentCustomerRequestedBookUnavailable
        )
        {
            return;
        }

        float currentOffer =
            GetCurrentOffer() - offerIncrement;

        if (currentOffer < 0)
            currentOffer = 0;

        offerInputField.text =
            currentOffer.ToString("0");
    }

    public void SubmitOffer()
    {
        if (
            currentCustomer == null ||
            waitingForNextCustomer
        )
        {
            return;
        }

        if (currentCustomerRequestedBookUnavailable)
        {
            DismissUnavailableReturningCustomer();
            return;
        }

        if (currentCustomer.itemToSell == null)
            return;

        float offer = GetCurrentOffer();

        if (
            currentCustomer.intent ==
            CustomerIntent.SellToPlayer &&
            offer > playerGold
        )
        {
            ShowDialogue(
                "You don't have enough gold."
            );

            return;
        }

        currentCustomer.ReceivePlayerOffer(offer);
    }

    public void LaterCustomer()
    {
        if (
            currentCustomer == null ||
            waitingForNextCustomer
        )
        {
            return;
        }

        if (currentCustomerRequestedBookUnavailable)
            return;

        if (customerSpawner == null)
        {
            customerSpawner =
                Object.FindFirstObjectByType<CustomerSpawner>();
        }

        if (customerSpawner == null)
        {
            Debug.LogError(
                "CustomerSpawner could not be found, " +
                "so the customer cannot be deferred."
            );

            return;
        }

        if (
            !customerSpawner.QueueCurrentCustomerForLater(
                currentCustomer
            )
        )
        {
            return;
        }

        waitingForNextCustomer = true;

        SetButtonsInteractable(false);

        ShowDialogue(
            "Alright, I'll come back later."
        );

        StartCoroutine(
            FinishDeferredCustomerAfterDelay()
        );
    }

    public void RejectCustomer()
    {
        if (
            currentCustomer == null ||
            waitingForNextCustomer
        )
        {
            return;
        }

        if (currentCustomerRequestedBookUnavailable)
        {
            DismissUnavailableReturningCustomer();
            return;
        }

        ShowDialogue("Maybe another time.");

        PlaySfx(_rejectClip, 1.4f);

        DayManager.Instance?.RegisterCustomerServed(
            currentCustomer.customerRace
        );

        DayManager.Instance?.RegisterReputationEarned(
            currentCustomer.customerRace,
            -3f
        );

        FinishCustomer();
    }

    private void DismissUnavailableReturningCustomer()
    {
        if (
            currentCustomer == null ||
            waitingForNextCustomer
        )
        {
            return;
        }

        waitingForNextCustomer = true;

        currentCustomerRequestedBookUnavailable = false;

        SetButtonsInteractable(false);

        if (offerInputField != null)
            offerInputField.interactable = false;

        DayManager.Instance?.RegisterCustomerServed(
            currentCustomer.customerRace
        );

        UnsubscribeFromCurrentCustomer();

        currentCustomer = null;

        StartCoroutine(FinishAfterDelay());
    }

    private float GetCurrentOffer()
    {
        if (offerInputField == null)
            return 0f;

        if (
            float.TryParse(
                offerInputField.text,
                out float value
            )
        )
        {
            return value;
        }

        return 0f;
    }

    private void HandleCustomerDialogue(string text)
    {
        ShowDialogue(text);
    }

    private void HandleDealAccepted(
        ItemData item,
        float finalPrice,
        string dialogue)
    {
        ShowDialogue(dialogue);

        if (
            currentCustomer.intent ==
            CustomerIntent.SellToPlayer
        )
        {
            PlaySfx(_buyBookClip, 1.5f);

            playerGold -= finalPrice;

            purchasedItems.Add(item);

            purchasePrices[item] = finalPrice;

            DayManager.Instance?.RegisterGoldEarned(
                -finalPrice
            );

            DayManager.Instance?.RegisterBookBought();
        }
        else
        {
            PlaySfx(_sellGoldClip, 1.6f);

            playerGold += finalPrice;

            purchasedItems.Remove(item);

            if (purchasePrices.ContainsKey(item))
                purchasePrices.Remove(item);

            DayManager.Instance?.RegisterGoldEarned(
                finalPrice
            );

            DayManager.Instance?.RegisterBookSold();
        }

        SaveGameSessionState();

        DayManager.Instance?.RegisterCustomerServed(
            currentCustomer.customerRace
        );

        float repGain =
            currentCustomer.intent ==
            CustomerIntent.SellToPlayer
                ? 2f
                : 3f;

        DayManager.Instance?.RegisterReputationEarned(
            currentCustomer.customerRace,
            repGain
        );

        if (
            goldCounter != null &&
            goldCounter.gameObject.activeInHierarchy
        )
        {
            goldCounter.UpdateCounter(playerGold);
        }

        FinishCustomer();
    }

    private void HandleDealRejected(string dialogue)
    {
        ShowDialogue(dialogue);

        PlaySfx(
            _rejectClip,
            1.5f,
            0.9f
        );

        DayManager.Instance?.RegisterCustomerServed(
            currentCustomer.customerRace
        );

        DayManager.Instance?.RegisterReputationEarned(
            currentCustomer.customerRace,
            -1f
        );

        FinishCustomer();
    }

    private IEnumerator FinishDeferredCustomerAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        UnsubscribeFromCurrentCustomer();

        if (customerSpawner != null)
            customerSpawner.ClearCurrentCustomer();

        currentCustomer = null;

        ClearUI();

        DayManager.Instance?.OnCustomerDeferred();
    }

    private void UnsubscribeFromCurrentCustomer()
    {
        if (currentCustomer == null)
            return;

        currentCustomer.OnDealAccepted -=
            HandleDealAccepted;

        currentCustomer.OnDealRejected -=
            HandleDealRejected;

        currentCustomer.OnDialogueGenerated -=
            HandleCustomerDialogue;
    }

    private void FinishCustomer()
    {
        waitingForNextCustomer = true;

        currentCustomerRequestedBookUnavailable = false;

        SetButtonsInteractable(false);

        UnsubscribeFromCurrentCustomer();

        currentCustomer = null;

        StartCoroutine(FinishAfterDelay());
    }

    private IEnumerator FinishAfterDelay()
    {
        yield return new WaitForSeconds(2.5f);

        ClearUI();

        DayManager.Instance?.OnCustomerLeft();
    }

    private void ShowDialogue(string message)
    {
        if (dialogueText == null)
            return;

        TypeWriterEffect typeWriter =
            dialogueText.GetComponent<TypeWriterEffect>();

        if (typeWriter != null)
        {
            typeWriter.Play(message);
        }
        else
        {
            dialogueText.text = message;
        }
    }

    private void SetButtonsInteractable(bool value)
    {
        if (offerButton != null)
            offerButton.interactable = value;

        if (increaseButton != null)
            increaseButton.interactable = value;

        if (decreaseButton != null)
            decreaseButton.interactable = value;

        if (refuseButton != null)
            refuseButton.interactable = value;

        if (laterButton != null)
            laterButton.interactable = value;
    }

    private void ClearUI()
    {
        currentCustomerRequestedBookUnavailable = false;

        if (bookTitleText != null)
            bookTitleText.text = "Book Title";

        if (bookStatsText != null)
            bookStatsText.text = "Value:";

        if (bookIconImage != null)
            bookIconImage.gameObject.SetActive(false);

        if (rareBookEffectController != null)
            rareBookEffectController.StopAllEffects();

        if (editionText != null)
            editionText.text = "Edition";

        if (conditionText != null)
            conditionText.text = "Condition";

        if (rarityText != null)
            rarityText.text = "Rarity";

        if (magicLevelText != null)
            magicLevelText.text = "Magic Level";

        if (ageText != null)
            ageText.text = "Age";

        if (curseText != null)
            curseText.text = "Curse";

        if (dialogueText != null)
            dialogueText.text = "";

        if (dialogueGroup != null)
            dialogueGroup.SetActive(false);

        if (offerInputField != null)
        {
            offerInputField.text = "";
            offerInputField.interactable = true;
        }

        if (intentLabelText != null)
            intentLabelText.gameObject.SetActive(false);

        if (boughtForText != null)
            boughtForText.gameObject.SetActive(false);
    }
}