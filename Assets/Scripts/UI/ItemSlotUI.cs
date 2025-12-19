using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Header("Cose FIGLIE da trascinare")]
    public TMP_Text itemText; // Questo è un figlio, quindi va trascinato manualmente nel prefab

    // Questa variabile la nascondiamo dall'Inspector ([HideInInspector]) 
    // perché se la calcola da sola, non devi toccarla tu!
    [HideInInspector] public Button myButton;

    void Awake()
    {
        // "GetComponent" dice: Cerca su QUESTO STESSO OGGETTO un componente di tipo Button
        myButton = GetComponent<Button>();

        // Controllo di sicurezza
        if (myButton == null)
        {
            Debug.LogError("ERRORE: Manca il componente Button su " + gameObject.name);
        }
    }

    public void SetData(Inventory.ItemSlot slot)
    {
        if (itemText != null)
        {
            itemText.text = slot.item.itemName + " x" + slot.count;
        }
    }
}