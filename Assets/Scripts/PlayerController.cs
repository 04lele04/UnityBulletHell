using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 6f;
    public int maxHP = 3;
    public int hp;

    public GameObject bulletPrefab;
    public float fireRate = 0.15f;

    // XP / LEVEL
    public int level = 1;
    public int xp = 0;
    public int xpToNextLevel = 5;

    float fireTimer;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = maxHP;

        UIManager.Instance.UpdateHP(hp);
    }

    void Update()
    {
        Move();
        Shoot();

        // Replay if game is over
        if (Time.timeScale == 0f && Input.GetKeyDown(KeyCode.R))
        {
            ReplayGame();
        }
    }

    void Move()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        rb.linearVelocity = input * speed; // Changed linearVelocity → velocity
    }

    void Shoot()
    {
        fireTimer -= Time.unscaledDeltaTime; // Use unscaledDeltaTime so shooting stops on pause

        if (fireTimer <= 0f)
        {
            fireTimer = fireRate;

            Enemy target = FindNearestEnemy();
            if (target != null)
            {
                Vector2 dir =
                    (target.transform.position - transform.position).normalized;

                Instantiate(bulletPrefab, transform.position, Quaternion.identity)
                    .GetComponent<PlayerBullet>()
                    .Init(dir);
            }
        }
    }

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float minDist = Mathf.Infinity;
        Enemy nearest = null;

        foreach (Enemy e in enemies)
        {
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = e;
            }
        }

        return nearest;
    }

    public void GainXP(int amount)
    {
        xp += amount;
        Debug.Log($"XP GAINED: +{amount} ({xp}/{xpToNextLevel})");

        if (xp >= xpToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
        level++;
        xp = 0;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        speed += 0.5f;

        UIManager.Instance.ShowLevelUp();
    }

    public void TakeDamage()
    {
        hp--;
        UIManager.Instance.UpdateHP(hp);

        if (hp <= 0)
        {
            Debug.Log("PLAYER DEAD — GAME OVER");
            UIManager.Instance.ShowGameOver();

            // Stop the game
            Time.timeScale = 0f;
        }
    }

    void ReplayGame()
    {
        // Reset Time.timeScale so game is not paused
        Time.timeScale = 1f;

        // Optional: Reset XP & level when replaying
        level = 1;
        xp = 0;
        xpToNextLevel = 5;
        speed = 6f;

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
