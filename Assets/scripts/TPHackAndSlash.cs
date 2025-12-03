using UnityEngine;
using UnityEngine.SceneManagement;
public class TPHackAndSlash : MonoBehaviour
{
    [Tooltip("HackAndSlash")]
    public string sceneToLoad;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}