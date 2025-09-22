using UnityEngine;
using Unity.Netcode; 
using UnityEngine.SceneManagement; 
public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    public bool partieEnCours { private set; get; } 
    public bool partieTerminee { private set; get; } 

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!IsHost) return;
        if (partieEnCours) return;

        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            NouvellePartie();
        }
    }

    public void NouvellePartie()
    {
        partieEnCours = true;
        Puck.instance.PlacementPuck(new Vector3(0, 0, 0));
    }

   // Fonction appelée par le ScoreManager pour terminer la partie
    public void FinPartie()
    {
        partieTerminee = true;
    }

    // Fonction appelée par le bouton Recommencer pour recommencer une partie
    public void Recommencer()
    {
        NetworkManager.Singleton.Shutdown(); 
        Destroy(NetworkManager.gameObject);
        partieEnCours = false; 
        SceneManager.LoadScene(0);
    }
}
