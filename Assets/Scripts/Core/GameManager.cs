using UnityEngine;
using System.Collections.Generic; // <--- Importante per le Liste

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public List<Monster> playerParty;

    public MonsterData activeEnemy; // La specie del nemico che incontrerai
    public int activeEnemyLevel;    // <--- NUOVO: Il livello del nemico

    public Vector3 nextSpawnPosition;
    public bool isReturningFromBattle;

    [Header("Setup Iniziale")]
    public MonsterData defaultStarter; // Trascina Ignis qui nell'Inspector

    [Header("Memoria Mondo")]
    public List<string> defeatedTrainers; // Lista di ID (es. "Steve_01", "Bob_02")
    public string currentOpponentID;

    [Header("Sistemi")]
    public Inventory inventory;

    [Header("Stati di Gioco")]
    public bool isDialogueActive = false; // Se è true, non aprire menu!


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        inventory = GetComponent<Inventory>();

        if (defeatedTrainers == null)
            defeatedTrainers = new List<string>();

        if (playerParty == null)
            playerParty = new List<Monster>();
    }

    public void AddDefeatedTrainer(string trainerID)
    {
        if (!defeatedTrainers.Contains(trainerID))
        {
            defeatedTrainers.Add(trainerID);
        }
    }

    void Start()
    {
        // Se avvii il gioco e non hai mostri, ti crea lo starter
        if (playerParty.Count == 0 && defaultStarter != null)
        {
            // Creiamo un mostro VIVO partendo dal file
            Monster starter = new Monster(defaultStarter, 5);
            playerParty.Add(starter);
            Debug.Log("Starter generato nel GameManager: " + starter._base.monsterName);
        }
    }
}