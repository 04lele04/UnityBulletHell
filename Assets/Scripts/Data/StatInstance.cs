public class StatInstance
{
    public StatData data;
    public int upgradeLevel;
    public bool isUnlocked;

    public StatInstance(StatData data)
    {
        this.data = data;
        upgradeLevel = 0;
        isUnlocked = false;
    }

    // ✅ NUOVO SISTEMA: Additivo vs Moltiplicativo
    public float GetBonus()
    {
        if (!isUnlocked || upgradeLevel <= 0)
            return 0f;

        switch (data.statType)
        {
            case StatType.Health:
            case StatType.ProjectileQuantity:
                // ADDITIVO: +1 per livello
                return upgradeLevel * 1f;

            case StatType.Speed:
            case StatType.Damage:
                // MOLTIPLICATIVO: +50% per livello (0.5 = 50%)
                return upgradeLevel * 0.5f;

            case StatType.FireRate:
                // RIDUZIONE: -33% per livello (più alto = più veloce)
                // Max 80% riduzione (livello 2.4, quindi cap a ~lvl 2)
                return UnityEngine.Mathf.Min(upgradeLevel * 0.33f, 0.8f);

            default:
                return upgradeLevel * data.valuePerLevel;
        }
    }
}