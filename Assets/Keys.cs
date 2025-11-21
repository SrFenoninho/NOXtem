using UnityEngine;

public class Keys : MonoBehaviour
{
    public string keyName = "Door";
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerKeys playerKeys = other.GetComponent<PlayerKeys>();
            if (playerKeys != null)
            {
                playerKeys.AddKey(keyName);
                Debug.Log($"Apanhaste a chave: {keyName}.");
                Destroy(gameObject);
            }
        }
    }
}
