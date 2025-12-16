using UnityEngine;

public class UpgradeCard
{
    public CardType cardType;
    public string displayText;
    public Sprite icon;

    // Only ONE of these is used depending on cardType
    public WeaponData weaponToUnlock;
    public WeaponUpgradeData weaponUpgrade;
    public StatData statUpgrade;

    public UpgradeCard(CardType type, string text, Sprite iconSprite = null)
    {
        cardType = type;
        displayText = text;
        icon = iconSprite;
    }
}
