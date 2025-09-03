using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private UserInputs inputs;
    [field: SerializeField] TextMeshProUGUI txt;
    [field: SerializeField] Camera cam;
    [field: SerializeField] LayerMask mouseMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        inputs = new UserInputs();
        Application.targetFrameRate = -1;
    }

    private void Start()
    {
        Debug.Log("Game Start");
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
        // Vector2 mousePosition = inputs.MapMain.Look.ReadValue<Vector2>();
        // transform.position = new Vector3(mousePosition.x, 0.5f, mousePosition.y);
        // txt.text = "Mouse position: " + inputs.MapMain.Look.ReadValue<Vector2>();
    }

    private void OnCollisionEnter(Collision collision)
    {
    }

    private void FixedUpdate()
    {
        Vector2 mousePosition = inputs.MapMain.Look.ReadValue<Vector2>();
        Vector3 mouseToWorld = Camera.main.ScreenToWorldPoint(new Vector3(
            mousePosition.x,
            mousePosition.y,
            Camera.main.transform.position.y));

        txt.text = "Hit position: " + mouseToWorld;

        transform.position = mouseToWorld;
    }
}
