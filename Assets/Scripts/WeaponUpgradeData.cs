using UnityEngine;

[CreateAssetMenu(fileName = "WeaponUpgrade", menuName = "Tarot/Weapon Upgrade")]
public class WeaponUpgradeData : ScriptableObject
{
    public WeaponData targetWeapon;
    public WeaponUpgradeType upgradeType;

    [Header("Display")]
    public string displayName;

    [TextArea(2, 3)]
    public string description;

    public Sprite icon;
}
