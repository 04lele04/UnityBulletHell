using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("XP")]
    public int level = 1;
    public int xp;
    public int xpToNextLevel = 5;

    [Header("Base stats")]
    public int baseMaxHP = 3;
    public float baseSpeed = 6f;

    [Header("Current Stats (READ ONLY)")]
    [SerializeField] private int maxHP;
    [SerializeField] private int hp;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float currentDamageMultiplier;
    [SerializeField] private float currentFireRateReduction;
    [SerializeField] private int currentBonusProjectiles;

    Rigidbody2D rb;
    CharacterManager cm;
    CharacterData character;

    List<float> weaponFireTimers = new();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cm = CharacterManager.Instance;
        character = cm.GetSelectedCharacter();

        maxHP = baseMaxHP;
        hp = maxHP;

        RecalculateStats();
        RefreshWeaponTimers();

        UIManager.Instance.UpdateHP(hp, maxHP);
        UIManager.Instance.UpdateXP(xp, xpToNextLevel);
    }

    void Update()
    {
        Move();
        ShootAllWeapons();
    }

    // ✅ RICALCOLA TUTTE LE STAT (chiamala dopo ogni upgrade!)
    public void RecalculateStats()
    {
        // Health (ADDITIVO: +1 per livello)
        float healthBonus = cm.GetStatBonus(StatType.Health);
        int oldMaxHP = maxHP;
        maxHP = baseMaxHP + Mathf.RoundToInt(healthBonus);

        // Se maxHP aumenta, cura proporzionalmente
        if (maxHP > oldMaxHP)
        {
            float hpRatio = (float)hp / oldMaxHP;
            hp = Mathf.RoundToInt(maxHP * hpRatio);
        }
        hp = Mathf.Min(hp, maxHP);

        // Speed (MOLTIPLICATIVO: x1.5 per livello)
        float speedBonus = cm.GetStatBonus(StatType.Speed);
        currentSpeed = baseSpeed * (1f + speedBonus);

        // Damage (MOLTIPLICATIVO: x1.5 per livello)
        currentDamageMultiplier = 1f + cm.GetStatBonus(StatType.Damage);

        // Fire Rate (RIDUZIONE: -33% per livello, max 80%)
        currentFireRateReduction = cm.GetStatBonus(StatType.FireRate);

        // Projectiles (ADDITIVO: +1 per livello)
        currentBonusProjectiles = Mathf.RoundToInt(
            cm.GetStatBonus(StatType.ProjectileQuantity));

        UIManager.Instance.UpdateHP(hp, maxHP);

        Debug.Log($"📊 Stats Updated | HP: {hp}/{maxHP} (+{healthBonus:F0}) | Speed: {currentSpeed:F1} (x{1f + speedBonus:F2}) | Dmg: x{currentDamageMultiplier:F2} | FireRate: -{currentFireRateReduction * 100:F0}% | Projectiles: +{currentBonusProjectiles}");
    }

    // ---------------- MOVE ----------------
    void Move()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        rb.linearVelocity = input * currentSpeed;
    }

    // ---------------- SHOOT ----------------
    void ShootAllWeapons()
    {
        var weapons = cm.GetEquippedWeapons();

        while (weaponFireTimers.Count < weapons.Count)
            weaponFireTimers.Add(0f);

        for (int i = 0; i < weapons.Count; i++)
        {
            weaponFireTimers[i] -= Time.deltaTime;

            if (weaponFireTimers[i] <= 0f)
            {
                weaponFireTimers[i] = weapons[i].fireRate * (1f - currentFireRateReduction);
                FireWeapon(weapons[i]);
            }
        }
    }

    void FireWeapon(WeaponInstance weapon)
    {
        Enemy target = FindNearestEnemy();
        if (target == null) return;

        Vector2 baseDir = (target.transform.position - transform.position).normalized;

        int totalProjectiles = weapon.projectileCount + currentBonusProjectiles;

        for (int i = 0; i < totalProjectiles; i++)
        {
            float angle = totalProjectiles > 1
                ? (-weapon.spreadAngle * (totalProjectiles - 1) / 2f) + weapon.spreadAngle * i
                : 0f;

            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;

            PlayerBullet bullet = Instantiate(
                weapon.data.bulletPrefab,
                transform.position,
                Quaternion.identity).GetComponent<PlayerBullet>();

            bullet.damage = Mathf.RoundToInt(weapon.damage * currentDamageMultiplier);
            bullet.speed = weapon.bulletSpeed;
            bullet.Init(dir);
        }
    }

    // ---------------- HEALTH ----------------
    public void TakeDamage()
    {
        hp--;
        UIManager.Instance.UpdateHP(hp, maxHP);

        if (hp <= 0)
        {
            UIManager.Instance.ShowGameOver();
            Time.timeScale = 0f;
        }
    }

    // ---------------- XP ----------------
    public void GainXP(int amount)
    {
        xp += amount;
        UIManager.Instance.UpdateXP(xp, xpToNextLevel);

        if (xp >= xpToNextLevel)
            LevelUp();
    }

    void LevelUp()
    {
        level++;
        xp = 0;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f);

        UIManager.Instance.ShowUpgradeMenu();
        Time.timeScale = 0f;
    }

    // ---------------- UTILS ----------------
    Enemy FindNearestEnemy()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Enemy nearest = null;
        float minDist = Mathf.Infinity;

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

    void RefreshWeaponTimers()
    {
        weaponFireTimers.Clear();
        foreach (var _ in cm.GetEquippedWeapons())
            weaponFireTimers.Add(0f);
    }

    public void OnWeaponUnlocked()
    {
        RefreshWeaponTimers();
    }
}