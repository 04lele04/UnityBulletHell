using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Tarot/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("Combat")]
    public GameObject bulletPrefab;
    public int baseDamage = 1;
    public float fireRate = 1f;
    public int projectileCount = 1;
    public float spreadAngle = 10f;
    public float bulletSpeed = 8f;

    [Header("Upgrades")]
    public int damagePerUpgrade = 1;
    public int projectileCountPerUpgrade = 1;
    public float fireRatePerUPgrade = 1f;


}
