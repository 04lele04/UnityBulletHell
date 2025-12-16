using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("HUD")]
    public TMP_Text hpText;
    public TMP_Text xpText;

    [Header("Notifications")]
    public TMP_Text levelUpText;
    public TMP_Text gameOverText;

    [Header("Upgrade Menu")]
    public GameObject upgradePanel;
    public GameObject upgradeCardPrefab;
    public Transform upgradeCardContainer;

    private Coroutine levelUpCoroutine;
    private List<GameObject> activeUpgradeCards = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (levelUpText != null)
            levelUpText.gameObject.SetActive(false);

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    public void UpdateHP(int hp, int maxHP)
    {
        if (hpText != null)
            hpText.text = $"HP: {hp}/{maxHP}";
    }

    public void UpdateXP(int xp, int xpToNext)
    {
        if (xpText != null)
            xpText.text = $"XP: {xp}/{xpToNext}";
    }

    public void ShowLevelUp()
    {
        if (levelUpCoroutine != null)
        {
            StopCoroutine(levelUpCoroutine);
            levelUpText.gameObject.SetActive(false);
        }
        levelUpCoroutine = StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine()
    {
        if (levelUpText != null)
        {
            levelUpText.gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(1.5f); // Use realtime since game is paused
            levelUpText.gameObject.SetActive(false);
        }
        levelUpCoroutine = null;
    }

    public void ShowUpgradeMenu()
    {
        // Fallback se upgrade system non è configurato
        if (upgradePanel == null || upgradeCardPrefab == null || upgradeCardContainer == null)
        {
            Debug.LogWarning("Upgrade Panel not configured!");
            ShowLevelUp();
            Time.timeScale = 1f;
            return;
        }

        // Usa riferimento sicuro da GameManager se Instance non è pronto
        CardGenerator cg = CardGenerator.Instance ?? GameManager.Instance?.cardGenerator;

        if (cg == null)
        {
            Debug.LogError("CardGenerator not found!");
            Time.timeScale = 1f;
            return;
        }

        // Genera le carte usando il riferimento sicuro
        List<UpgradeCard> cards = cg.GenerateCards(3);

        // Pulizia delle carte precedenti
        foreach (GameObject cardObj in activeUpgradeCards)
        {
            Destroy(cardObj);
        }
        activeUpgradeCards.Clear();

        if (cards.Count == 0)
        {
            Debug.LogWarning("No upgrade cards available!");
            Time.timeScale = 1f;
            return;
        }

        // Crea la UI delle carte
        foreach (UpgradeCard card in cards)
        {
            GameObject cardObj = Instantiate(upgradeCardPrefab, upgradeCardContainer);

            TMP_Text cardText = cardObj.GetComponentInChildren<TMP_Text>();
            if (cardText != null)
                cardText.text = card.displayText;

            Image cardIcon = cardObj.transform.Find("Icon")?.GetComponent<Image>();
            if (cardIcon != null && card.icon != null)
                cardIcon.sprite = card.icon;

            Button button = cardObj.GetComponent<Button>();
            if (button != null)
            {
                UpgradeCard selectedCard = card; // cattura la carta nel closure
                button.onClick.AddListener(() => OnCardSelected(selectedCard));
            }

            activeUpgradeCards.Add(cardObj);
        }

        upgradePanel.SetActive(true);
    }
    void OnCardSelected(UpgradeCard card)
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        // Apply the card effect
        CardGenerator.Instance.ApplyCard(card);

        // Notify player if weapon was unlocked (to refresh timers)
        if (card.cardType == CardType.WeaponUnlock)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
                player.OnWeaponUnlocked();
        }

        // Resume game
        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        if (levelUpCoroutine != null)
        {
            StopCoroutine(levelUpCoroutine);
            levelUpCoroutine = null;
        }

        if (levelUpText != null)
            levelUpText.gameObject.SetActive(false);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (gameOverText != null)
        {
            gameOverText.text = $"GAME OVER\n\nNemici uccisi: {Enemy.enemiesKilled}\n\nPremi R per riavviare";
            gameOverText.gameObject.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}