using UnityEngine;

[CreateAssetMenu(fileName = "New Move", menuName = "BeastBound/Move")]
public class MoveBase : ScriptableObject
{
    [Header("Info Mossa")]
    public string moveName;        // Nome (es. "Braciere")
    [TextArea]
    public string description;     // Descrizione (es. "Lancia una piccola fiamma")

    public MonsterType type;

    [Header("Stats")]
    public int power;              // Potenza (es. 40)
    public int accuracy;           // Precisione (es. 100 per 100%)

    // In futuro qui aggiungeremo il "Tipo" (Fuoco, Acqua...)
}