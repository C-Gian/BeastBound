using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum BattleState
{
    START,
    PLAYERTURN,
    PLAYERMOVESELECT,
    ENEMYTURN,
    WON,
    LOST,
    BUSY // <--- Aggiunto questo
}

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

    [Header("UI Inventario")]
    public GameObject inventoryPanel;       // Il pannello intero
    public Transform itemsContainer;        // Dove generare i bottoni
    public GameObject itemButtonPrefab;     // Il prefab del bottone creato prima
    public TMP_Text itemDescriptionText;    // Testo descrizione in basso

    private ItemBase selectedItem;          // L'oggetto che stiamo guardando

    // --- NUOVO: Livelli separati ---
    [Range(1, 100)] public int playerStartLevel = 5;
    [Range(1, 100)] public int enemyStartLevel = 3;

    public BattleState state;
    private BattleUnit enemyUnit;
    private BattleUnit playerUnit;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        actionsPanel.SetActive(false);
        movesPanel.SetActive(false);

        if (moveSelectionPanel != null)
        {
            moveSelectionPanel.SetActive(false);
        }

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
        // 0. Reset UI
        actionsPanel.SetActive(false);
        movesPanel.SetActive(false);

        // 1. RECUPERO DATI (GameManager vs Debug)
        Monster playerMonster = null;
        Monster enemyMonster = null;

        // Controlliamo se esiste il GameManager e se ha una squadra
        if (GameManager.instance != null && GameManager.instance.playerParty.Count > 0)
        {
            // Prende il primo mostro della squadra (quello VERO, con exp e livelli salvati)
            playerMonster = GameManager.instance.playerParty[0];

            // Se il GameManager ha impostato un nemico specifico (es. incontro nell'erba)
            if (GameManager.instance.activeEnemy != null)
            {
                enemyMonster = new Monster(GameManager.instance.activeEnemy, GameManager.instance.activeEnemyLevel);
            }
        }

        // FALLBACK: Se non abbiamo trovato dati nel GameManager (o stiamo testando solo la scena battaglia)
        // usiamo le variabili di Debug dell'Inspector
        if (playerMonster == null)
        {
            playerMonster = new Monster(debugPlayerBase, playerStartLevel);
        }

        if (enemyMonster == null)
        {
            // Se il nemico è null, usiamo quello di debug
            enemyMonster = new Monster(debugEnemyBase, enemyStartLevel);
        }

        // 2. SPAWN VISUALE E SETUP HUD
        GameObject playerGO = Instantiate(monsterPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
        playerUnit = playerGO.GetComponent<BattleUnit>();
        playerUnit.Setup(playerMonster);
        playerHUD.SetupHUD(playerUnit.monster);

        GameObject enemyGO = Instantiate(monsterPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
        enemyUnit = enemyGO.GetComponent<BattleUnit>();
        enemyUnit.Setup(enemyMonster);
        enemyHUD.SetupHUD(enemyUnit.monster);

        // 3. INIZIO BATTAGLIA
        battleLogText.text = "Appare un " + enemyUnit.monster._base.monsterName + " Lvl " + enemyUnit.monster.level + "!";

        // Aspettiamo 2 secondi per leggere il testo dell'apparizione
        yield return new WaitForSeconds(2f);

        // 4. CONTROLLO VELOCITÀ
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

            // --- 1. SALVATAGGIO VITTORIA NPC (Il pezzo mancante) ---
            // Se c'è un GameManager e abbiamo un ID dell'avversario (es. "Steve_01")
            if (GameManager.instance != null && !string.IsNullOrEmpty(GameManager.instance.currentOpponentID))
            {
                Debug.Log("Vittoria registrata contro: " + GameManager.instance.currentOpponentID);
                GameManager.instance.AddDefeatedTrainer(GameManager.instance.currentOpponentID);

                // Resettiamo l'ID per pulizia, così non lo segna due volte per sbaglio
                GameManager.instance.currentOpponentID = "";
            }
            // -------------------------------------------------------

            // 2. Calcolo EXP
            int expGained = enemyUnit.monster._base.expYield * enemyUnit.monster.level;
            battleLogText.text = "Guadagni " + expGained + " Punti Esperienza!";
            yield return WaitForInput();

            // 3. Assegna EXP ai dati
            playerUnit.monster.AddExp(expGained);

            // 4. CICLO DI LIVELLAMENTO
            while (playerUnit.monster.currentExp >= playerUnit.monster.ExpForNextLevel)
            {
                yield return StartCoroutine(playerHUD.SmoothExpChange(playerUnit.monster.ExpForNextLevel));
                yield return new WaitForSeconds(0.2f);

                playerUnit.monster.LevelUp();

                battleLogText.text = "LEVEL UP!";
                playerHUD.SetLevelText(playerUnit.monster.level);
                playerHUD.SetExpInstant(0);
                yield return new WaitForSeconds(1f);

                battleLogText.text = playerUnit.monster._base.monsterName + " sale al livello " + playerUnit.monster.level + "!";

                playerHUD.expSlider.maxValue = playerUnit.monster.ExpForNextLevel;
                playerHUD.SetHP(playerUnit.monster.currentHP);

                yield return WaitForInput();

                // Controllo nuove mosse
                MoveBase newMove = playerUnit.monster.GetMoveAtCurrentLevel();
                if (newMove != null)
                {
                    if (playerUnit.monster.moves.Count < 4)
                    {
                        playerUnit.monster.LearnMove(newMove);
                        battleLogText.text = playerUnit.monster._base.monsterName + " ha imparato " + newMove.moveName + "!";
                        yield return WaitForInput();
                    }
                    else
                    {
                        battleLogText.text = playerUnit.monster._base.monsterName + " vuole imparare " + newMove.moveName + "...";
                        yield return WaitForInput();
                        battleLogText.text = "...ma conosce già 4 mosse!";
                        yield return WaitForInput();

                        // Chiama la UI "Scorda Mossa"
                        yield return StartCoroutine(ChooseMoveToForget(newMove));
                        yield return WaitForInput();
                    }
                }
            }

            // 5. Riempi barra col resto dell'EXP
            yield return StartCoroutine(playerHUD.SmoothExpChange(playerUnit.monster.currentExp));
            yield return WaitForInput();

            SceneManager.LoadScene(0);
        }
        else if (state == BattleState.LOST)
        {
            battleLogText.text = "Sei stato sconfitto...";
            playerUnit.monster.currentHP = playerUnit.monster.MaxHP; // Cura di emergenza (o game over)
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

    // 1. Chiamato quando premi il bottone "BAG" nel menu principale
    public void OnBagButton()
    {
        actionsPanel.SetActive(false); // Nascondi azioni (Attacca/Fuga)
        inventoryPanel.SetActive(true); // Mostra zaino

        UpdateInventoryUI();
    }

    // 2. Disegna la lista degli oggetti
    void UpdateInventoryUI()
    {
        // Pulisci i vecchi bottoni (se c'erano)
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Recupera gli oggetti dal GameManager
        if (GameManager.instance.inventory != null)
        {
            foreach (var slot in GameManager.instance.inventory.slots)
            {
                // Crea il bottone
                GameObject btnObj = Instantiate(itemButtonPrefab, itemsContainer);

                // Imposta il testo: "Pokeball x5"
                TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
                btnText.text = slot.item.itemName + " x" + slot.count;

                // Aggiungi l'evento click
                Button btn = btnObj.GetComponent<Button>();
                ItemBase itemRef = slot.item; // Variabile locale per la closure
                btn.onClick.AddListener(() => OnItemSelected(itemRef));
            }
        }
    }

    // 3. Quando clicchi su un oggetto nella lista
    public void OnItemSelected(ItemBase item)
    {
        selectedItem = item;
        itemDescriptionText.text = item.description;

        // Qui potremmo aprire un pannellino di conferma "Usa / Annulla"
        // Per semplicità, usiamo la Dialog Box principale per chiedere conferma
        StartCoroutine(AskConfirmation(item));
    }

    IEnumerator AskConfirmation(ItemBase item)
    {
        // Nascondiamo momentaneamente lo zaino per vedere la scena
        inventoryPanel.SetActive(false);

        battleLogText.text = "Vuoi usare " + item.itemName + "? (Z per Sì, X per No)";
        yield return new WaitForSeconds(0.2f); // Piccolo delay anti-input

        bool choiceMade = false;
        bool confirmed = false;

        // Aspetta input (Loop semplice)
        while (!choiceMade)
        {
            if (Input.GetKeyDown(KeyCode.Z)) // Conferma
            {
                confirmed = true;
                choiceMade = true;
            }
            else if (Input.GetKeyDown(KeyCode.X)) // Annulla
            {
                confirmed = false;
                choiceMade = true;
            }
            yield return null;
        }

        if (confirmed)
        {
            // Esegui l'azione dell'oggetto
            yield return StartCoroutine(UseItem(item));
        }
        else
        {
            // Torna allo zaino
            battleLogText.text = "Scegli un'azione.";
            OnBagButton();
        }
    }

    // 4. Logica di utilizzo (Cattura o altro)
    IEnumerator UseItem(ItemBase item)
    {
        // Consumiamo l'oggetto
        GameManager.instance.inventory.RemoveItem(item, 1);

        // È una Pokeball?
        if (item is PokeballItem)
        {
            yield return StartCoroutine(ThrowPokeball((PokeballItem)item));
        }
        else
        {
            // In futuro qui gestiremo Pozioni ecc.
            battleLogText.text = "Hai usato " + item.itemName + ". Non succede nulla (WIP).";
            yield return WaitForInput();

            // Torna al menu principale o passa turno
            actionsPanel.SetActive(true);
        }
    }

    // Tasto "Indietro" nello zaino
    public void OnBackFromBag()
    {
        inventoryPanel.SetActive(false);
        actionsPanel.SetActive(true);
        battleLogText.text = "Scegli un'azione.";
    }

    IEnumerator ThrowPokeball(PokeballItem pokeball)
    {
        state = BattleState.BUSY;
        battleLogText.text = "Lanci la " + pokeball.itemName + "!";
        yield return WaitForInput();

        // LOGICA DI CATTURA (Formula Semplificata)
        // Probabilità basata sugli HP rimanenti del nemico
        float hpPercent = (float)enemyUnit.monster.currentHP / enemyUnit.monster.MaxHP;

        // Esempio: 
        // 100% vita = 0% bonus
        // 1% vita = quasi 100% bonus
        // Moltiplicato per il rate della palla (es. 1.0, 1.5, 2.0)

        // Numero a caso tra 0 e 100
        int randomValue = Random.Range(0, 100);

        // Soglia per catturare:
        // Se HP sono pieni (1.0), soglia = 10 * rate (Difficile)
        // Se HP sono vuoti (0.1), soglia = 60 * rate (Facile)
        int catchThreshold = (int)((1.0f - hpPercent) * 60f * pokeball.catchRateModifier) + 10;

        // Simulazione suspance...
        battleLogText.text = "...";
        yield return new WaitForSeconds(1f);

        if (randomValue < catchThreshold)
        {
            battleLogText.text = "Preso! " + enemyUnit.monster._base.monsterName + " catturato!";
            yield return WaitForInput();

            // Aggiungi alla squadra
            if (GameManager.instance.playerParty.Count < 6)
            {
                GameManager.instance.playerParty.Add(enemyUnit.monster);
                battleLogText.text = "Aggiunto alla squadra.";
            }
            else
            {
                battleLogText.text = "Squadra piena! (Futuro: Invia al PC)";
                // Per ora perso se full
            }
            yield return WaitForInput();

            // Fine Battaglia (Vittoria tecnica)
            enemyUnit.gameObject.SetActive(false);

            // IMPORTANTE: Pulizia dati se era un selvatico
            // Se avevi salvato l'ID per Steve, qui non serve o va gestito diversamente
            SceneManager.LoadScene(0);
        }
        else
        {
            battleLogText.text = "Agh! Si è liberato!";
            yield return WaitForInput();

            // Turno Nemico
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }
}