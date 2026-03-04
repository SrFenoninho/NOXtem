using UnityEngine;
using System.Collections.Generic;

public class PlayerKeys : MonoBehaviour
{
    // ---------------------------------------------
    //  INVENTÁRIO DE CHAVES
    // ---------------------------------------------
    // Lista de IDs das chaves que o jogador carrega
    private List<string> keys = new List<string>();

    public void AddKey(string keyID)
    {
        // Evitar duplicados no inventário
        if (!keys.Contains(keyID))
            keys.Add(keyID);
    }

    public bool HasKey(string keyID)
    {
        return keys.Contains(keyID);
    }

    public void RemoveKey(string keyID)
    {
        keys.Remove(keyID);
    }
}
