using UnityEngine;
using TMPro; // Serve se vuoi mostrare il testo a schermo (opzionale per ora, ma predisposto)

public class Healer : MonoBehaviour
{
    [Header("Configurazione")]
    public bool healAllParty = true; // Se vero cura tutti, se falso solo il primo

    [Header("UI (Opzionale)")]
    public GameObject dialoguePanel; // Il pannello nero (puoi riusare quello di Steve)
    public TMP_Text dialogueText;

    private bool isPlayerClose;

    private void Update()
    {
        // Se sono vicino e premo E
        if (isPlayerClose && Input.GetKeyDown(KeyCode.E))
        {
            HealParty();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = true;
            Debug.Log("Premi E per curare la squadra.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;
            // Se avevi aperto un pannello, qui potresti chiuderlo
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
        }
    }

    void HealParty()
    {
        if (GameManager.instance != null && GameManager.instance.playerParty.Count > 0)
        {
            // Ciclo su tutti i mostri della squadra
            foreach (Monster monster in GameManager.instance.playerParty)
            {
                monster.currentHP = monster.MaxHP; // Ripristina HP al massimo

                // Opzionale: Ripristina anche i PP delle mosse se volessimo gestirli in futuro
            }

            Debug.Log("Squadra curata completamente!");

            // Feedback Visivo (se hai collegato il pannello)
            if (dialoguePanel != null && dialogueText != null)
            {
                dialoguePanel.SetActive(true);
                dialogueText.text = "La tua squadra è tornata in piena forma!";
            }
        }
        else
        {
            Debug.LogWarning("Nessuna squadra trovata nel GameManager!");
        }
    }
}