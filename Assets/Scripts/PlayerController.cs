using UnityEngine;

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

        Debug.Log($"PLAYER HP: {hp}");
        Debug.Log($"LEVEL: {level} | XP: {xp}/{xpToNextLevel}");
    }

    void Update()
    {
        Move();
        Shoot();
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

        // TEMP upgrade
        speed += 0.5f;

        Debug.Log($"LEVEL UP! → LEVEL {level}");
        Debug.Log($"NEXT LEVEL XP: {xpToNextLevel}");
    }

    public void TakeDamage()
    {
        hp--;
        Debug.Log($"PLAYER HP: {hp}");

        if (hp <= 0)
        {
            Debug.Log("PLAYER DEAD — GAME OVER");
            Destroy(gameObject);
            Time.timeScale = 0f;
        }
    }
}
