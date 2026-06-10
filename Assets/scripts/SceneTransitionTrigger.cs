using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Scene Transition Settings")]
    [Tooltip("Nome exato da Scene para onde queres ir (ex: Level2)")]
    public string sceneName; 
    
    [Tooltip("Marca esta caixa se preferires usar o número (Build Index) em vez do nome")]
    public bool useBuildIndex = false; 
    public int sceneBuildIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (useBuildIndex)
            {
                SceneManager.LoadScene(sceneBuildIndex);
            }
            else
            {
                if (!string.IsNullOrEmpty(sceneName))
                {
                    SceneManager.LoadScene(sceneName);
                }
                else
                {
                    Debug.LogWarning("Aviso: O SceneTransitionTrigger não tem nenhum nome de cena definido!");
                }
            }
        }
    }
}
