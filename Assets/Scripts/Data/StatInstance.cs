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

    public float GetValue()
    {
        return upgradeLevel * data.valuePerLevel;
    }
}
