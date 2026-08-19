using UnityEngine;
using UnityEngine.SceneManagement;

public class TPHackAndSlash : MonoBehaviour
{




    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    public string sceneToLoad;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LoadingManager.Carregar(sceneToLoad);
        }
    }
}
