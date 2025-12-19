using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MonsterSummaryUI : MonoBehaviour
{
    [Header("Info Generali")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text hpText;
    public TMP_Text expText;
    public Image monsterImage;

    [Header("Statistiche")]
    public TMP_Text atkText;
    public TMP_Text defText;
    public TMP_Text spdText;

    [Header("Mosse")]
    public List<TMP_Text> moveTexts;

    public void SetData(Monster monster)
    {
        // 1. Info Base
        nameText.text = monster._base.monsterName;
        levelText.text = "Lvl " + monster.level;
        hpText.text = $"HP: {monster.currentHP}/{monster.MaxHP}";

        // Controllo sicurezza per EXP
        if (monster.ExpForNextLevel > 0)
            expText.text = $"EXP: {monster.currentExp}/{monster.ExpForNextLevel}";
        else
            expText.text = "EXP: Max";

        // 2. Stats
        atkText.text = "Atk: " + monster.Attack;
        defText.text = "Def: " + monster.Defense;
        spdText.text = "Spd: " + monster.Speed;

        // 3. Mosse (CORRETTO)
        foreach (var text in moveTexts) text.text = "-";

        for (int i = 0; i < monster.moves.Count; i++)
        {
            if (i < moveTexts.Count)
            {
                // In base al tuo errore, monster.moves[i] è DIRETTAMENTE il MoveBase.
                // Quindi leggiamo direttamente le proprietà da lì.
                string mName = monster.moves[i].moveName;
                int mPP = monster.moves[i].pp;

                moveTexts[i].text = $"{mName} (PP: {mPP})";
            }
        }
    }
}