using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;


public class PowerupManager : NetworkBehaviour
{
  public static PowerupManager Singleton;

  private static readonly WaitForSecondsRealtime s_waitTime = new(5.0f);
  private static readonly WaitForSecondsRealtime s_waitSpawn = new(1.0f);

  [field: SerializeField] private int m_powerupCountX;
  [field: SerializeField] private int m_powerupCountZ;
  [field: SerializeField] public GameObject PrefabPowerUp { get; private set; }
  [field: SerializeField] private Material m_powerupDefaultMat;

  private List<Vector3> m_poolPoints;
  private List<Vector3> m_activePoints;
  private List<Powerup> m_activePowerups;
  private List<Powerup> m_poolPowerups;

  Dictionary<PowerupEffects, Material> m_dicEffMaterial;

  private void Awake()
  {
    if (Singleton == null)
    {
      Singleton = this;
    }
    else
    {
      Destroy(gameObject);
      return;
    }

    InitMaterials();
  }

  public void Begin()
  {
    if (!IsServer) return;
    SpawnPowerups();
    StartCoroutine(SpawnPeriodic());
  }

  public void Initialize()
  {
    // m_poolPowerUp = new Queue<GameObject>();
    m_activePowerups = new List<Powerup>();
    m_poolPowerups = new List<Powerup>();
    m_poolPoints = new List<Vector3>();
    m_activePoints = new List<Vector3>();
    // InitMaterials();

    float _pSizeX = PrefabPowerUp.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh.bounds.size.x;
    float _pSizeZ = PrefabPowerUp.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh.bounds.size.z;

    float _top = -SceneDataJeu.Singleton.Limit_center.transform.position.z;
    float _bottom = SceneDataJeu.Singleton.Limit_center.transform.position.z;
    float _left = -SceneDataJeu.Singleton.Limit_x.transform.position.x;
    float _right = SceneDataJeu.Singleton.Limit_x.transform.position.x;

    float _spacingX = (_right - _left - _pSizeX * m_powerupCountX) / (m_powerupCountX + 1);
    float _spacingZ = (_top - _bottom - _pSizeZ * m_powerupCountZ) / (m_powerupCountZ + 1);

    int _columnGrid = 0;
    int _rowGrid = -1;

    for (int i = 0; i < (m_powerupCountX * m_powerupCountZ); i++)
    {
      if (_columnGrid % m_powerupCountX == 0)
      {
        _columnGrid = 0;
        _rowGrid++;
      }

      GameObject _instance = Instantiate(
        PrefabPowerUp,
        new Vector3(15, 15, 15),
        Quaternion.identity
      );

      _instance.GetComponent<NetworkObject>().SpawnWithOwnership(0);
      NetworkDeactivatePowerup_Rpc(_instance.GetComponent<NetworkObject>().NetworkObjectId);

      m_poolPowerups.Add(new Powerup
      {
        obj = _instance.GetComponent<NetworkObject>().NetworkObjectId
      });

      m_poolPoints.Add(new Vector3(
        _left + _spacingX + (_pSizeX / 2) + ((_pSizeX + _spacingX) * _columnGrid),
        0,
        _bottom + _spacingZ + (_pSizeZ / 2) + ((_pSizeZ + _spacingZ) * _rowGrid)
      ));

      _columnGrid++;
    }
  }

  private void SpawnPowerups()
  {
    if (!IsServer) return;
    
    for (int i = 0; i < 6; i++)
    {
      // Get NetworkObject ref
      Powerup _powerup = m_poolPowerups[i];
      int _posIndex = UnityEngine.Random.Range(0, m_poolPoints.Count);

      // Get GameObject with associated networkID and set its position
      GameObject _obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_powerup.obj].gameObject;

      m_activePoints.Add(m_poolPoints[_posIndex]);
      _obj.transform.position = m_poolPoints[_posIndex];
      m_poolPoints.RemoveAt(_posIndex);

      PowerupEffects _eff = (PowerupEffects)UnityEngine.Random.Range(0, (int)PowerupEffects.Count);
      _powerup.effect = _eff;

      m_activePowerups.Add(m_poolPowerups[i]);
      m_poolPowerups.RemoveAt(i);
      
      // Appeler RPC pour activer le bon powerup
      NetworkActivatePowerup_Rpc(_powerup.obj, (byte)_eff);
    }
  }

  /// <summary>
  /// Initialization des materiaux pour les powerups
  /// </summary>
  void InitMaterials()
  {
    m_dicEffMaterial = new Dictionary<PowerupEffects, Material>();
    for (int i = 0; i < (int)PowerupEffects.Count; i++)
      m_dicEffMaterial.Add((PowerupEffects)i, new Material(m_powerupDefaultMat));

    m_dicEffMaterial[PowerupEffects.grow].color = Color.red;
    m_dicEffMaterial[PowerupEffects.shrink].color = Color.green;
    m_dicEffMaterial[PowerupEffects.slow].color = Color.blue;
  }

  private void ResetPowerups()
  {
    foreach (Powerup _powerup in m_activePowerups)
    {
      m_poolPowerups.Add(_powerup);
      NetworkDeactivatePowerup_Rpc(_powerup.obj);
    }

    foreach (Vector3 _point in m_activePoints)
    {
      m_poolPoints.Add(_point);
    }

    m_activePowerups = new List<Powerup>();
    m_activePoints = new List<Vector3>();
  }

  private IEnumerator SpawnPeriodic()
  {
    while (true)
    {
      yield return s_waitTime;
      ResetPowerups();
      yield return s_waitSpawn;
      SpawnPowerups();
    }
  }

  //////////////////////////////////////////////////////////////////////////////
  /////////////////////////////////////////////////// Remote Network Procedures
  //////////////////////////////////////////////////////////////////////////////

  [Rpc(SendTo.Everyone)]
  private void NetworkDeactivatePowerup_Rpc(ulong _target)
  {
    GameObject _gameObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_target].gameObject;
    _gameObject.SetActive(false);
    _gameObject.isStatic = true;
  }

  [Rpc(SendTo.Everyone)]
  private void NetworkActivatePowerup_Rpc(ulong _target, byte _mType)
  {
    GameObject _gameObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_target].gameObject;
    _gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = m_dicEffMaterial[(PowerupEffects)_mType];
    _gameObject.SetActive(true);
  }
}