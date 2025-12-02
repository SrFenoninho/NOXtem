using UnityEngine;
using System.Collections.Generic; // necessary for using lists

public class PlayerKeys : MonoBehaviour
{
    private List<string> keys = new List<string>(); // list makes a list of strings

    public void AddKey(string keyID) // add a key to the player's inventory
    {
        if (!keys.Contains(keyID))
        {
            keys.Add(keyID);
        }
    }

    public bool HasKey(string keyID) // check if the player has a key
    {
        return keys.Contains(keyID);
    }

    public void RemoveKey(string keyID) // remove a key from the player's inventory
    {
        keys.Remove(keyID);
    }
}