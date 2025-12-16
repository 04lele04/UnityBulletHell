using UnityEngine;

[CreateAssetMenu(fileName = "Character", menuName = "Tarot/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string characterName;
    [TextArea(2, 4)]
    public string description;
    public Sprite portrait;
    
    [Header("Major Arcana")]
    public int arcanaNumber; // 0-21
    public string arcanaName; // "The Fool", "The Magician", etc.
    
    [Header("Base Stats")]
    public float baseSpeed = 6f;
    public int baseMaxHP = 3;
    
    [Header("Starter Weapon")]
    public WeaponData starterWeapon;
    
    [Header("Passive Effect")]
    [TextArea(2, 3)]
    public string passiveDescription;
    public PassiveEffectType passiveType;
    public float passiveValue; // Generic value for the passive
}

public enum PassiveEffectType
{
    None,
    RegenerateHP,        // Regenerate HP over time
    DamageBoost,         // % damage increase
    SpeedBoost,          // % speed increase
    ProjectilePierce,    // Bullets pierce N enemies
    ExtraXP             // Gain X% more XP
}