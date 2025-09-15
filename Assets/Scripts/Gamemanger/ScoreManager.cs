using UnityEngine;
using TMPro;
using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager instance; // singleton
    [SerializeField] private TMP_Text scoreTxt;
    [SerializeField] private int pointageCible;
    private NetworkVariable<int> scoreHote = new NetworkVariable<int>();
    private NetworkVariable<int> scoreClient = new NetworkVariable<int>();
    //public GameObject pannelVictoire; 
    //public GameObject pannelDefaite; 
    private void Awake()
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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            scoreHote.Value = 0;
            scoreClient.Value = 0;
        }

        scoreHote.OnValueChanged += OnChangementPointageHote;
        scoreClient.OnValueChanged += OnChangementPointageClient;
    }

    /* Méthode appelée lors de la désactivation de l'objet réseau
    - Se désabonne des événements de changement de valeur des scores */
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        scoreHote.OnValueChanged -= OnChangementPointageHote;
        scoreClient.OnValueChanged -= OnChangementPointageClient;
    }
    /* Fonction pour augmenter le score de l'hôte
     - On incrémente le score de l'hôte
     - On vérifie si la partie est terminée*/
    public void AugmenteHoteScore()
    {
        scoreHote.Value++;
        VerifieFinPartie();
    }

    /* Fonction pour augmenter le score du client
     - On incrémente le score du client
     - On vérifie si la partie est terminée*/
    public void AugmenteScoreClient()
    {
        scoreClient.Value++;
        VerifieFinPartie();
    }

    // Méthode pour gérer le changement de valeur du score de l'hôte
    // Elle est appelée à chaque fois que le score de l'hôte change
    // Elle met à jour le texte affiché avec les scores actuels
    private void OnChangementPointageHote(int ancienScoreHote, int nouveauScoreHote)
    {
        if (ancienScoreHote == nouveauScoreHote) return; // Évite de mettre à jour si le score n'a pas changé

        scoreTxt.text = scoreHote.Value + " - " + scoreClient.Value;
    }

    // Méthode pour gérer le changement de valeur du score du client
    // Elle est appelée à chaque fois que le score du client change
    // Elle met à jour le texte affiché avec les scores actuels
    private void OnChangementPointageClient(int ancienScoreClient, int nouveauScoreClient)
    {
        if (ancienScoreClient == nouveauScoreClient) return; // Évite de mettre à jour si le score n'a pas changé

        scoreTxt.text = scoreHote.Value + " - " + scoreClient.Value;
    }


    /* Fonction pour vérifier si la partie est terminée
     - Si le score de l'hôte ou du client atteint le pointage cible, on affiche le panel de victoire ou de défaite
     - On appelle la fonction GagnantHote_ClientRpc ou GagnantClient_ClientRpc selon le cas
     - On appelle la fonction FinPartie du GameManager pour terminer la partie */
    void VerifieFinPartie()
    {
        if (scoreHote.Value >= pointageCible)
        {
            GagnantHote_ClientRpc();
            GameManager.instance.FinPartie();
        }
        else if (scoreClient.Value >= pointageCible)
        {
            GagnantClient_ClientRpc();
            GameManager.instance.FinPartie();
        }
    }
    [Rpc(SendTo.Everyone)]
    private void GagnantHote_ClientRpc()
    {

        if (IsServer)
        {
            //pannelVictoire.SetActive(true);
        }
        else
        {
            //pannelDefaite.SetActive(true);
        }
    }
    /* Fonction RPC pour afficher le panel de victoire pour le client et le panel de défaite pour l'hôte
     - Appelée par le serveur pour tous les clients */
    [Rpc(SendTo.Everyone)]
    private void GagnantClient_ClientRpc()
    {

        if (IsServer)
        {
            //pannelDefaite.SetActive(true);
        }
        else
        {
            //pannelVictoire.SetActive(true);
        }
    }
}
