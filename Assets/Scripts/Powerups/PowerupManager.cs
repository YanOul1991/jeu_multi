using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.Collections;


public class PowerupManager : NetworkBehaviour
{
  public static PowerupManager Singleton;

  private static readonly WaitForSecondsRealtime s_waitTime = new(10.0f);
  private static readonly WaitForSecondsRealtime s_waitSpawn = new(2.0f);

  [field: SerializeField] private int m_powerupCountX;
  [field: SerializeField] private int m_powerupCountZ;
  [field: SerializeField] public GameObject PrefabPowerUp { get; private set; }
  [field: SerializeField] private Material m_powerupDefaultMat;

  private List<Vector3> m_poolPoints;
  private List<Vector3> m_activePoints;
  // private List<Powerup> m_activePowerups;
  private List<ulong> m_poolPowerups;

  Dictionary<PowerupEffects, Material> m_dicEffMaterial;
  Dictionary<ulong, PowerupEffects> m_dicActivePowerups;

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
    // m_activePowerups = new List<Powerup>();
    m_poolPowerups = new List<ulong>();
    m_poolPoints = new List<Vector3>();
    m_activePoints = new List<Vector3>();
    m_dicActivePowerups = new Dictionary<ulong, PowerupEffects>();
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
      m_poolPowerups.Add(_instance.GetComponent<NetworkObject>().NetworkObjectId);

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
      // Get NetworkObject
      ulong _powerup = m_poolPowerups[i];

      int _posIndex = UnityEngine.Random.Range(0, m_poolPoints.Count);

      // Get GameObject with associated networkID and set its position
      GameObject _obj = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_powerup].gameObject;

      m_activePoints.Add(m_poolPoints[_posIndex]);
      _obj.transform.position = m_poolPoints[_posIndex];
      m_poolPoints.RemoveAt(_posIndex);

      PowerupEffects _eff = (PowerupEffects)UnityEngine.Random.Range(0, (int)PowerupEffects.Count);

      m_dicActivePowerups.Add(m_poolPowerups[i], _eff);
      m_poolPowerups.RemoveAt(i);

      // Appeler RPC pour activer le bon powerup
      NetworkActivatePowerup_Rpc(_powerup, (byte)_eff);
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
    foreach (KeyValuePair<ulong, PowerupEffects> pair in m_dicActivePowerups)
    {
      m_poolPowerups.Add(pair.Key);
      NetworkDeactivatePowerup_Rpc(pair.Key);
    }

    foreach (Vector3 _point in m_activePoints)
    {
      m_poolPoints.Add(_point);
    }

    m_dicActivePowerups.Clear();
    m_activePoints.Clear();
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

  [Rpc(SendTo.Server)]
  public void NetworkPowerupHit_Rpc(ulong _player, ulong _powerupObj)
  {
    Debug.Log("Applying effect to player");

    if (!IsServer) return;
    NetworkObject _networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_player];
    PowerupEffects _effect = m_dicActivePowerups[_powerupObj];

    NetworkDeactivatePowerup_Rpc(_powerupObj);

    switch (_effect)
    {
    case PowerupEffects.grow :
      _networkObject.gameObject.transform.localScale *= 2;
        break;
    case PowerupEffects.shrink :
      _networkObject.gameObject.transform.localScale /= 2;
        break;
    case PowerupEffects.slow :
      _networkObject.gameObject.transform.localScale *= 2;
      break;
    default:
        break;
    }

    StartCoroutine(ResetEffect(_player, m_dicActivePowerups[_powerupObj]));
  }

  private IEnumerator ResetEffect(ulong _target, PowerupEffects _effect)
  {
    Debug.Log("Starting countdown to reset");
    yield return new WaitForSeconds(8.0f);

    NetworkObject _networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_target];
    
    switch (_effect)
    {
      case PowerupEffects.grow:
        _networkObject.gameObject.transform.localScale /= 2;
        break;
      case PowerupEffects.shrink:
        _networkObject.gameObject.transform.localScale *= 2;
        break;
      case PowerupEffects.slow:
        _networkObject.gameObject.transform.localScale *= 2;
        break;
      default:
        break;
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
  }

  [Rpc(SendTo.Everyone)]
  private void NetworkActivatePowerup_Rpc(ulong _target, byte _mType)
  {
    GameObject _gameObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_target].gameObject;
    _gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = m_dicEffMaterial[(PowerupEffects)_mType];
    _gameObject.SetActive(true);
  }
}