using UnityEngine;

public class KeySpawnRandomizer : MonoBehaviour
{
    // ---------------------------------------------
    //  INSPETOR
    // ---------------------------------------------
    [Header("Key Spawn Points")]
    public GameObject[] keyObjects; // todos os possiveis locais de spawn da chave

    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    void Start()
    {
        if (keyObjects.Length == 0) return;

        // Escolher aleatoriamente qual a chave que vai aparecer
        int randomIndex = Random.Range(0, keyObjects.Length);

        // Destruir todas as chaves exceto a selecionada
        for (int i = 0; i < keyObjects.Length; i++)
        {
            if (i != randomIndex && keyObjects[i] != null)
                Destroy(keyObjects[i]);
        }
    }
}
