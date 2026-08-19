using UnityEngine;

public class KeySpawnRandomizer : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Key Spawn Points")]

    public GameObject[] keyObjects;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (keyObjects.Length == 0) return;

        int randomIndex = Random.Range(0, keyObjects.Length);

        for (int i = 0; i < keyObjects.Length; i++)
        {
            if (i != randomIndex && keyObjects[i] != null)
                Destroy(keyObjects[i]);
        }
    }
}
