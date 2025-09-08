using UnityEngine;
using UnityEngine.InputSystem; // <- Required for new input system

public class MouseSpeedTracker : MonoBehaviour
{
    private Vector2 lastMousePosition;
    public float mouseSpeed;

    void Update()
    {
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();
        mouseSpeed = (currentMousePosition - lastMousePosition).magnitude / Time.deltaTime;
        lastMousePosition = currentMousePosition;
        if(mouseSpeed != 0)
        {
            if(mouseSpeed > 10000)
            {
                Debug.Log("<color=Purple>Mouse Speed: </color>"+mouseSpeed);
            }
            else if (mouseSpeed > 5000)
            {
                Debug.Log("<color=Red>Mouse Speed: </color>"+mouseSpeed);
            }
            else if (mouseSpeed > 1000)
            {
                Debug.Log("<color=Yellow>Mouse Speed: </color>"+mouseSpeed);
            }
            else
            {
                Debug.Log("<color=Green>Mouse Speed: </color>"+mouseSpeed);
            }
        }
    }
}
