using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    private bool isGameOver = false;

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

        isGameOver = false;
    }

    void Update()
    {
        // ✅ Controlla R per restart anche quando Time.timeScale = 0
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
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
            yield return new WaitForSecondsRealtime(1.5f);
            levelUpText.gameObject.SetActive(false);
        }
        levelUpCoroutine = null;
    }

    public void ShowUpgradeMenu()
    {
        if (upgradePanel == null || upgradeCardPrefab == null || upgradeCardContainer == null)
        {
            Debug.LogWarning("Upgrade Panel not configured!");
            ShowLevelUp();
            Time.timeScale = 1f;
            return;
        }

        CardGenerator cg = CardGenerator.Instance ?? GameManager.Instance?.cardGenerator;

        if (cg == null)
        {
            Debug.LogError("CardGenerator not found!");
            Time.timeScale = 1f;
            return;
        }

        List<UpgradeCard> cards = cg.GenerateCards(3);

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
                UpgradeCard selectedCard = card;
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

        CardGenerator.Instance.ApplyCard(card);

        if (card.cardType == CardType.WeaponUnlock)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
                player.OnWeaponUnlocked();
        }

        Time.timeScale = 1f;
    }

    public void ShowGameOver()
    {
        isGameOver = true;

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

    void RestartGame()
    {
        Debug.Log("🔄 Restarting game...");

        // Reset time scale
        Time.timeScale = 1f;

        // Reset enemy counter
        Enemy.enemiesKilled = 0;

        // ✅ DISTRUGGI I MANAGER PERSISTENTI (così vengono ricreati freschi)
        if (CharacterManager.Instance != null)
            Destroy(CharacterManager.Instance.gameObject);

        if (CardGenerator.Instance != null)
            Destroy(CardGenerator.Instance.gameObject);

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}