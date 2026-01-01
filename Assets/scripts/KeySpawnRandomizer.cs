using UnityEngine;

public class KeySpawnRandomizer : MonoBehaviour
{
    [Header("Key Spawn Points")]
    public GameObject[] keyObjects;

    void Start()
    {
        if (keyObjects.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, keyObjects.Length); // Select a random key

        for (int i = 0; i < keyObjects.Length; i++)
        {
            if (i != randomIndex)
            {
                if (keyObjects[i] != null)
                {
                    Destroy(keyObjects[i]); // Destroy all keys except the randomly selected one
                }
            }
        }
    }
}