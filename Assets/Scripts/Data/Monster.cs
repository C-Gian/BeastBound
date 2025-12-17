using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Monster
{
    public MonsterData _base;
    public int level;
    public int currentHP;
    public int currentExp;

    public List<MoveBase> moves;

    public Monster(MonsterData pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
        currentHP = MaxHP;
        currentExp = 0;

        moves = new List<MoveBase>();

        foreach (var move in _base.learnableMoves)
        {
            if (move.level <= level)
            {
                moves.Add(move.moveBase);
            }
            if (moves.Count >= 4) break;
        }
    }

    public int MaxHP
    {
        get { return Mathf.FloorToInt((_base.maxHP * level) / 10f) + 10; }
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((_base.attack * level) / 10f) + 5; }
    }

    public int Defense
    {
        get { return Mathf.FloorToInt((_base.defense * level) / 10f) + 5; }
    }

    public int ExpForNextLevel
    {
        get
        {
            return 10 + (level * level * level);
        }
    }

    public int Speed
    {
        get { return Mathf.FloorToInt((_base.speed * level) / 10f) + 5; }
    }

    public bool TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
        {
            currentHP = 0;
            return true; 
        }
        return false; 
    }

    public MoveBase GetMoveAtCurrentLevel()
    {
        foreach (var learnable in _base.learnableMoves)
        {
            if (learnable.level == level)
            {
                return learnable.moveBase;
            }
        }
        return null; // Nessuna mossa nuova a questo livello
    }

    public void LearnMove(MoveBase newMove)
    {
        if (moves.Count < 4)
        {
            moves.Add(newMove);
        }
    }

    public void AddExp(int expToAdd)
    {
        currentExp += expToAdd;
    }

    public void LevelUp()
    {
        currentExp -= ExpForNextLevel; // Sottrae l'exp usata per livellare
        level++;
        currentHP = MaxHP; // Cura completa
    }
}