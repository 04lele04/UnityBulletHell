using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("Characters")]
    public List<CharacterData> availableCharacters;

    private CharacterData selectedCharacter;

    [Header("Runtime Data")]
    public List<WeaponInstance> equippedWeapons = new List<WeaponInstance>();

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
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
    public CharacterData GetSelectedCharacter()
    {
        return selectedCharacter;
    }

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

    // ---------------- Placeholder / stub methods ----------------
    public bool CanEquipMoreWeapons() => true;
    public bool HasWeapon(WeaponData weapon) => false;
    public void UnlockWeapon(WeaponData weapon) { }
    public void UpgradeWeapon(WeaponData weapon, WeaponUpgradeType type) { }

    public List<StatInstance> GetCharacterStats() => new();
    public bool CanUnlockMoreStats() => true;
    public void UnlockStat(StatData stat) { }
    public void UpgradeStat(StatData stat) { }
    public float GetStatMultiplier(StatType type) => 0f;

    [Header("All available weapons and upgrades")]
    public List<WeaponData> allWeapons;
    public List<WeaponUpgradeData> allWeaponUpgrades;
}
