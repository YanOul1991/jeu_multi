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

    // Fonction appelée pour le bouton qui permet de se connecter comme hôte
    public void LanceCommeHote()
    {
        NetworkManager.Singleton.StartHost();
    }

    // Fonction appelée pour le bouton qui permet de se connecter comme client
    public void LanceCommeClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    // L'hôte de la partie attend que 2 joueurs soient connectés pour lancer la partie
    // Seulement l'hôte exécute ce code
    // Aucune vérification si partie déjà en cours
    void Update()
    {
        if (!IsHost) return;
        if (partieEnCours) return;

        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            NouvellePartie();
        }
    }

    // Activation d'une nouvelle partie lorsque 2 joueurs. On appelle la fonction de la balle qui
    // la place au milieu et qui lui donne une vélocité.
    public void NouvellePartie()
    {
        partieEnCours = true;
        Puck.instance.LancePuckMilieu();
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
