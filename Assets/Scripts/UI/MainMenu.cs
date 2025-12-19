using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("Pannelli")]
    public GameObject menuRoot;
    public GameObject teamPanel;
    public GameObject bagPanel;

    [Header("Pannello Dettagli")]
    public GameObject summaryPanel;
    public MonsterSummaryUI summaryScript;

    [Header("Elementi Team")]
    public GameObject memberSlotPrefab;
    public Transform teamListContainer;

    [Header("Elementi Bag")]
    public GameObject itemSlotPrefab;
    public Transform bagListContainer;
    public TMP_Text itemDescriptionText;

    [Header("Script da Bloccare")]
    public PlayerMovement playerMovementScript;
    public CameraOrbit cameraOrbitScript;

    private void Update()
    {
        // 1. Controllo Priorità
        if (GameManager.instance != null && GameManager.instance.isDialogueActive) return;

        // 2. Tasto TAB
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMenu();
        }

        // --- CODICE SPIA (DEBUG) ---
        // Premi play, premi il tasto "-" e guarda nella Console cosa scrive Unity!
        if (Input.anyKeyDown)
        {
            foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                    Debug.Log("Hai premuto il tasto: " + kcode);
            }
        }
        // ---------------------------

        // 3. TASTO "INDIETRO" (Modificato per includere più varianti)
        // Aggiungiamo KeypadMinus (il meno del tastierino) e Slash (che spesso è il meno italiano)
        if (Input.GetKeyDown(KeyCode.Minus) ||
            Input.GetKeyDown(KeyCode.KeypadMinus) ||
            Input.GetKeyDown(KeyCode.Slash) || // <--- Spesso il tasto "-" italiano è questo per Unity!
            Input.GetKeyDown(KeyCode.Backspace) ||
            Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackInput();
        }
    }

    // --- LOGICA INTELLIGENTE PER TORNARE INDIETRO ---
    public void HandleBackInput()
    {
        // Se il menu è chiuso, il tasto indietro non fa nulla
        if (!menuRoot.activeSelf) return;

        // PRIORITA' 1: Se siamo nella scheda Dettagli (Livello profondo)
        if (summaryPanel != null && summaryPanel.activeSelf)
        {
            CloseSummary(); // Torna alla lista team
        }
        // PRIORITA' 2: Se siamo nel Menu principale (Livello base)
        else
        {
            ToggleMenu(); // Chiude tutto e torna al gioco
        }
    }

    public void ToggleMenu()
    {
        bool isOpening = !menuRoot.activeSelf;

        if (isOpening)
        {
            // APERTURA
            menuRoot.SetActive(true);
            teamPanel.SetActive(false);
            bagPanel.SetActive(false);

            // Reset pannelli profondi
            if (summaryPanel != null) summaryPanel.SetActive(false);

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerMovementScript != null) playerMovementScript.enabled = false;
            if (cameraOrbitScript != null) cameraOrbitScript.enabled = false;
        }
        else
        {
            // CHIUSURA
            CloseAll();
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerMovementScript != null) playerMovementScript.enabled = true;
            if (cameraOrbitScript != null) cameraOrbitScript.enabled = true;
        }
    }

    public void CloseAll()
    {
        menuRoot.SetActive(false);
        teamPanel.SetActive(false);
        bagPanel.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);
    }

    // --- GESTIONE DETTAGLI ---
    public void OpenSummary(Monster monster)
    {
        teamPanel.SetActive(false);
        summaryPanel.SetActive(true);

        if (summaryScript != null)
        {
            summaryScript.SetData(monster);
        }
    }

    public void CloseSummary()
    {
        summaryPanel.SetActive(false);
        teamPanel.SetActive(true); // Riapre la lista team
    }

    // --- GESTIONE TEAM ---
    public void OpenTeam()
    {
        teamPanel.SetActive(true);
        bagPanel.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);

        foreach (Transform child in teamListContainer) Destroy(child.gameObject);

        if (GameManager.instance != null)
        {
            foreach (Monster monster in GameManager.instance.playerParty)
            {
                GameObject slotObj = Instantiate(memberSlotPrefab, teamListContainer);
                PartyMemberUI uiScript = slotObj.GetComponent<PartyMemberUI>();

                if (uiScript != null)
                {
                    uiScript.SetData(monster);

                    if (uiScript.myButton != null)
                    {
                        Monster currentMonster = monster;
                        uiScript.myButton.onClick.AddListener(() => {
                            OpenSummary(currentMonster);
                        });
                    }
                }
            }
        }
    }

    // --- GESTIONE BAG ---
    public void OpenBag()
    {
        bagPanel.SetActive(true);
        teamPanel.SetActive(false);
        if (summaryPanel != null) summaryPanel.SetActive(false);

        foreach (Transform child in bagListContainer) Destroy(child.gameObject);

        if (GameManager.instance != null && GameManager.instance.inventory != null)
        {
            foreach (var slot in GameManager.instance.inventory.slots)
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, bagListContainer);
                ItemSlotUI uiScript = slotObj.GetComponent<ItemSlotUI>();

                if (uiScript != null)
                {
                    uiScript.SetData(slot);
                    if (uiScript.myButton != null)
                    {
                        string desc = slot.item.description;
                        uiScript.myButton.onClick.AddListener(() => {
                            if (itemDescriptionText != null) itemDescriptionText.text = desc;
                        });
                    }
                }
            }
        }
    }
}