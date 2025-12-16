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
    public List<WeaponInstance> equippedWeapons = new();
    private List<StatInstance> characterStats = new();

    [Header("All available weapons and upgrades")]
    public List<WeaponData> allWeapons;
    public List<WeaponUpgradeData> allWeaponUpgrades;

    [Header("All available stats")]
    public List<StatData> allStats;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStats();
        }
        else Destroy(gameObject);
    }

    void InitializeStats()
    {
        characterStats.Clear();

        foreach (var stat in allStats)
        {
            if (stat != null)
                characterStats.Add(new StatInstance(stat));
        }

        Debug.Log($"✅ Initialized {characterStats.Count} stats");
    }

    // ---------------- CHARACTER ----------------
    public void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
        equippedWeapons.Clear();

        if (character?.starterWeapon != null)
            equippedWeapons.Add(new WeaponInstance(character.starterWeapon));
    }

    public CharacterData GetSelectedCharacter() => selectedCharacter;

    // ---------------- WEAPONS ----------------
    public List<WeaponInstance> GetEquippedWeapons() => equippedWeapons;
    public bool CanEquipMoreWeapons() => equippedWeapons.Count < 3;
    public bool HasWeapon(WeaponData weapon) => equippedWeapons.Any(w => w.data == weapon);

    public void UnlockWeapon(WeaponData weapon)
    {
        if (!HasWeapon(weapon) && CanEquipMoreWeapons())
            equippedWeapons.Add(new WeaponInstance(weapon));
    }

    public void UpgradeWeapon(WeaponData weapon, WeaponUpgradeType type)
    {
        WeaponInstance w = equippedWeapons.Find(x => x.data == weapon);
        if (w == null) return;

        switch (type)
        {
            case WeaponUpgradeType.Damage: w.damage += weapon.damagePerUpgrade; break;
            case WeaponUpgradeType.ProjectileCount: w.projectileCount += weapon.projectileCountPerUpgrade; break;
            case WeaponUpgradeType.FireRate: w.fireRate *= 0.9f; break;
        }
    }

    // ---------------- STATS ----------------
    public List<StatInstance> GetCharacterStats() => characterStats;

    public bool CanUnlockMoreStats() => characterStats.Any(s => !s.isUnlocked);

    public void UnlockStat(StatData data)
    {
        StatInstance stat = characterStats.Find(s => s.data == data);
        if (stat == null || stat.isUnlocked) return;

        stat.isUnlocked = true;
        stat.upgradeLevel = 1;
    }

    public void UpgradeStat(StatData data)
    {
        StatInstance stat = characterStats.Find(s => s.data == data);
        if (stat == null) return;

        if (!stat.isUnlocked)
        {
            stat.isUnlocked = true;
            stat.upgradeLevel = 1;
        }
        else stat.upgradeLevel++;
    }

    // BONUS grezzo (0 se locked)
    public float GetStatBonus(StatType type)
    {
        StatInstance stat = characterStats.Find(s => s.data.statType == type);
        return (stat != null && stat.isUnlocked) ? stat.GetBonus() : 0f;
    }
}
