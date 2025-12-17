using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    [Header("DEBUG (Usa questi se avvii da questa scena)")]
    public MonsterData debugEnemy;
    public MonsterData debugPlayer;

    [Header("Setup Arena")]
    public Transform enemySpawnPoint;
    public Transform playerSpawnPoint; // DOVE APPARE IL TUO MOSTRO
    public GameObject monsterPrefab;   // Usiamo lo stesso prefab (la capsula) per entrambi!

    [Header("UI References")]
    public TMP_Text battleLogText;
    public Button attackButton;

    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    public BattleState state;
    private BattleUnit enemyUnit;
    private BattleUnit playerUnit;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        MonsterData enemyData = null;
        MonsterData playerData = null;

        if (GameManager.instance != null)
        {
            enemyData = GameManager.instance.activeEnemy;
            playerData = GameManager.instance.playerMonster;
        }
        else
        {
            // NO: Siamo in modalità test, usiamo i dati di debug
            Debug.LogWarning("MODALITÀ DEBUG: Uso i mostri di prova dell'Inspector.");
            enemyData = debugEnemy;
            playerData = debugPlayer;
        }

        // Sicurezza
        if (enemyData == null || playerData == null)
        {
            Debug.LogError("Mancano i dati dei mostri! Verifica il GameManager.");
            yield break;
        }

        // 2. SPAWN NEMICO
        // Usiamo la rotazione dello spawnPoint così guarda nella direzione giusta
        GameObject enemyGO = Instantiate(monsterPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        enemyUnit = enemyGO.GetComponent<BattleUnit>();
        enemyUnit.Setup(enemyData);

        enemyHUD.SetupHUD(enemyData);

        // 3. SPAWN PLAYER (IL TUO MOSTRO)
        GameObject playerGO = Instantiate(monsterPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        playerUnit = playerGO.GetComponent<BattleUnit>();
        playerUnit.Setup(playerData);

        playerHUD.SetupHUD(playerData);

        battleLogText.text = "Vai " + playerUnit.baseData.monsterName + "!";
        yield return new WaitForSeconds(2f);
        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        battleLogText.text = "Cosa deve fare " + playerUnit.baseData.monsterName + "?";
        attackButton.interactable = true;
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        StartCoroutine(PlayerAttack());
    }

    IEnumerator PlayerAttack()
    {
        attackButton.interactable = false;

        // CALCOLO DANNO: Usiamo l'attacco del NOSTRO mostro (Aqua)
        int damage = playerUnit.baseData.attack;

        // Applichiamo il danno al nemico
        enemyUnit.currentHP -= damage;

        enemyHUD.SetHP(enemyUnit.currentHP);

        battleLogText.text = playerUnit.baseData.monsterName + " attacca e infligge " + damage + " danni!";

        yield return new WaitForSeconds(2f);

        if (enemyUnit.currentHP <= 0)
        {
            state = BattleState.WON;
            StartCoroutine(EndBattle()); // Ora EndBattle è una Coroutine per l'attesa
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyTurn()
    {
        battleLogText.text = enemyUnit.baseData.monsterName + " sta attaccando...";

        yield return new WaitForSeconds(1.5f);

        int damage = enemyUnit.baseData.attack;
        playerUnit.currentHP -= damage;

        playerHUD.SetHP(playerUnit.currentHP);

        battleLogText.text = "Il tuo mostro subisce " + damage + " danni! (HP: " + playerUnit.currentHP + ")";

        yield return new WaitForSeconds(2f);

        if (playerUnit.currentHP <= 0)
        {
            state = BattleState.LOST;
            StartCoroutine(EndBattle());
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    // --- NUOVA LOGICA DI FINE BATTAGLIA (AUTO-EXIT) ---
    IEnumerator EndBattle()
    {
        if (state == BattleState.WON)
        {
            battleLogText.text = "VITTORIA! " + enemyUnit.baseData.monsterName + " è esausto.";
            enemyUnit.gameObject.SetActive(false); // Il nemico sviene
        }
        else if (state == BattleState.LOST)
        {
            battleLogText.text = "SCONFITTA! " + playerUnit.baseData.monsterName + " non può più combattere.";
            playerUnit.gameObject.SetActive(false); // Il tuo mostro sviene
        }

        // Attesa drammatica di 3 secondi per leggere il risultato
        yield return new WaitForSeconds(3f);

        battleLogText.text = "Torno al mondo...";
        yield return new WaitForSeconds(1f);

        // Carica la scena numero 0 (il Mondo)
        SceneManager.LoadScene(0);
    }

    // --- LOGICA FUGA ---
    public void OnRunButton()
    {
        if (state != BattleState.PLAYERTURN) return;

        StartCoroutine(RunAwaySequence());
    }

    IEnumerator RunAwaySequence()
    {
        // 1. Blocchiamo tutto
        state = BattleState.LOST; // Usiamo LOST o creiamo uno stato ESCAPED, ma per ora va bene bloccare
        attackButton.interactable = false;

        // Nota: Se hai un riferimento al pulsante Run, disattiva anche quello
        // runButton.interactable = false; 

        // 2. Messaggio
        battleLogText.text = "Sei scappato gambe levate!";

        // 3. Attesa
        yield return new WaitForSeconds(2f);

        // 4. Carica Mondo
        SceneManager.LoadScene(0);
    }
}