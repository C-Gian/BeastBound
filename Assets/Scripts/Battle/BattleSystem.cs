using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum BattleState { START, PLAYERTURN, PLAYERMOVESELECT, ENEMYTURN, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    [Header("Setup Arena")]
    public Transform enemySpawnPoint;
    public Transform playerSpawnPoint;
    public GameObject monsterPrefab;

    [Header("UI References")]
    public TMP_Text battleLogText;
    public BattleHUD playerHUD;
    public BattleHUD enemyHUD;

    [Header("UI Interaction")]
    public GameObject actionsPanel;
    public GameObject movesPanel;
    public TMP_Text[] moveTexts;
    public Button[] moveButtons;

    [Header("UI Move Relearner")]
    public GameObject moveSelectionPanel;
    public TMP_Text newMoveText;          // <-- Questo è il collegamento al testo "NewMoveInfoText"
    public Button[] forgetButtons;
    public TMP_Text[] forgetTexts;
    public Button dontLearnButton;

    private MoveBase moveToLearn;

    [Header("DEBUG / SETUP")]
    public MonsterData debugEnemyBase;
    public MonsterData debugPlayerBase;

    // --- NUOVO: Livelli separati ---
    [Range(1, 100)] public int playerStartLevel = 5;
    [Range(1, 100)] public int enemyStartLevel = 3;

    public BattleState state;
    private BattleUnit enemyUnit;
    private BattleUnit playerUnit;

    void Start()
    {
        actionsPanel.SetActive(false);
        movesPanel.SetActive(false);
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    void Update()
    {
        if (state == BattleState.PLAYERMOVESELECT)
        {
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackButton();
            }
        }
    }

    IEnumerator SetupBattle()
    {
        // Uso i due livelli separati
        Monster playerMonster = new Monster(debugPlayerBase, playerStartLevel);
        Monster enemyMonster = new Monster(debugEnemyBase, enemyStartLevel);

        GameObject playerGO = Instantiate(monsterPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        playerUnit = playerGO.GetComponent<BattleUnit>();
        playerUnit.Setup(playerMonster);
        playerHUD.SetupHUD(playerUnit.monster);

        GameObject enemyGO = Instantiate(monsterPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        enemyUnit = enemyGO.GetComponent<BattleUnit>();
        enemyUnit.Setup(enemyMonster);
        enemyHUD.SetupHUD(enemyUnit.monster);

        battleLogText.text = "Appare un " + enemyUnit.monster._base.monsterName + " Lvl " + enemyUnit.monster.level + "!";

        // --- NUOVO: CONTROLLO VELOCITÀ ---
        if (playerUnit.monster.Speed >= enemyUnit.monster.Speed)
        {
            battleLogText.text = "Sei più veloce! Attacchi per primo.";
            yield return new WaitForSeconds(1f);
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
        else
        {
            battleLogText.text = enemyUnit.monster._base.monsterName + " è velocissimo!";
            yield return new WaitForSeconds(1f);
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    // ... (PlayerTurn, OnAttackButton, ecc. rimangono uguali - OMESSI PER BREVITÀ, SONO SOTTO) ...
    // ... COPIA PURE I METODI DI INTERAZIONE UI DALLO SCRIPT PRECEDENTE O TIENILI SE NON LI HAI CANCELLATI ...

    // RIMETTO I METODI UI ESSENZIALI PER COMPLETEZZA (Cosi puoi fare copia-incolla sicuro)
    void PlayerTurn()
    {
        battleLogText.text = "Cosa farà " + playerUnit.monster._base.monsterName + "?";
        actionsPanel.SetActive(true);
        movesPanel.SetActive(false);
    }

    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN) return;
        state = BattleState.PLAYERMOVESELECT;
        actionsPanel.SetActive(false);
        movesPanel.SetActive(true);

        List<MoveBase> currentMoves = playerUnit.monster.moves;
        for (int i = 0; i < moveButtons.Length; i++)
        {
            if (i < currentMoves.Count)
            {
                moveButtons[i].gameObject.SetActive(true);
                moveButtons[i].interactable = true;
                if (moveTexts[i] != null) moveTexts[i].text = currentMoves[i].moveName;
            }
            else moveButtons[i].gameObject.SetActive(false);
        }
    }

    public void OnBackButton() { state = BattleState.PLAYERTURN; movesPanel.SetActive(false); actionsPanel.SetActive(true); }

    public void OnMoveSelected(int moveIndex)
    {
        if (state != BattleState.PLAYERMOVESELECT) return;
        if (moveIndex >= playerUnit.monster.moves.Count) return;
        movesPanel.SetActive(false);
        StartCoroutine(PlayerAttack(playerUnit.monster.moves[moveIndex]));
    }

    public void OnRunButton() { if (state != BattleState.PLAYERTURN) return; StartCoroutine(RunAwaySequence()); }

    // ... CALCOLI DANNO ...
    float GetTypeEffectiveness(MonsterType moveType, MonsterType defenderType)
    {
        if (moveType == MonsterType.None || defenderType == MonsterType.None) return 1f;
        if (moveType == MonsterType.Water)
        {
            if (defenderType == MonsterType.Fire) return 2f;
            if (defenderType == MonsterType.Water || defenderType == MonsterType.Grass) return 0.5f;
        }
        else if (moveType == MonsterType.Fire)
        {
            if (defenderType == MonsterType.Grass) return 2f;
            if (defenderType == MonsterType.Fire || defenderType == MonsterType.Water) return 0.5f;
        }
        else if (moveType == MonsterType.Grass)
        {
            if (defenderType == MonsterType.Water) return 2f;
            if (defenderType == MonsterType.Grass || defenderType == MonsterType.Fire) return 0.5f;
        }
        return 1f;
    }

    IEnumerator PlayerAttack(MoveBase move)
    {
        movesPanel.SetActive(false); actionsPanel.SetActive(false);
        battleLogText.text = playerUnit.monster._base.monsterName + " usa " + move.moveName + "!";
        yield return new WaitForSeconds(1f);

        int atk = playerUnit.monster.Attack;
        int def = enemyUnit.monster.Defense;
        int damage = Mathf.Max((atk + move.power) - (def / 2), 1);
        float effectiveness = GetTypeEffectiveness(move.type, enemyUnit.monster._base.type);
        damage = Mathf.RoundToInt(damage * effectiveness);

        bool isDead = enemyUnit.monster.TakeDamage(damage);
        enemyHUD.SetHP(enemyUnit.monster.currentHP);

        if (effectiveness > 1f) battleLogText.text = "È superefficace! (" + damage + ")";
        else if (effectiveness < 1f) battleLogText.text = "Non è molto efficace... (" + damage + ")";
        else battleLogText.text = "Colpito! (" + damage + ")";

        yield return new WaitForSeconds(2f);

        if (isDead) { state = BattleState.WON; StartCoroutine(EndBattle()); }
        else { state = BattleState.ENEMYTURN; StartCoroutine(EnemyTurn()); }
    }

    IEnumerator EnemyTurn()
    {
        if (enemyUnit.monster.moves.Count > 0)
        {
            int r = Random.Range(0, enemyUnit.monster.moves.Count);
            MoveBase move = enemyUnit.monster.moves[r];
            battleLogText.text = enemyUnit.monster._base.monsterName + " usa " + move.moveName + "!";
            yield return new WaitForSeconds(1f);

            int atk = enemyUnit.monster.Attack;
            int def = playerUnit.monster.Defense;
            int damage = Mathf.Max((atk + move.power) - (def / 2), 1);
            float effectiveness = GetTypeEffectiveness(move.type, playerUnit.monster._base.type);
            damage = Mathf.RoundToInt(damage * effectiveness);

            bool isDead = playerUnit.monster.TakeDamage(damage);
            playerHUD.SetHP(playerUnit.monster.currentHP);
            battleLogText.text = "Subisci " + damage + " danni!";
        }
        else { battleLogText.text = "Il nemico aspetta..."; }

        yield return new WaitForSeconds(2f);
        if (playerUnit.monster.currentHP <= 0) { state = BattleState.LOST; StartCoroutine(EndBattle()); }
        else { state = BattleState.PLAYERTURN; PlayerTurn(); }
    }

    IEnumerator RunAwaySequence()
    {
        state = BattleState.LOST;
        actionsPanel.SetActive(false);
        battleLogText.text = "Fuga strategica!";
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(0);
    }

    IEnumerator EndBattle()
    {
        if (state == BattleState.WON)
        {
            battleLogText.text = "VITTORIA! " + enemyUnit.monster._base.monsterName + " è esausto.";
            yield return WaitForInput();

            enemyUnit.gameObject.SetActive(false);

            // 1. Calcolo EXP
            int expGained = enemyUnit.monster._base.expYield * enemyUnit.monster.level;
            battleLogText.text = "Guadagni " + expGained + " Punti Esperienza!";
            yield return WaitForInput();

            // 2. Assegna EXP ai dati
            playerUnit.monster.AddExp(expGained);

            // 3. CICLO DI LIVELLAMENTO
            while (playerUnit.monster.currentExp >= playerUnit.monster.ExpForNextLevel)
            {
                // A. Anima barra fino al massimo
                yield return StartCoroutine(playerHUD.SmoothExpChange(playerUnit.monster.ExpForNextLevel));
                yield return new WaitForSeconds(0.2f);

                // B. Level Up Dati
                playerUnit.monster.LevelUp();

                // C. Feedback
                battleLogText.text = "LEVEL UP!";
                playerHUD.SetLevelText(playerUnit.monster.level);
                playerHUD.SetExpInstant(0);
                yield return new WaitForSeconds(1f);

                battleLogText.text = playerUnit.monster._base.monsterName + " sale al livello " + playerUnit.monster.level + "!";

                // Aggiorna massimo barra e HP
                playerHUD.expSlider.maxValue = playerUnit.monster.ExpForNextLevel;
                playerHUD.SetHP(playerUnit.monster.currentHP);

                yield return WaitForInput();

                // D. CONTROLLO NUOVE MOSSE
                MoveBase newMove = playerUnit.monster.GetMoveAtCurrentLevel();
                if (newMove != null)
                {
                    if (playerUnit.monster.moves.Count < 4)
                    {
                        // C'è spazio: Impara subito
                        playerUnit.monster.LearnMove(newMove);
                        battleLogText.text = playerUnit.monster._base.monsterName + " ha imparato " + newMove.moveName + "!";
                        yield return WaitForInput();
                    }
                    else
                    {
                        // SPAZIO PIENO: Apri il menu Scorda Mossa (ECCO LA MODIFICA)
                        battleLogText.text = playerUnit.monster._base.monsterName + " vuole imparare " + newMove.moveName + "...";
                        yield return WaitForInput();
                        battleLogText.text = "...ma conosce già 4 mosse!";
                        yield return WaitForInput();

                        // Chiama la nuova funzione UI che hai appena scritto
                        yield return StartCoroutine(ChooseMoveToForget(newMove));

                        // Quando la Coroutine finisce, l'utente ha scelto. Andiamo avanti.
                        yield return WaitForInput();
                    }
                }
            }

            // 4. Riempi barra col resto dell'EXP
            yield return StartCoroutine(playerHUD.SmoothExpChange(playerUnit.monster.currentExp));
            yield return WaitForInput();

            SceneManager.LoadScene(0);
        }
        else if (state == BattleState.LOST)
        {
            battleLogText.text = "Sei stato sconfitto...";
            playerUnit.monster.currentHP = playerUnit.monster.MaxHP;
            yield return WaitForInput();
            SceneManager.LoadScene(0);
        }
    }

    IEnumerator WaitForInput()
    {
        // Se sto tenendo premuto Z, aspetto pochissimo (0.1s), altrimenti il solito 0.5s
        float waitTime = (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Space)) ? 0.05f : 0.5f;
        yield return new WaitForSeconds(waitTime);

        // Aspetta finché non preme NUOVAMENTE Z
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Space));
    }

    public IEnumerator ChooseMoveToForget(MoveBase newMove)
    {
        moveToLearn = newMove;
        moveSelectionPanel.SetActive(true);

        newMoveText.text = "Vuoi imparare: " + newMove.moveName + "\n" +
                           "Potenza: " + newMove.power + " | Tipo: " + newMove.type;

        List<MoveBase> currentMoves = playerUnit.monster.moves;
        for (int i = 0; i < 4; i++)
        {
            forgetTexts[i].text = currentMoves[i].moveName + "\n(" + currentMoves[i].power + ")";

            forgetButtons[i].onClick.RemoveAllListeners();
            int index = i;
            forgetButtons[i].onClick.AddListener(() => OnForgetMoveSelected(index));
        }

        dontLearnButton.onClick.RemoveAllListeners();
        dontLearnButton.onClick.AddListener(() => OnKeepOldMoves());

        yield return new WaitUntil(() => moveSelectionPanel.activeSelf == false);
    }

    public void OnForgetMoveSelected(int indexToForget)
    {
        string forgottenMoveName = playerUnit.monster.moves[indexToForget].moveName;

        playerUnit.monster.moves[indexToForget] = moveToLearn;

        battleLogText.text = "1, 2, 3... Puff! Dimenticato " + forgottenMoveName + ".\n" +
                             "E al suo posto ha imparato " + moveToLearn.moveName + "!";

        moveSelectionPanel.SetActive(false);
    }

    public void OnKeepOldMoves()
    {
        battleLogText.text = playerUnit.monster._base.monsterName + " non ha imparato " + moveToLearn.moveName + ".";
        moveSelectionPanel.SetActive(false);
    }
}