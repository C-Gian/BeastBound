using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public MonsterData baseData; // I dati (Ignis)
    public int currentHP;        // La vita attuale

    // Funzione chiamata quando il mostro appare
    public void Setup(MonsterData data)
    {
        baseData = data;
        currentHP = data.maxHP;

        // Cambiamo colore per far vedere che è lui
        GetComponent<Renderer>().material.color = data.associatedColor;
    }
}