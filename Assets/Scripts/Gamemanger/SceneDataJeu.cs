using System;
using System.Collections.Generic;
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


  [Header("PowerUps")]
  [field: SerializeField] private int m_powerupCountX;
  [field: SerializeField] private int m_powerupCountZ;
  [field: SerializeField] public GameObject PrefabPowerUp { get; private set; }
  private List<GameObject> m_poolPowerUp;
  private List<Vector3> m_powerupsGridPoints;

  private void Awake()
  {
    if (Singleton == null) Singleton = this;
    else Destroy(gameObject);

    m_boutonStart_host.onClick.AddListener(StartHost);
    m_boutonStart_client.onClick.AddListener(StartClient);
    m_poolPowerUp = new List<GameObject>();
  }

  private void Start()
  {
    GameObject instance = Instantiate(PrefabPowerUp, Vector3.zero, Quaternion.identity);
    m_poolPowerUp.Add(instance);

    Powerup _sp = m_poolPowerUp[0].GetComponent<Powerup>();

    float _top = -Limit_center.transform.position.z;
    float _bottom = Limit_center.transform.position.z;
    float _left = -Limit_x.transform.position.x;
    float _right = Limit_x.transform.position.x;

    for (int i = 0; i < (m_powerupCountX * m_powerupCountZ) - 1; i++)
    {
      instance = Instantiate(PrefabPowerUp);
      m_poolPowerUp.Add(instance);
    }

    float _spacingX = (_right - _left - _sp.SizeX * m_powerupCountX) / (m_powerupCountX + 1);
    float _spacingZ = (_top - _bottom - _sp.SizeZ * m_powerupCountZ) / (m_powerupCountZ + 1);

    int _columnGrid = 0;
    int _rowGrid = -1;
    
    for (int i = 0; i < (m_powerupCountX * m_powerupCountZ); i++)
    {
      if (_columnGrid % m_powerupCountX == 0)
      {
        _columnGrid = 0;
        _rowGrid++;
      }

      m_poolPowerUp[i].transform.position = new Vector3(
        _left + _spacingX + (_sp.SizeX / 2) + ((_sp.SizeX + _spacingX) * _columnGrid),
        0,
        _bottom + _spacingZ + (_sp.SizeZ / 2) + ((_sp.SizeZ + _spacingZ) * _rowGrid)
      );

      _columnGrid++;
    }
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