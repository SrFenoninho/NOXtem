using UnityEngine;
using UnityEngine.SceneManagement;

public class TPHackAndSlash : MonoBehaviour
{
    public string sceneToLoad;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadingManager.Carregar(sceneToLoad);
        }
    }
}
