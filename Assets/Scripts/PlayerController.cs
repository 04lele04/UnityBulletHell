using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    
    [Header("Health")]
    public int maxHP = 3;
    public int hp;
    
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public float fireRate = 0.15f;
    public int bulletDamage = 1;
    public int projectileCount = 1; // Numero di proiettili per raffica
    public float spreadAngle = 15f; // Angolo di spread tra i proiettili
    
    [Header("XP / Level")]
    public int level = 1;
    public int xp = 0;
    public int xpToNextLevel = 5;
    
    float fireTimer;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hp = maxHP;
        UIManager.Instance.UpdateHP(hp, maxHP);
    }

    void Update()
    {
        Move();
        Shoot();
        
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
        rb.linearVelocity = input * speed;
    }

    void Shoot()
    {
        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            fireTimer = fireRate;
            Enemy target = FindNearestEnemy();
            if (target != null)
            {
                Vector2 baseDir = (target.transform.position - transform.position).normalized;
                
                // Spara multipli proiettili con spread
                for (int i = 0; i < projectileCount; i++)
                {
                    float angle = 0f;
                    
                    if (projectileCount > 1)
                    {
                        // Calcola l'angolo per ogni proiettile
                        float totalSpread = spreadAngle * (projectileCount - 1);
                        angle = -totalSpread / 2 + (spreadAngle * i);
                    }
                    
                    Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
                    
                    PlayerBullet bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity)
                        .GetComponent<PlayerBullet>();
                    bullet.damage = bulletDamage;
                    bullet.Init(dir);
                }
            }
        }
    }

    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
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
        UIManager.Instance.UpdateXP(xp, xpToNextLevel);
        Debug.Log($"XP GAINED: +{amount} ({xp}/{xpToNextLevel})");
        
        if (xp >= xpToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
        level++;
        xp = 0;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);
        
        UIManager.Instance.UpdateXP(xp, xpToNextLevel);
        UIManager.Instance.ShowUpgradeMenu();
        
        // Pausa il gioco durante la scelta
        Time.timeScale = 0f;
    }

    // Metodi per applicare i potenziamenti
    public void ApplyUpgrade(UpgradeType type)
    {
        switch (type)
        {
            case UpgradeType.Damage:
                bulletDamage += 1;
                Debug.Log($"Danno aumentato a {bulletDamage}");
                break;
                
            case UpgradeType.Speed:
                speed += 1f;
                Debug.Log($"Velocità aumentata a {speed}");
                break;
                
            case UpgradeType.Health:
                maxHP += 1;
                hp = maxHP; // Cura completamente
                UIManager.Instance.UpdateHP(hp, maxHP);
                Debug.Log($"HP massimi aumentati a {maxHP}");
                break;
                
            case UpgradeType.FireRate:
                fireRate = Mathf.Max(0.05f, fireRate - 0.02f);
                Debug.Log($"Fire rate migliorato a {fireRate}s");
                break;
                
            case UpgradeType.ProjectileCount:
                projectileCount += 1;
                Debug.Log($"Proiettili per raffica: {projectileCount}");
                break;
        }
        
        // Riprendi il gioco
        Time.timeScale = 1f;
    }

    public void TakeDamage()
    {
        hp--;
        UIManager.Instance.UpdateHP(hp, maxHP);
        
        if (hp <= 0)
        {
            Debug.Log("PLAYER DEAD — GAME OVER");
            UIManager.Instance.ShowGameOver();
            Time.timeScale = 0f;
        }
    }

    void ReplayGame()
    {
        Time.timeScale = 1f;
        Enemy.enemiesKilled = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

public enum UpgradeType
{
    Damage,
    Speed,
    Health,
    FireRate,
    ProjectileCount
}