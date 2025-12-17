using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class BattleHUD : MonoBehaviour
{
    [Header("Testi")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text hpText;

    [Header("Barre")]
    public Slider hpSlider;
    public Image hpFillImage;
    public Gradient hpGradient;

    public Slider expSlider;

    private int _maxHP;

    public void SetupHUD(Monster monster)
    {
        _maxHP = monster.MaxHP;
        nameText.text = monster._base.monsterName;
        SetLevelText(monster.level);

        hpSlider.maxValue = monster.MaxHP;
        hpSlider.value = monster.currentHP;
        hpText.text = monster.currentHP + "/" + _maxHP;

        float normalizedHP = (float)monster.currentHP / monster.MaxHP;
        if (hpFillImage != null)
            hpFillImage.color = hpGradient.Evaluate(normalizedHP);

        // Setup EXP Iniziale (Senza animazione)
        if (expSlider != null)
        {
            expSlider.maxValue = monster.ExpForNextLevel;
            expSlider.value = monster.currentExp;
        }
    }

    // --- FUNZIONI DI AGGIORNAMENTO ---

    public void SetHP(int currentHP)
    {
        StartCoroutine(SmoothHPChange(currentHP));
    }

    // NUOVO: Funzione per impostare l'EXP istantaneamente (utile per il reset a 0)
    public void SetExpInstant(int value)
    {
        if (expSlider != null) expSlider.value = value;
    }

    public void SetLevelText(int level)
    {
        if (levelText != null)
            levelText.text = "Lvl " + level;
    }

    // --- COROUTINES (ANIMAZIONI) ---

    IEnumerator SmoothHPChange(int targetHP)
    {
        float startValue = hpSlider.value;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetHP, elapsed / duration);
            hpSlider.value = newValue;

            if (hpFillImage != null)
                hpFillImage.color = hpGradient.Evaluate(hpSlider.normalizedValue);

            yield return null;
        }
        hpSlider.value = targetHP;
        hpText.text = targetHP + "/" + _maxHP;
    }

    public IEnumerator SmoothExpChange(int targetExp)
    {
        if (expSlider == null) yield break;

        float startValue = expSlider.value;
        float duration = 1.0f; // Durata normale dell'animazione (1 secondo)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // LOGICA FAST FORWARD:
            // Se tengo premuto Z (o Spazio), la velocità è 5x, altrimenti 1x
            float speedMultiplier = (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.Space)) ? 5f : 1f;

            // Incrementiamo il tempo trascorso moltiplicato per la velocità
            elapsed += Time.deltaTime * speedMultiplier;

            // Interpolazione
            expSlider.value = Mathf.Lerp(startValue, targetExp, elapsed / duration);
            yield return null;
        }
        expSlider.value = targetExp;
    }
}