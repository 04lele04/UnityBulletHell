public class WeaponInstance
{
    public WeaponData data;

    public int damage;
    public int projectileCount;
    public float fireRate;
    public float bulletSpeed;
    public float spreadAngle;

    public WeaponInstance(WeaponData data)
    {
        this.data = data;

        damage = data.baseDamage;
        projectileCount = data.projectileCount;
        fireRate = data.fireRate;
        bulletSpeed = data.bulletSpeed;
        spreadAngle = data.spreadAngle;
    }

    public void ApplyUpgrade(WeaponUpgradeType type)
    {
        switch (type)
        {
            case WeaponUpgradeType.Damage:
                damage += data.damagePerUpgrade;
                break;

            case WeaponUpgradeType.ProjectileCount:
                projectileCount += data.projectileCountPerUpgrade;
                break;

            case WeaponUpgradeType.FireRate:
                fireRate *= 0.9f;
                break;
        }
    }
}
