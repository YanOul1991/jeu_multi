using System;
using System.Collections.Generic;
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

  private const float c_delatMultiplier = 20000.0f;

  private UserInputs m_inputs;
  private List<Action> m_updateActions;
  private bool m_isReady = false;

  private Vector2 m_hostMouseDelta;
  private Vector2 m_clientMouseDelta;

  ///////////////////////////////////////////////////////////////////// FUNCTIONS

  private void Awake()
  {
    if (Singleton == null)
    {
      Singleton = this;
    }
    else
    {
      Destroy(gameObject);
    }

    m_inputs = new UserInputs();
    m_updateActions = new List<Action>();
    m_clientMouseDelta = new Vector2();

    Application.targetFrameRate = -1;
    Cursor.lockState = CursorLockMode.Confined;
    Cursor.visible = false;
  }

  private void Start()
  {
    if (IsClient)
    {
      Debug.Log("Turning camera for client");
      Camera.main.transform.localEulerAngles = new Vector3(90, 180, 0);
    }
  }

  public override void OnNetworkSpawn()
  {
    base.OnNetworkSpawn();

    if (IsServer)
    {
      NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
      return;
    }
    else
    {
      Debug.Log("Client Spawned!!!");
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (!m_isReady) return;
    foreach (Action action in m_updateActions) action();
  }

  //////////////////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////////////////

  private void OnClientConnected(ulong obj)
  {
    int _clientCount = NetworkManager.Singleton.ConnectedClients.Count;

    if (_clientCount >= 2)
    {
      GameObject _player1 = Instantiate(SceneDataJeu.Singleton.PlayerPrefab);
      GameObject _player2 = Instantiate(SceneDataJeu.Singleton.PlayerPrefab);

      _player1.transform.position = new Vector3(0, 0, -10.0f);
      _player2.transform.position = new Vector3(0, 0, 10.0f);

      _player1.GetComponent<NetworkObject>().SpawnWithOwnership(0);
      _player2.GetComponent<NetworkObject>().SpawnWithOwnership(0);

      ulong _player1_NetworkID = _player1.GetComponent<NetworkObject>().NetworkObjectId;
      ulong _player2_NetworkID = _player2.GetComponent<NetworkObject>().NetworkObjectId;

      GameStart_Rpc(_player1_NetworkID, _player2_NetworkID);
    }
  }

  [Rpc(SendTo.Everyone)]
  private void GameStart_Rpc(
    ulong _player1_NetworkID,
    ulong _player2_NetworkID)
  {
    m_player1 = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_player1_NetworkID].gameObject;
    m_player2 = NetworkManager.Singleton.SpawnManager.SpawnedObjects[_player2_NetworkID].gameObject;
    m_limit_x = SceneDataJeu.Singleton.Limit_x;
    m_limit_z = SceneDataJeu.Singleton.Limit_z;
    m_limit_center = SceneDataJeu.Singleton.Limit_center;

    if (IsServer)
    {
      Debug.Log("Starting game as server");
      m_updateActions.Add(CheckDeltaPlayer1);
      m_updateActions.Add(CheckDeltaPlayer2);
      m_updateActions.Add(CheckBoundsPlayer1);
      m_updateActions.Add(CheckBoundsPlayer2);
    }
    else
    {
      Debug.Log("Starting game as client");
      Camera.main.transform.localEulerAngles = new Vector3(90, 180, 0);
      m_updateActions.Add(LocalUpdateDelta);
      Destroy(m_player1.GetComponent<NetworkRigidbody>());
      Destroy(m_player2.GetComponent<NetworkRigidbody>());
      
      m_player1.GetComponent<Rigidbody>().isKinematic = true;
      m_player2.GetComponent<Rigidbody>().isKinematic = true;
    }

    m_inputs.Enable();
    m_isReady = true;
  }

  [Rpc(SendTo.Server)]
  private void SendClientMove_Rpc(Vector2 _clientDelta)
  {
    m_clientMouseDelta = -_clientDelta;
  }

  private void LocalUpdateDelta()
  {
    Vector2 mouseDelta = m_inputs.MapMain.Look.ReadValue<Vector2>();
    mouseDelta *= c_delatMultiplier;
    SendClientMove_Rpc(mouseDelta);
  }

  public void ServerPhysicsUpdate()
  {

  }

  private void CheckDeltaPlayer1()
  {

    Vector2 mouseDelta = m_inputs.MapMain.Look.ReadValue<Vector2>();
    mouseDelta *= c_delatMultiplier;
    m_player1.GetComponent<Rigidbody>().AddForce(new(mouseDelta.x, 0, mouseDelta.y));

    if (mouseDelta.magnitude < Mathf.Epsilon)
      m_player1.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
  }

  private void CheckDeltaPlayer2()
  {
    m_player2.GetComponent<Rigidbody>().AddForce(new(m_clientMouseDelta.x, 0, m_clientMouseDelta.y));
    if (m_clientMouseDelta.magnitude < Mathf.Epsilon)
      m_player2.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
  }
  
  void CheckBoundsPlayer1()
  {
    if (m_player1.transform.position.x > m_limit_x.transform.position.x)
      m_player1.transform.position = new(m_limit_x.transform.position.x, 0, m_player1.transform.position.z);

    if (m_player1.transform.position.x < -m_limit_x.transform.position.x)
      m_player1.transform.position = new(-m_limit_x.transform.position.x, 0, m_player1.transform.position.z);

    if (m_player1.transform.position.z < m_limit_z.transform.position.z)
      m_player1.transform.position = new(m_player1.transform.position.x, 0, m_limit_z.transform.position.z);

    if (m_player1.transform.position.z > m_limit_center.transform.position.z)
      m_player1.transform.position = new(m_player1.transform.position.x, 0, m_limit_center.transform.position.z);
  }
  
  void CheckBoundsPlayer2()
  {
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