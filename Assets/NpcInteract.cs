using TMPro; // FONDAMENTALE: Ci permette di usare TextMeshPro
using UnityEngine;
using UnityEngine.SceneManagement;

public class NpcInteract : MonoBehaviour
{
    private bool isPlayerClose = false;

    // --- VARIABILI UI ---
    [Header("UI References")] // Crea un titoletto nell'Inspector per ordine
    public GameObject dialoguePanel; // Il rettangolo nero intero
    public TMP_Text dialogueText;    // Il componente del testo
    // --------------------

    [Header("Data")]
    public MonsterData monsterToGive;

    void Start()
    {
        // All'inizio il pannello deve essere spento (invisibile)
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (isPlayerClose && Input.GetKeyDown(KeyCode.E))
        {
            // Se il pannello è già aperto, lo chiudiamo, altrimenti parliamo
            if (dialoguePanel.activeSelf)
            {
                CloseDialogue();
            }
            else
            {
                Talk();
            }
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
            CloseDialogue(); // Se ti allontani mentre parla, il box si chiude
        }
    }

    void Talk()
    {
        Debug.Log("Inizio Battaglia con " + monsterToGive.monsterName);

        if (GameManager.instance != null)
        {
            GameManager.instance.activeEnemy = monsterToGive;

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

        SceneManager.LoadScene("BattleScene");
    }


    void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}