using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : NetworkBehaviour
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

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void Awake()
  {
    inputs = new UserInputs();
    Application.targetFrameRate = -1;

    m_limit_x = SceneDataJeu.Singleton.Limit_x;
    m_limit_z = SceneDataJeu.Singleton.Limit_z;
    m_limit_center = SceneDataJeu.Singleton.Limit_center;

    // if (m_player2 == true)
    // {
    //   m_spawn_position *= -1;
    //   Camera.main.transform.localEulerAngles = new Vector3(90, 180, 0);
    //   m_boundCheck = CheckBoundsPlayer2;
    //   m_deltaCheck = CheckDeltaPlayer2;
    // }
    // else
    // {
    //   m_boundCheck = CheckBoundsPlayer1;
    //   m_deltaCheck = CheckDeltaPlayer1;
    // }
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
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (!IsLocalPlayer) return;

    m_deltaCheck();
    m_boundCheck();
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
    // transform.position += new Vector3(mouseDelta.x, 0, mouseDelta.y);
    GetComponent<Rigidbody>().AddForce(new(mouseDelta.x, 0, mouseDelta.y));

    if (mouseDelta.magnitude < Mathf.Epsilon)
      GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
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