using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CardGenerator : MonoBehaviour
{
    public static CardGenerator Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public List<UpgradeCard> GenerateCards(int count)
    {
        List<UpgradeCard> possibleCards = new List<UpgradeCard>();
        CharacterManager cm = CharacterManager.Instance;
        
        if (cm == null)
        {
            Debug.LogError("CharacterManager not found!");
            return possibleCards;
        }
        
        // 1. Weapon Unlock Cards (only if can equip more)
        if (cm.CanEquipMoreWeapons())
        {
            foreach (WeaponData weapon in cm.allWeapons)
            {
                if (!cm.HasWeapon(weapon))
                {
                    UpgradeCard card = new UpgradeCard(
                        CardType.WeaponUnlock,
                        $"🔓 {weapon.weaponName.ToUpper()}\n{weapon.description}",
                        weapon.icon
                    );
                    card.weaponToUnlock = weapon;
                    possibleCards.Add(card);
                }
            }
        }
        
        // 2. Weapon Upgrade Cards (only for owned weapons)
        foreach (WeaponInstance weapon in cm.GetEquippedWeapons())
        {
            // Find all upgrades for this weapon
            var upgrades = cm.allWeaponUpgrades.Where(u => u.targetWeapon == weapon.data);
            
            foreach (WeaponUpgradeData upgrade in upgrades)
            {
                string displayText = GetWeaponUpgradeDisplay(weapon, upgrade.upgradeType);
                
                UpgradeCard card = new UpgradeCard(
                    CardType.WeaponUpgrade,
                    displayText,
                    upgrade.icon ?? weapon.data.icon
                );
                card.weaponUpgrade = upgrade;
                possibleCards.Add(card);
            }
        }
        
        // 3. Stat Unlock Cards (only if can unlock more)
        if (cm.CanUnlockMoreStats())
        {
            foreach (StatInstance stat in cm.GetCharacterStats())
            {
                if (!stat.isUnlocked)
                {
                    UpgradeCard card = new UpgradeCard(
                        CardType.StatUnlock,
                        $"🌟 {stat.data.unlockDisplayText}\n{stat.data.description}",
                        stat.data.icon
                    );
                    card.statUpgrade = stat.data;
                    possibleCards.Add(card);
                }
            }
        }
        
        // 4. Stat Upgrade Cards (only for unlocked stats)
        foreach (StatInstance stat in cm.GetCharacterStats())
        {
            if (stat.isUnlocked)
            {
                UpgradeCard card = new UpgradeCard(
                    CardType.StatUpgrade,
                    $"⬆️ {stat.data.upgradeDisplayText}\n(Current: Lv.{stat.upgradeLevel})",
                    stat.data.icon
                );
                card.statUpgrade = stat.data;
                possibleCards.Add(card);
            }
        }
        
        // Shuffle and return requested count
        if (possibleCards.Count == 0)
        {
            Debug.LogWarning("No possible cards to generate!");
            return possibleCards;
        }
        
        // Fisher-Yates shuffle
        for (int i = 0; i < possibleCards.Count; i++)
        {
            int randomIndex = Random.Range(i, possibleCards.Count);
            UpgradeCard temp = possibleCards[i];
            possibleCards[i] = possibleCards[randomIndex];
            possibleCards[randomIndex] = temp;
        }
        
        return possibleCards.GetRange(0, Mathf.Min(count, possibleCards.Count));
    }
    
    string GetWeaponUpgradeDisplay(WeaponInstance weapon, WeaponUpgradeType upgradeType)
    {
        string weaponName = weapon.data.weaponName;
        
        switch (upgradeType)
        {
            case WeaponUpgradeType.Damage:
                return $"⚔️ {weaponName} DAMAGE\n+{weapon.data.damagePerUpgrade} damage\n(Current: {weapon.damage})";
            
            case WeaponUpgradeType.ProjectileCount:
                return $"🌟 {weaponName} PROJECTILES\n+{weapon.data.projectileCountPerUpgrade} projectile\n(Current: {weapon.projectileCount})";
            
            case WeaponUpgradeType.FireRate:
                return $"⚡ {weaponName} FIRE RATE\nFaster shooting\n(Current: {weapon.fireRate:F2}s)";
            
            default:
                return $"{weaponName} Upgrade";
        }
    }
    
    public void ApplyCard(UpgradeCard card)
    {
        CharacterManager cm = CharacterManager.Instance;
        if (cm == null) return;
        
        switch (card.cardType)
        {
            case CardType.WeaponUnlock:
                if (card.weaponToUnlock != null)
                    cm.UnlockWeapon(card.weaponToUnlock);
                break;
            
            case CardType.WeaponUpgrade:
                if (card.weaponUpgrade != null)
                    cm.UpgradeWeapon(card.weaponUpgrade.targetWeapon, card.weaponUpgrade.upgradeType);
                break;
            
            case CardType.StatUnlock:
                if (card.statUpgrade != null)
                    cm.UnlockStat(card.statUpgrade);
                break;
            
            case CardType.StatUpgrade:
                if (card.statUpgrade != null)
                    cm.UpgradeStat(card.statUpgrade);
                break;
        }
    }
}