using UnityEngine;

[CreateAssetMenu(fileName = "Nuovo Oggetto", menuName = "Items/Crea Nuovo Oggetto")]
public class ItemBase : ScriptableObject
{
    [Header("Info Generiche")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon; // L'icona da mostrare nell'inventario
}