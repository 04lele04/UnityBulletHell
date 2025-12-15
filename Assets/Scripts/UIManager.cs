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
    public TMP_Text xpText; // NUOVO - aggiungi questo nell'Inspector
    
    [Header("Notifications")]
    public TMP_Text levelUpText;
    public TMP_Text gameOverText;
    
    [Header("Upgrade Menu")]
    public GameObject upgradePanel;
    public GameObject upgradeButtonPrefab;
    public Transform upgradeButtonContainer;
    
    private Coroutine levelUpCoroutine;
    private List<GameObject> activeUpgradeButtons = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        levelUpText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
        
        // Nascondi il panel all'inizio
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    // METODO VECCHIO (compatibile con il tuo codice originale)
    public void UpdateHP(int hp)
    {
        hpText.text = $"HP: {hp}";
    }
    
    // METODO NUOVO (overload con maxHP)
    public void UpdateHP(int hp, int maxHP)
    {
        hpText.text = $"HP: {hp}/{maxHP}";
    }
    
    // NUOVO METODO per aggiornare XP
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
        levelUpText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        levelUpText.gameObject.SetActive(false);
        levelUpCoroutine = null;
    }

    // NUOVO METODO per mostrare il menu degli upgrade
    public void ShowUpgradeMenu()
    {
        // Se non hai ancora configurato il panel, mostra solo il level up text
        if (upgradePanel == null || upgradeButtonPrefab == null || upgradeButtonContainer == null)
        {
            Debug.LogWarning("Upgrade Panel non configurato! Configura i riferimenti nell'Inspector.");
            ShowLevelUp();
            Time.timeScale = 1f; // Riprendi subito il gioco
            return;
        }
        
        // Pulisci bottoni precedenti
        foreach (GameObject btn in activeUpgradeButtons)
        {
            Destroy(btn);
        }
        activeUpgradeButtons.Clear();
        
        // Scegli 3 upgrade casuali
        List<UpgradeType> availableUpgrades = new List<UpgradeType>
        {
            UpgradeType.Damage,
            UpgradeType.Speed,
            UpgradeType.Health,
            UpgradeType.FireRate,
            UpgradeType.ProjectileCount
        };
        
        // Mescola la lista
        for (int i = 0; i < availableUpgrades.Count; i++)
        {
            UpgradeType temp = availableUpgrades[i];
            int randomIndex = Random.Range(i, availableUpgrades.Count);
            availableUpgrades[i] = availableUpgrades[randomIndex];
            availableUpgrades[randomIndex] = temp;
        }
        
        int optionsToShow = Mathf.Min(3, availableUpgrades.Count);
        
        for (int i = 0; i < optionsToShow; i++)
        {
            UpgradeType upgradeType = availableUpgrades[i];
            GameObject btn = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
            
            // Configura il bottone
            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = GetUpgradeDescription(upgradeType);
            
            Button button = btn.GetComponent<Button>();
            if (button != null)
                button.onClick.AddListener(() => OnUpgradeSelected(upgradeType));
            
            activeUpgradeButtons.Add(btn);
        }
        
        upgradePanel.SetActive(true);
    }

    string GetUpgradeDescription(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                return "⚔️ DANNO +1\nColpisci più forte";
            case UpgradeType.Speed:
                return "⚡ VELOCITÀ +1\nMuoviti più velocemente";
            case UpgradeType.Health:
                return "❤️ VITA +1\nAumenta HP massimi e cura";
            case UpgradeType.FireRate:
                return "🔫 CADENZA +\nSpara più velocemente";
            case UpgradeType.ProjectileCount:
                return "🌟 PROIETTILI +1\nSpara più proiettili";
            default:
                return "Upgrade";
        }
    }

    void OnUpgradeSelected(UpgradeType type)
    {
        upgradePanel.SetActive(false);
        
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.ApplyUpgrade(type);
        }
    }

    public void ShowGameOver()
    {
        if (levelUpCoroutine != null)
        {
            StopCoroutine(levelUpCoroutine);
            levelUpCoroutine = null;
        }
        
        levelUpText.gameObject.SetActive(false);
        
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
        
        // Mostra stats finali
        gameOverText.text = $"GAME OVER\n\nNemici uccisi: {Enemy.enemiesKilled}\n\nPremi R per riavviare";
        gameOverText.gameObject.SetActive(true);
        
        Time.timeScale = 0f;
    }
}