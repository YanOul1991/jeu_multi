using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
  private UserInputs inputs;
  [field: SerializeField] private Transform m_limit_x;
  [field: SerializeField] private Transform m_limit_z;
  [field: SerializeField] private Transform m_limit_center;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void Awake()
  {
    inputs = new UserInputs();
    Application.targetFrameRate = -1;
  }
  
  private void Start()
  {
    Debug.Log("Game Start");
    Cursor.lockState = CursorLockMode.Confined;
    Cursor.visible = false;
  }

  private void OnEnable()
  {
    inputs.Enable();
  }

  private void OnDisable()
  {
    inputs.Disable();
  }

  // Update is called once per frame
  void Update()
  {
    Vector2 mouseDelta = inputs.MapMain.Look.ReadValue<Vector2>();
    mouseDelta *= 1000f;
    // transform.position += new Vector3(mouseDelta.x, 0, mouseDelta.y);
    GetComponent<Rigidbody>().AddForce(new(mouseDelta.x, 0, mouseDelta.y));

    if (mouseDelta.magnitude < Mathf.Epsilon)
    {
      GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
    }
    
    if (transform.position.x > m_limit_x.transform.position.x)
    {
      transform.position = new(m_limit_x.transform.position.x, 0, transform.position.z);
    }

    if (transform.position.x < -m_limit_x.transform.position.x)
    {
      transform.position = new(-m_limit_x.transform.position.x, 0, transform.position.z);
    }

    if (transform.position.z < m_limit_z.transform.position.z)
    {
      transform.position = new(transform.position.x, 0, m_limit_z.transform.position.z);
    }

    if (transform.position.z > m_limit_center.transform.position.z)
    {
      transform.position = new(transform.position.x, 0, m_limit_center.transform.position.z);
    }
  }
}