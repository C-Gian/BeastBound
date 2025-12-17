using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    // Riferimento al mostro "vivo" (con HP variabili, livello, ecc.)
    public Monster monster;

    public void Setup(Monster _monster)
    {
        monster = _monster;

        // Assegna il colore in base alla specie (se usi i materiali colorati)
        GetComponent<MeshRenderer>().material.color = monster._base.associatedColor;
    }
}