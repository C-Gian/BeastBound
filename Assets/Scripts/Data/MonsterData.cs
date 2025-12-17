using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Monster", menuName = "BeastBound/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("Info Specie")]
    public string monsterName;
    public MonsterType type;
    [TextArea] public string description;
    public Color associatedColor;

    [Header("Statistiche Base")]
    public int maxHP;
    public int attack;
    public int defense;
    public int speed;
    public int expYield; 

    [Header("Pool Mosse (Imparabili)")]
    public List<LearnableMove> learnableMoves;
}

[System.Serializable]
public class LearnableMove
{
    public MoveBase moveBase;
    public int level;
}