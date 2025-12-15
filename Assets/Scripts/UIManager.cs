using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TMP_Text hpText;
    public TMP_Text levelUpText;
    public TMP_Text gameOverText;

    private Coroutine levelUpCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        levelUpText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false); 
    }

    public void UpdateHP(int hp)
    {
        hpText.text = $"HP: {hp}";
    }

    public void ShowLevelUp()
    {
        // Cancel any existing LevelUp coroutine
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

    public void ShowGameOver()
    {
        // Hide LevelUp text immediately
        if (levelUpCoroutine != null)
        {
            StopCoroutine(levelUpCoroutine);
            levelUpCoroutine = null;
        }
        levelUpText.gameObject.SetActive(false);

        // Show GameOver text
        gameOverText.gameObject.SetActive(true);

        // Stop the game
        Time.timeScale = 0f;
    }
}
