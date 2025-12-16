using UnityEngine;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    [Header("Characters")]
    public List<CharacterData> availableCharacters;
    private CharacterData selectedCharacter;

    void Awake()
    {
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

    public void SelectCharacter(CharacterData character)
    {
        selectedCharacter = character;
    }

    public CharacterData GetSelectedCharacter()
    {
        return selectedCharacter;
    }

    // 🔧 Placeholder methods so the project compiles
    public bool CanEquipMoreWeapons() => true;
    public bool HasWeapon(WeaponData weapon) => false;
    public void UnlockWeapon(WeaponData weapon) {}
    public void UpgradeWeapon(WeaponData weapon, WeaponUpgradeType type) {}

    public List<WeaponInstance> GetEquippedWeapons() => new();
    public List<StatInstance> GetCharacterStats() => new();

    public bool CanUnlockMoreStats() => true;
    public void UnlockStat(StatData stat) {}
    public void UpgradeStat(StatData stat) {}

    public float GetStatMultiplier(StatType type) => 0f;

    public List<WeaponData> allWeapons;
    public List<WeaponUpgradeData> allWeaponUpgrades;
}
