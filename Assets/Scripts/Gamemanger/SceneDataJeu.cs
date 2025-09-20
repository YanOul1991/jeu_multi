using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SceneDataJeu : MonoBehaviour
{
    static public SceneDataJeu Singleton;

    [field: SerializeField] public Transform Limit_z { get; private set; }
    [field: SerializeField] public Transform Limit_x { get; private set; }
    [field: SerializeField] public Transform Limit_center { get; private set; }
    [field: SerializeField] public Vector3 DefaultSpawn { get; private set; }

    [Header("UI elements")]
    [field: SerializeField] private GameObject m_mainMenuUI;
    [field: SerializeField] private Button m_boutonStart_host;
    [field: SerializeField] private Button m_boutonStart_client;

    [Header("Prefab Objects")]
    [field: SerializeField] public GameObject PlayerPrefab { get; private set; }
    [field: SerializeField] public GameObject PuckPrefab { get; private set; }

    private void Awake()
    {
        if (Singleton == null) Singleton = this;
        else Destroy(gameObject);

        m_boutonStart_host.onClick.AddListener(StartHost);
        m_boutonStart_client.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        m_mainMenuUI.SetActive(false);
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        m_mainMenuUI.SetActive(false);
    }
}