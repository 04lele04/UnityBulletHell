using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("Characters")]
    public List<CharacterData> availableCharacters;

    private CharacterData selectedCharacter;

    [Header("Runtime Data")]
    public List<WeaponInstance> equippedWeapons = new List<WeaponInstance>();
    private List<StatInstance> characterStats = new List<StatInstance>();

    [Header("All available weapons and upgrades")]
    public List<WeaponData> allWeapons;
    public List<WeaponUpgradeData> allWeaponUpgrades;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeStats()
    {
        // Crea le StatInstance per ogni StatData disponibile (es. Damage, Speed, FireRate, ProjectileQuantity, Health)
        StatData[] allStats = Resources.LoadAll<StatData>("Stats"); // oppure lista statica se le hai
        foreach (StatData stat in allStats)
        {
            characterStats.Add(new StatInstance(stat));
        }
    }

    /// <summary>
    /// Selects a character and initializes its starting weapon.
    /// </summary>
    public void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
        InitializeSelectedCharacter();
    }

    /// <summary>
    /// Returns the currently selected character.
    /// </summary>
    public CharacterData GetSelectedCharacter() => selectedCharacter;

    /// <summary>
    /// Equip the selected character's starter weapon.
    /// </summary>
    public void InitializeSelectedCharacter()
    {
        equippedWeapons.Clear();

        if (selectedCharacter != null && selectedCharacter.starterWeapon != null)
        {
            equippedWeapons.Add(new WeaponInstance(selectedCharacter.starterWeapon));
        }
    }

    /// <summary>
    /// Returns the currently equipped weapons.
    /// </summary>
    public List<WeaponInstance> GetEquippedWeapons() => equippedWeapons;

    // ---------------- Weapon Management ----------------
    public bool CanEquipMoreWeapons() => equippedWeapons.Count < 3; // esempio: massimo 3 armi

    public bool HasWeapon(WeaponData weapon) => equippedWeapons.Any(w => w.data == weapon);

    public void UnlockWeapon(WeaponData weapon)
    {
        if (!HasWeapon(weapon) && CanEquipMoreWeapons())
        {
            equippedWeapons.Add(new WeaponInstance(weapon));
            Debug.Log($"Weapon unlocked: {weapon.weaponName}");
        }
    }

    public void UpgradeWeapon(WeaponData weapon, WeaponUpgradeType type)
    {
        WeaponInstance w = equippedWeapons.Find(x => x.data == weapon);
        if (w == null) return;

        switch (type)
        {
            case WeaponUpgradeType.Damage: w.damage += weapon.damagePerUpgrade; break;
            case WeaponUpgradeType.ProjectileCount: w.projectileCount += weapon.projectileCountPerUpgrade; break;
            case WeaponUpgradeType.FireRate: w.fireRate *= 0.9f; break; // 10% faster
        }

        Debug.Log($"Weapon {weapon.weaponName} upgraded: {type}");
    }

    // ---------------- Stat Management ----------------
    public List<StatInstance> GetCharacterStats() => characterStats;

    public bool CanUnlockMoreStats() => characterStats.Count(s => !s.isUnlocked) > 0;

    public void UnlockStat(StatData statData)
    {
        StatInstance stat = characterStats.Find(s => s.data == statData);
        if (stat != null && !stat.isUnlocked)
        {
            stat.isUnlocked = true;
            stat.upgradeLevel = 1; // parte già con 1 livello
            Debug.Log($"Stat unlocked: {stat.data.statType}");
        }
    }

    public void UpgradeStat(StatData statData)
    {
        StatInstance stat = characterStats.Find(s => s.data == statData);
        if (stat != null && stat.isUnlocked)
        {
            stat.upgradeLevel++;
            Debug.Log($"Stat upgraded: {stat.data.statType}, level {stat.upgradeLevel}");
        }
    }

    public float GetStatMultiplier(StatType type)
    {
        StatInstance stat = characterStats.Find(s => s.data.statType == type);
        return stat != null ? stat.GetValue() : 0f;
    }
}
