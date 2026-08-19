using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{



    // ---------------------------------------------
    //  INSPECTOR
    // ---------------------------------------------
    [Header("Scene Transition Settings")]



    // ---------------------------------------------
    //  PRIVATE STATE
    // ---------------------------------------------
    [Tooltip("Nome exato da Scene para onde queres ir (ex: Level2)")]

    public string sceneName; 

    [Tooltip("Marca esta caixa se preferires usar o número (Build Index) em vez do nome")]
    public bool useBuildIndex = false; 
    public int sceneBuildIndex;





    // ---------------------------------------------
    //  UNITY
    // ---------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerKeys keys = other.GetComponent<PlayerKeys>();
            if (keys != null)
            {
                string chavesJuntas = string.Join(",", keys.GetKeys());
                PlayerPrefs.SetString("PlayerKeys", chavesJuntas);
                PlayerPrefs.Save();
            }

            if (useBuildIndex)
            {
                LoadingManager.Carregar(sceneBuildIndex);
            }
            else
            {
                if (!string.IsNullOrEmpty(sceneName))
                {
                    LoadingManager.Carregar(sceneName);
                }
                else
                {
                }
            }
        }
    }
}
