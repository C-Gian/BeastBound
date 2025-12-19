using UnityEngine;

// Invece di "ScriptableObject", ereditiamo da "ItemBase"
// Così abbiamo gratis nome, descrizione e icona, ma aggiungiamo cose nuove.
[CreateAssetMenu(fileName = "Nuova Pokeball", menuName = "Items/Crea Pokeball")]
public class PokeballItem : ItemBase
{
    [Header("Logica di Cattura")]
    public float catchRateModifier = 1f; // 1x per la sfera base, 1.5x per la Mega, etc.
}