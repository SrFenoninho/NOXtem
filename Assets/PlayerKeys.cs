using UnityEngine;
using System.Collections.Generic;

public class PlayerKeys : MonoBehaviour
{
    private List<string> keys = new List<string>();

    public void AddKey(string keyID)
    {
        if (!keys.Contains(keyID))
        {
            keys.Add(keyID);
        }
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