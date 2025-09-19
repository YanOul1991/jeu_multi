using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerServerAuthority : NetworkBehaviour
{
  private UserInputs inputs;
  [field: SerializeField] private Transform m_limit_x;
  [field: SerializeField] private Transform m_limit_z;
  [field: SerializeField] private Transform m_limit_center;
  [field: SerializeField] private Vector3 m_spawn_position;

  // [field: SerializeField] private bool m_player2;

  // Function ref
  private Action m_boundCheck;
  private Action m_deltaCheck;

  private Vector2 m_clientMouseDelta;
  [field: SerializeField] private GameObject m_clientGameObject;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void Awake()
  {
    inputs = new UserInputs();
    Application.targetFrameRate = -1;

    m_limit_x = SceneDataJeu.Singleton.Limit_x;
    m_limit_z = SceneDataJeu.Singleton.Limit_z;
    m_limit_center = SceneDataJeu.Singleton.Limit_center;
  }

  private void Start()
  {
    Cursor.lockState = CursorLockMode.Confined;
    Cursor.visible = false;
  }

  private void OnEnable()
  {
    inputs.Enable();
    transform.position = m_spawn_position;
  }

  private void OnDisable()
  {
    inputs.Disable();
  }

  public override void OnNetworkSpawn()
  {
    base.OnNetworkSpawn();

    if (IsServer)
    {

      m_boundCheck = CheckBoundsPlayer1;
      m_deltaCheck = CheckDeltaPlayer1;
    }
    else
    {
      m_boundCheck = CheckBoundsPlayer2;
      m_deltaCheck = CheckDeltaPlayer2;

      m_spawn_position *= -1;
      Camera.main.transform.localEulerAngles = new Vector3(90, 180, 0);
      Destroy(GetComponent<NetworkRigidbody>());
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (!IsLocalPlayer) return;

    if (IsServer)
    {
      Debug.Log("Client Mouse Delta = " + m_clientMouseDelta);
    }

    m_deltaCheck();
    m_boundCheck();
  }

  [Rpc(SendTo.Server)]
  void SendClientMouseDeltaRpc(Vector2 _value)
  {
    Debug.Log("Client Move: " + _value);
    SendPhysicsDataRpc(_value);
  } 

  [Rpc(SendTo.NotServer)]
  void SendPhysicsDataRpc(Vector2 _value)
  {
    GetComponent<Rigidbody>().AddForce(new(_value.x, 0, _value.y));
    if (_value.magnitude < Mathf.Epsilon)
      GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
  }

  /////////////////////////////////////////////////////////////////
  /////////////////////////////////////////////////////////////////

  void CheckDeltaPlayer1()
  {
    Vector2 mouseDelta = inputs.MapMain.Look.ReadValue<Vector2>();
    mouseDelta *= 1000f;
    // transform.position += new Vector3(mouseDelta.x, 0, mouseDelta.y);
    GetComponent<Rigidbody>().AddForce(new(mouseDelta.x, 0, mouseDelta.y));

    if (mouseDelta.magnitude < Mathf.Epsilon)
      GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
  }

  void CheckDeltaPlayer2()
  {
    Vector2 mouseDelta = inputs.MapMain.Look.ReadValue<Vector2>();
    mouseDelta *= -1000f;
    SendClientMouseDeltaRpc(mouseDelta);
  }

  void CheckBoundsPlayer1()
  {
    if (transform.position.x > m_limit_x.transform.position.x)
      transform.position = new(m_limit_x.transform.position.x, 0, transform.position.z);

    if (transform.position.x < -m_limit_x.transform.position.x)
      transform.position = new(-m_limit_x.transform.position.x, 0, transform.position.z);

    if (transform.position.z < m_limit_z.transform.position.z)
      transform.position = new(transform.position.x, 0, m_limit_z.transform.position.z);

    if (transform.position.z > m_limit_center.transform.position.z)
      transform.position = new(transform.position.x, 0, m_limit_center.transform.position.z);
  }

    void CheckBoundsPlayer2()
  {
    if (transform.position.x > m_limit_x.transform.position.x)
      transform.position = new(m_limit_x.transform.position.x, 0, transform.position.z);

    if (transform.position.x < -m_limit_x.transform.position.x)
      transform.position = new(-m_limit_x.transform.position.x, 0, transform.position.z);

    if (transform.position.z > -m_limit_z.transform.position.z)
      transform.position = new(transform.position.x, 0, -m_limit_z.transform.position.z);

    if (transform.position.z < -m_limit_center.transform.position.z)
      transform.position = new(transform.position.x, 0, -m_limit_center.transform.position.z);
  }
}