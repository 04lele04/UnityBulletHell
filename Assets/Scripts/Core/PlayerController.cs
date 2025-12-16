using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("XP / Level")]
    public int level = 1;
    public int xp = 0;
    public int xpToNextLevel = 5;

    [Header("Health (from character)")]
    private int maxHP = 3;
    private int hp;

    [Header("Movement (from character)")]
    private float baseSpeed = 6f;

    // Weapon firing
    private List<float> weaponFireTimers = new List<float>();

    Rigidbody2D rb;
    CharacterManager characterManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        characterManager = CharacterManager.Instance;

        if (characterManager == null)
        {
            Debug.LogError("CharacterManager not found! Cannot initialize player.");
            enabled = false;
            return;
        }

        // Initialize from selected character
        CharacterData character = characterManager.GetSelectedCharacter();
        if (character != null)
        {
            maxHP = character.baseMaxHP;
            baseSpeed = character.baseSpeed;
            hp = maxHP;

            Debug.Log($"Playing as: {character.characterName}");
            Debug.Log($"Passive: {character.passiveDescription}");
        }
        else
        {
            Debug.LogError("No character selected!");
        }

        // Initialize weapon timers
        RefreshWeaponTimers();

        UIManager.Instance.UpdateHP(hp, maxHP);
        UIManager.Instance.UpdateXP(xp, xpToNextLevel);
    }

    void Update()
    {
        Move();
        ShootAllWeapons();

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

        // Apply speed stat multiplier
        float speedMultiplier = 1f + characterManager.GetStatMultiplier(StatType.Speed);
        float finalSpeed = baseSpeed * speedMultiplier;

        rb.linearVelocity = input * finalSpeed;
    }

    void ShootAllWeapons()
    {
        List<WeaponInstance> weapons = characterManager.GetEquippedWeapons();

        // Ensure we have enough timers
        while (weaponFireTimers.Count < weapons.Count)
        {
            weaponFireTimers.Add(0f);
        }

        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponInstance weapon = weapons[i];
            weaponFireTimers[i] -= Time.deltaTime;

            if (weaponFireTimers[i] <= 0f)
            {
                // Apply fire rate stat multiplier (lower is better)
                float fireRateMultiplier = 1f - characterManager.GetStatMultiplier(StatType.FireRate);
                fireRateMultiplier = Mathf.Max(0.1f, fireRateMultiplier); // Prevent zero or negative

                weaponFireTimers[i] = weapon.fireRate * fireRateMultiplier;
                FireWeapon(weapon);
            }
        }
    }

    void FireWeapon(WeaponInstance weapon)
    {
        Enemy target = FindNearestEnemy();
        if (target == null) return;

        Vector2 baseDir = (target.transform.position - transform.position).normalized;

        // Apply projectile quantity stat (additive)
        int bonusProjectiles = Mathf.RoundToInt(characterManager.GetStatMultiplier(StatType.ProjectileQuantity));
        int totalProjectiles = weapon.projectileCount + bonusProjectiles;

        // Shoot multiple projectiles with spread
        for (int i = 0; i < totalProjectiles; i++)
        {
            float angle = 0f;

            if (totalProjectiles > 1)
            {
                float totalSpread = weapon.spreadAngle * (totalProjectiles - 1);
                angle = -totalSpread / 2 + (weapon.spreadAngle * i);
            }

            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;

            PlayerBullet bullet = Instantiate(weapon.data.bulletPrefab, transform.position, Quaternion.identity)
                .GetComponent<PlayerBullet>();

            if (bullet != null)
            {
                // Apply damage stat multiplier (percentage increase)
                float damageMultiplier = 1f + characterManager.GetStatMultiplier(StatType.Damage);
                int finalDamage = Mathf.RoundToInt(weapon.damage * damageMultiplier);

                bullet.damage = finalDamage;
                bullet.speed = weapon.bulletSpeed;
                bullet.Init(dir);
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

        // Pause the game during upgrade selection
        Time.timeScale = 0f;
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

    // Helper to refresh weapon timers when weapons are added
    void RefreshWeaponTimers()
    {
        List<WeaponInstance> weapons = characterManager.GetEquippedWeapons();
        weaponFireTimers.Clear();

        foreach (WeaponInstance weapon in weapons)
        {
            weaponFireTimers.Add(0f); // Start ready to fire
        }
    }

    // Called by UIManager when a weapon is unlocked
    public void OnWeaponUnlocked()
    {
        RefreshWeaponTimers();
    }
}