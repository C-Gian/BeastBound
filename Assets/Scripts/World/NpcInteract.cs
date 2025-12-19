using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NpcInteract : MonoBehaviour
{
    private bool isPlayerClose = false;

    [Header("Identità NPC")]
    public string trainerID = "Steve_01"; // <--- NUOVO: Nome univoco per salvare la vittoria

    // --- VARIABILI UI ---
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    // --------------------

    [Header("Data")]
    public MonsterData monsterToGive; // (In realtà è il nemico contro cui combatti)
    [Range(1, 100)] public int enemyLevel = 3; // <--- Ho aggiunto il livello qui per comodità

    void Start()
    {
        // All'inizio il pannello deve essere spento (invisibile)
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void Update()
    {
        // Se il giocatore è vicino e preme E
        if (isPlayerClose && Input.GetKeyDown(KeyCode.E))
        {
            Talk();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true;
            Debug.Log("Premi E per parlare");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;

            // Chiudi il pannello se ti allontani
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            // SBLOCCO MENU: Ora puoi riaprire il menu con TAB
            if (GameManager.instance != null)
            {
                GameManager.instance.isDialogueActive = false;
            }
        }
    }

    void Talk()
    {
        // --- 1. BLOCCO MENU ---
        // Appena iniziamo a parlare, diciamo al GameManager di BLOCCARE il tasto TAB
        if (GameManager.instance != null)
        {
            GameManager.instance.isDialogueActive = true;
        }

        // --- 2. CONTROLLO VITTORIA ---
        if (GameManager.instance != null && GameManager.instance.defeatedTrainers.Contains(trainerID))
        {
            // MOSTRA DIALOGO DI SCONFITTA
            if (dialoguePanel != null && dialogueText != null)
            {
                dialoguePanel.SetActive(true);
                dialogueText.text = "Mi hai già sconfitto! Non ho più mostri da mandare in campo.";
            }
            return; // Esci, non caricare la battaglia!
        }

        // --- 3. MOSTRA DIALOGO SFIDA ---
        if (dialoguePanel != null && dialogueText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueText.text = "Preparati a lottare!";
        }

        // --- 4. SETUP BATTAGLIA (IL TUO CODICE ORIGINALE) ---
        Debug.Log("Inizio Battaglia con " + monsterToGive.monsterName);

        if (GameManager.instance != null)
        {
            // A. Passiamo chi è il nemico e a che livello è
            GameManager.instance.activeEnemy = monsterToGive;
            GameManager.instance.activeEnemyLevel = enemyLevel;

            // B. Passiamo l'ID, così se vinciamo il BattleSystem lo può segnare
            GameManager.instance.currentOpponentID = trainerID;

            // C. Salviamo la posizione per il ritorno
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                GameManager.instance.nextSpawnPosition = player.transform.position;
                GameManager.instance.isReturningFromBattle = true;
            }
        }
        else
        {
            Debug.LogError("ERRORE: Manca GameManager!");
            return;
        }

        // D. Carica la scena
        SceneManager.LoadScene("BattleScene");
    }

    void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);

            // --- SBLOCCO ALTRI MENU ---
            if (GameManager.instance != null)
                GameManager.instance.isDialogueActive = false;
            // --------------------------
        }
    }
}