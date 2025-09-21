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
  private Vector3[] m_powerupsPoints;
  private Queue<GameObject> m_poolPowerUp;
  private List<GameObject> m_poolActivePowerup;
  [field: SerializeField] private List<Powerup> m_poolpow;

  [Header("Powerups Effects")]
  [field: SerializeField] private Material m_powerupDefaultMat;

  Dictionary<PowerupEffects, Material> m_dicEffMaterial;

  private void Awake()
  {
    if (Singleton == null) Singleton = this;
    else Destroy(gameObject);

    m_boutonStart_host.onClick.AddListener(StartHost);
    m_boutonStart_client.onClick.AddListener(StartClient);

    m_poolPowerUp = new Queue<GameObject>();
    m_poolActivePowerup = new List<GameObject>();
    m_poolpow = new List<Powerup>();

    InitMaterials();
  }

  private void Start()
  {
    SetPowerupsGrid();
    SpawnPowerups();
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

  private void SetPowerupsGrid()
  {
    float _pSizeX = PrefabPowerUp.GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
    float _pSizeZ = PrefabPowerUp.GetComponent<MeshFilter>().sharedMesh.bounds.size.z;

    float _top = -Limit_center.transform.position.z;
    float _bottom = Limit_center.transform.position.z;
    float _left = -Limit_x.transform.position.x;
    float _right = Limit_x.transform.position.x;

    float _spacingX = (_right - _left - _pSizeX * m_powerupCountX) / (m_powerupCountX + 1);
    float _spacingZ = (_top - _bottom - _pSizeZ * m_powerupCountZ) / (m_powerupCountZ + 1);

    int _columnGrid = 0;
    int _rowGrid = -1;

    m_powerupsPoints = new Vector3[m_powerupCountX * m_powerupCountZ];

    for (int i = 0; i < (m_powerupCountX * m_powerupCountZ); i++)
    {
      if (_columnGrid % m_powerupCountX == 0)
      {
        _columnGrid = 0;
        _rowGrid++;
      }

      // m_poolPowerUp.Enqueue(Instantiate(
      //   PrefabPowerUp,
      //   new Vector3(15, 15, 15),
      //   Quaternion.identity
      // ));

      m_poolpow.Add(new Powerup
      {
        obj = Instantiate(
          PrefabPowerUp,
          new Vector3(15, 15, 15),
          Quaternion.identity
        ),
        effect = PowerupEffects.grow
      });

      m_powerupsPoints[i] = new Vector3(
        _left + _spacingX + (_pSizeX / 2) + ((_pSizeX + _spacingX) * _columnGrid),
        0,
        _bottom + _spacingZ + (_pSizeZ / 2) + ((_pSizeZ + _spacingZ) * _rowGrid)
      );

      _columnGrid++;
    }
  }

  private void SpawnPowerups()
  {
    for (int i = 0; i < 10; i++)
    {
      // GameObject _powerUp = m_poolPowerUp.Dequeue();
      // m_poolActivePowerup.Add(_powerUp);
      // _powerUp.transform.position = m_powerupsPoints[UnityEngine.Random.Range(0, m_powerupsPoints.Length)];

      Powerup p = m_poolpow[i];
      p.obj.transform.position = m_powerupsPoints[UnityEngine.Random.Range(0, m_powerupsPoints.Length)];

      PowerupEffects _eff = (PowerupEffects)UnityEngine.Random.Range(0, (int)PowerupEffects.Count);
      p.obj.GetComponent<MeshRenderer>().material = m_dicEffMaterial[_eff];
      p.effect = _eff;
      m_poolpow[i] = p;
    }
  }

  private void InitMaterials()
  {
    m_dicEffMaterial = new Dictionary<PowerupEffects, Material>();

    for (int i = 0; i < (int)PowerupEffects.Count; i++)
      m_dicEffMaterial.Add((PowerupEffects)i, new Material(m_powerupDefaultMat));
      
    m_dicEffMaterial[PowerupEffects.grow].color = Color.red;
    m_dicEffMaterial[PowerupEffects.shrink].color = Color.green;
    m_dicEffMaterial[PowerupEffects.slow].color = Color.blue;
  }
}