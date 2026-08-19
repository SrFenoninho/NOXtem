using UnityEngine;
using System.Collections.Generic;

public class PlayerKeys : MonoBehaviour
{




    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    private List<string> keys = new List<string>();





    // ---------------------------------------------
    //  PUBLIC METHODS
    // ---------------------------------------------
    public void AddKey(string keyID)
    {
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

    public List<string> GetKeys()
    {
        return keys;
    }

    public void ClearKeys()
    {
        keys.Clear();
    }
}
