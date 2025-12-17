using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections; // Necessario per le animazioni (Coroutine)

public class BattleHUD : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text hpText;
    public Slider hpSlider;
    public Image fillImage;
    public Gradient hpGradient;
    private int _maxHP;

    public void SetupHUD(MonsterData data)
    {
        _maxHP = data.maxHP;

        nameText.text = data.monsterName;
        hpSlider.maxValue = data.maxHP;
        hpSlider.value = data.maxHP;
        hpText.text = data.maxHP + "/" + _maxHP;

        // Impostiamo il colore iniziale (Tutto verde)
        fillImage.color = hpGradient.Evaluate(1f);
    }

    // Modifichiamo SetHP per avviare l'animazione
    public void SetHP(int currentHP)
    {
        // Invece di cambiare valore subito, avviamo la transizione fluida
        StartCoroutine(SmoothHPChange(currentHP));
    }

    IEnumerator SmoothHPChange(int targetHP)
    {
        float startValue = hpSlider.value;
        float duration = 1f; // L'animazione dura 1 secondo
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float newValue = Mathf.Lerp(startValue, targetHP, elapsed / duration);
            hpSlider.value = newValue;

            fillImage.color = hpGradient.Evaluate(hpSlider.normalizedValue);

            yield return null;
        }

        hpSlider.value = targetHP;
        hpText.text = targetHP + "/" + _maxHP;
    }
}