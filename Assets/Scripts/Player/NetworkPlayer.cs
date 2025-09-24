using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public sealed class NetworkPlayer : NetworkBehaviour
{
  static public NetworkPlayer Singleton;
  [field: SerializeField] private GameObject m_player1;       
  [field: SerializeField] private GameObject m_player2;
  [field: SerializeField] private Transform  m_limit_x;
  [field: SerializeField] private Transform  m_limit_z;
  [field: SerializeField] private Transform  m_limit_center;

  private UserInputs m_inputs;
  private List<Action> m_updateActions;
  private Vector2 m_hostMouseDelta;
  private Vector2 m_clientMouseDelta;
  private bool m_isReady = false;

  private const float c_deltaDefault = 20000.0f;
  private float m_deltaMultiplier;
  public event Action OnPlayerDisconnected;

  ///////////////////////////////////////////////////////////////////// FUNCTIONS

  private void Awake()
  {
    if (Singleton == null)
      Singleton = this;
    else
      Destroy(gameObject);

    DontDestroyOnLoad(this);

    m_inputs = new UserInputs();
    m_updateActions = new List<Action>();
    m_hostMouseDelta = new Vector2();
    m_clientMouseDelta = new Vector2();

    Application.targetFrameRate = 120;


  }

  public override void OnNetworkSpawn()
  {
    base.OnNetworkSpawn();
#if DEBUG
    Debug.Log("Connected to game");
#endif
    if (IsServer)
    {
      NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
  }

  public override void OnNetworkDespawn()
  {
    base.OnNetworkDespawn();
#if DEBUG
    Debug.Log("Disconnected from game");
#endif
  }

  // Update is called once per frame
  void Update()
  {
    if (!m_isReady) return;
    foreach (Action action in m_updateActions) action();
  }

  void FixedUpdate()
  {
    if (!IsServer || !m_isReady) return;
    ServerPhysicsUpdate();
  }

  //////////////////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////////////////

  private void OnClientConnected(ulong obj)
  {
    if (!IsServer) return;

    int _clientCount = NetworkManager.Singleton.ConnectedClients.Count;


    if (_clientCount >= 2)
    {
      NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
      GameObject _player1 = Instantiate(SceneDataJeu.Singleton.PlayerPrefab);
      GameObject _player2 = Instantiate(SceneDataJeu.Singleton.PlayerPrefab);

      _player1.transform.position = new Vector3(0, 0, -10.0f);
      _player2.transform.position = new Vector3(0, 0, 10.0f);

      _player1.GetComponent<NetworkObject>().SpawnWithOwnership(0);
      _player2.GetComponent<NetworkObject>().SpawnWithOwnership(0);

      ulong _player1Network = _player1.GetComponent<NetworkObject>().NetworkObjectId;
      ulong _player2Network = _player2.GetComponent<NetworkObject>().NetworkObjectId;

      GameStart_Rpc(_player1Network, _player2Network);
      PowerupManager.Singleton.Initialize(_player1Network, _player2Network);
      PowerupManager.Singleton.Begin();
    }
  }

  [Rpc(SendTo.ClientsAndHost)]
  private void GameStart_Rpc(
    ulong _player1,
    ulong _player2)
  {
    m_player1 = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_player1].gameObject;
    m_player2 = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_player2].gameObject;
    m_limit_x = SceneDataJeu.Singleton.Limit_x;
    m_limit_z = SceneDataJeu.Singleton.Limit_z;
    m_limit_center = SceneDataJeu.Singleton.Limit_center;
    m_deltaMultiplier = 1.0f;

    if (IsServer)
    {
      m_updateActions.Add(HostUpdateDelta);
      m_updateActions.Add(ServerCheckNoMouseMove);
      m_updateActions.Add(ServerCheckPlayerBounds);
#if DEBUG
      Debug.Log($"Currently connected player count: {NetworkManager.Singleton.ConnectedClients.Count}");
#endif  
    }
    else
    {
      Camera.main.transform.localEulerAngles = new Vector3(90, 180, 0);
      m_updateActions.Add(ClientUpdateDelta);

      Destroy(m_player1.GetComponent<NetworkRigidbody>());
      Destroy(m_player2.GetComponent<NetworkRigidbody>());

      m_player1.GetComponent<Rigidbody>().isKinematic = true;
      m_player2.GetComponent<Rigidbody>().isKinematic = true;
    }
    m_inputs.Enable();
    Cursor.lockState = CursorLockMode.Confined;
    Cursor.visible = false;
    m_isReady = true;
#if DEBUG
    Invoke(nameof(Disconnect_Rpc), 10f);
#endif
  }

  [Rpc(SendTo.Everyone)]
  private void Disconnect_Rpc()
  {
    if (IsServer)
    {
      NetworkManager.Singleton.SpawnManager.SpawnedObjects[m_player1.GetComponent<NetworkObject>().NetworkObjectId].Despawn(true);
      NetworkManager.Singleton.SpawnManager.SpawnedObjects[m_player2.GetComponent<NetworkObject>().NetworkObjectId].Despawn(true);
    }

    PowerupManager.Singleton.Test_GameEnd();
    NetworkManager.Singleton.Shutdown();
    m_inputs = new UserInputs();
    m_updateActions = new List<Action>();
    m_hostMouseDelta = new Vector2();
    m_clientMouseDelta = new Vector2();
    m_isReady = false;
    OnPlayerDisconnected?.Invoke();
  }

  [Rpc(SendTo.Server)]
  private void SendClientMove_Rpc(Vector2 _clientDelta)
  {
    m_clientMouseDelta = -_clientDelta * m_deltaMultiplier;
  }

  private void ClientUpdateDelta()
  {
    SendClientMove_Rpc(m_inputs.MapMain.Look.ReadValue<Vector2>() * c_deltaDefault) ;
  }
  
  private void HostUpdateDelta()
  {
    m_hostMouseDelta = m_deltaMultiplier * c_deltaDefault * m_inputs.MapMain.Look.ReadValue<Vector2>();
  }

  public void ServerPhysicsUpdate()
  {
    m_player1.GetComponent<Rigidbody>().AddForce(new Vector3(
      m_hostMouseDelta.x, 
      0, 
      m_hostMouseDelta.y
    ));

    m_player2.GetComponent<Rigidbody>().AddForce(new Vector3(
      m_clientMouseDelta.x, 
      0, 
      m_clientMouseDelta.y
    ));
  }
  
  private void ServerCheckNoMouseMove()
  {
    if (m_hostMouseDelta.magnitude < Mathf.Epsilon)
      m_player1.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

    if (m_clientMouseDelta.magnitude < Mathf.Epsilon)
      m_player2.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
  }

  void ServerCheckPlayerBounds()
  {
    // Limites pour le joueur 1
    if (m_player1.transform.position.x > m_limit_x.transform.position.x)
      m_player1.transform.position = new(m_limit_x.transform.position.x, 0, m_player1.transform.position.z);

    if (m_player1.transform.position.x < -m_limit_x.transform.position.x)
      m_player1.transform.position = new(-m_limit_x.transform.position.x, 0, m_player1.transform.position.z);

    if (m_player1.transform.position.z < m_limit_z.transform.position.z)
      m_player1.transform.position = new(m_player1.transform.position.x, 0, m_limit_z.transform.position.z);

    if (m_player1.transform.position.z > m_limit_center.transform.position.z)
      m_player1.transform.position = new(m_player1.transform.position.x, 0, m_limit_center.transform.position.z);

    // Limites pour le joueur 2
    if (m_player2.transform.position.x > m_limit_x.transform.position.x)
      m_player2.transform.position = new(m_limit_x.transform.position.x, 0, m_player2.transform.position.z);

    if (m_player2.transform.position.x < -m_limit_x.transform.position.x)
      m_player2.transform.position = new(-m_limit_x.transform.position.x, 0, m_player2.transform.position.z);

    if (m_player2.transform.position.z > -m_limit_z.transform.position.z)
      m_player2.transform.position = new(m_player2.transform.position.x, 0, -m_limit_z.transform.position.z);

    if (m_player2.transform.position.z < -m_limit_center.transform.position.z)
      m_player2.transform.position = new(m_player2.transform.position.x, 0, -m_limit_center.transform.position.z);
  }
}