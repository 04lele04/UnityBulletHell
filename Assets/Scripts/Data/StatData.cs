using UnityEngine;

[CreateAssetMenu(fileName = "Stat", menuName = "Tarot/Stat Data")]
public class StatData : ScriptableObject
{
    public StatType statType;

    public string unlockDisplayText;
    public string upgradeDisplayText;
    [TextArea]
    public string description;

    public Sprite icon;

    public float valuePerLevel = 0.1f;
}
