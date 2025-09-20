using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/* Script pour charger la scène du jeu ce qui permettra d'initialiser le serveur
*/
public class LoadGame : MonoBehaviour
{
    [field: SerializeField] private SceneAsset m_sceneToLoad;

    void Start()
    {
        SceneManager.LoadScene(m_sceneToLoad.name);
    }
}
