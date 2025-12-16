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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public List<UpgradeCard> GenerateCards(int count)
    {
        CharacterManager cm = CharacterManager.Instance;
        if (cm == null)
        {
            Debug.LogError("CharacterManager not found!");
            return new List<UpgradeCard>();
        }

        // Crea pool separati
        List<UpgradeCard> weaponCards = new List<UpgradeCard>();
        List<UpgradeCard> statCards = new List<UpgradeCard>();

        // 1. Weapon Cards (Unlock + Upgrade)
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
                    weaponCards.Add(card);
                }
            }
        }

        foreach (WeaponInstance weapon in cm.GetEquippedWeapons())
        {
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
                weaponCards.Add(card);
            }
        }

        // 2. Stat Cards (Unlock + Upgrade)
        Debug.Log($"=== STAT CARD GENERATION DEBUG ===");
        Debug.Log($"CanUnlockMoreStats: {cm.CanUnlockMoreStats()}");

        if (cm.CanUnlockMoreStats())
        {
            var allStats = cm.GetCharacterStats();
            Debug.Log($"Total stats found: {allStats.Count}");

            foreach (StatInstance stat in allStats)
            {
                Debug.Log($"Stat: {stat.data.unlockDisplayText}, IsUnlocked: {stat.isUnlocked}");

                if (!stat.isUnlocked)
                {
                    UpgradeCard card = new UpgradeCard(
                        CardType.StatUnlock,
                        $"🌟 {stat.data.unlockDisplayText}\n{stat.data.description}",
                        stat.data.icon
                    );
                    card.statUpgrade = stat.data;
                    statCards.Add(card);
                    Debug.Log($"✅ Added StatUnlock card: {stat.data.unlockDisplayText}");
                }
            }
        }

        var allStatsForUpgrade = cm.GetCharacterStats();
        Debug.Log($"Checking stats for upgrade, total: {allStatsForUpgrade.Count}");

        foreach (StatInstance stat in allStatsForUpgrade)
        {
            if (stat.isUnlocked)
            {
                UpgradeCard card = new UpgradeCard(
                    CardType.StatUpgrade,
                    $"⬆️ {stat.data.upgradeDisplayText}\n(Current: Lv.{stat.upgradeLevel})",
                    stat.data.icon
                );
                card.statUpgrade = stat.data;
                statCards.Add(card);
                Debug.Log($"✅ Added StatUpgrade card: {stat.data.upgradeDisplayText}");
            }
        }

        Debug.Log($"=== FINAL COUNTS ===");
        Debug.Log($"Weapon Cards: {weaponCards.Count}");
        Debug.Log($"Stat Cards: {statCards.Count}");

        // 3. DISTRIBUZIONE INTELLIGENTE (priorità agli stat se disponibili)
        List<UpgradeCard> finalCards = new List<UpgradeCard>();

        // Se ci sono stat cards, garantisci almeno 1-2 stat cards
        int guaranteedStatCards = 0;
        if (statCards.Count > 0)
        {
            // Se richieste 3 carte, almeno 1 stat
            // Se richieste 4+ carte, almeno 2 stat
            guaranteedStatCards = count >= 4 ? 2 : 1;
            guaranteedStatCards = Mathf.Min(guaranteedStatCards, statCards.Count);
        }

        int weaponCount = count - guaranteedStatCards;

        Debug.Log($"Distribution: {weaponCount} weapons, {guaranteedStatCards} stats (guaranteed)");

        // Shuffle entrambe le liste
        ShuffleList(weaponCards);
        ShuffleList(statCards);

        // Aggiungi carte armi
        int weaponsAdded = 0;
        for (int i = 0; i < weaponCount && i < weaponCards.Count; i++)
        {
            finalCards.Add(weaponCards[i]);
            weaponsAdded++;
        }

        // Aggiungi carte stat (garantite)
        int statsAdded = 0;
        for (int i = 0; i < guaranteedStatCards && i < statCards.Count; i++)
        {
            finalCards.Add(statCards[i]);
            statsAdded++;
        }

        // Se mancano carte per raggiungere 'count', riempi con quello che resta
        int remaining = count - finalCards.Count;
        if (remaining > 0)
        {
            // Prima prova ad aggiungere altre stat cards
            for (int i = statsAdded; i < statCards.Count && remaining > 0; i++)
            {
                finalCards.Add(statCards[i]);
                remaining--;
            }

            // Poi altre weapon cards
            for (int i = weaponsAdded; i < weaponCards.Count && remaining > 0; i++)
            {
                finalCards.Add(weaponCards[i]);
                remaining--;
            }
        }

        // Mescola il risultato finale
        ShuffleList(finalCards);

        Debug.Log($"✅ Final cards returned: {finalCards.Count}");
        foreach (var card in finalCards)
        {
            Debug.Log($"  → {card.cardType}: {card.displayText.Split('\n')[0]}");
        }

        if (finalCards.Count == 0)
        {
            Debug.LogWarning("⚠️ No possible cards to generate!");
        }

        return finalCards;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
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

        // ✅ CRUCIALE: Ricalcola le stat del player dopo ogni upgrade!
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.RecalculateStats();
        }
    }
}