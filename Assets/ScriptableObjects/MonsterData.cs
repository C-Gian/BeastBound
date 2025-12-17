using UnityEngine;

// Questo attributo aggiunge una voce nel menu "Create" di Unity
[CreateAssetMenu(fileName = "NewMonster", menuName = "BeastBound/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("Info Generali")]
    public string monsterName; // Nome del mostro (es. "Fiammotto")
    [TextArea] public string description; // Descrizione per il Pokedex

    [Header("Aspetto")]
    public Color associatedColor; // Per ora usiamo un colore, in futuro il Modello 3D

    [Header("Statistiche Base")]
    public int maxHP;
    public int attack;
    public int defense;
}