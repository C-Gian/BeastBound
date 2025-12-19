using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    // Una piccola classe interna per definire "Oggetto + Quantità"
    [System.Serializable]
    public class ItemSlot
    {
        public ItemBase item;
        public int count;
    }

    [Header("Contenuto Zaino")]
    public List<ItemSlot> slots; // La lista visibile nell'Inspector

    // Funzione per aggiungere un oggetto
    public void AddItem(ItemBase itemToAdd, int count = 1)
    {
        // 1. Controlla se abbiamo già questo oggetto nello zaino
        foreach (ItemSlot slot in slots)
        {
            if (slot.item == itemToAdd)
            {
                slot.count += count; // Aumenta solo il numero
                Debug.Log("Aggiunto " + count + " " + itemToAdd.itemName + ". Totale: " + slot.count);
                return;
            }
        }

        // 2. Se non c'era, crea un nuovo slot
        ItemSlot newSlot = new ItemSlot();
        newSlot.item = itemToAdd;
        newSlot.count = count;
        slots.Add(newSlot);
        Debug.Log("Nuovo oggetto ottenuto: " + itemToAdd.itemName);
    }

    // Funzione per rimuovere/usare un oggetto
    public bool RemoveItem(ItemBase itemToRemove, int count = 1)
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.item == itemToRemove)
            {
                if (slot.count >= count)
                {
                    slot.count -= count;
                    Debug.Log("Usato " + count + " " + itemToRemove.itemName + ". Rimasti: " + slot.count);

                    // Se arriviamo a 0, potremmo voler rimuovere lo slot dalla lista, 
                    // ma per ora lasciamolo a 0 così ci ricordiamo di averlo avuto.
                    if (slot.count == 0)
                    {
                        slots.Remove(slot);
                    }
                    return true; // Operazione riuscita
                }
                else
                {
                    Debug.LogWarning("Non hai abbastanza " + itemToRemove.itemName);
                    return false; // Fallito
                }
            }
        }
        Debug.LogWarning("Oggetto non trovato nell'inventario!");
        return false;
    }
}