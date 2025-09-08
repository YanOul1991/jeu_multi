using UnityEngine;

public class Force : MonoBehaviour
{
    public float thrust = 20f;
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(0, 0, thrust, ForceMode.Impulse);
    }
}
