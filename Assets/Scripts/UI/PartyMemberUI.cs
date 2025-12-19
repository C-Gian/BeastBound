using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [Header("UI Components (Trascina i figli qui)")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public Slider hpSlider;
    public TMP_Text hpText;
    public Slider expSlider;

    // Nascondiamo il bottone dall'inspector perché se lo trova da solo
    [HideInInspector] public Button myButton;

    void Awake()
    {
        // Cerca il bottone su se stesso
        myButton = GetComponent<Button>();

        if (myButton == null)
        {
            // Se ti dimentichi di aggiungere il componente Button al prefab, questo errore te lo ricorda
            Debug.LogError("ERRORE: Manca il componente Button sul prefab del Pokémon!");
        }
    }

    public void SetData(Monster monster)
    {
        nameText.text = monster._base.monsterName;
        levelText.text = "Lvl " + monster.level;

        // HP
        hpSlider.maxValue = monster.MaxHP;
        hpSlider.value = monster.currentHP;
        hpText.text = monster.currentHP + " / " + monster.MaxHP;

        // EXP
        if (expSlider != null)
        {
            expSlider.maxValue = monster.ExpForNextLevel;
            expSlider.value = monster.currentExp;
        }
    }
}