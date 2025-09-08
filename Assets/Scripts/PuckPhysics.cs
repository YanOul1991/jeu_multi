using UnityEngine;

public class PuckPhysics : MonoBehaviour
{
    
    void Start()
    {
        
    }


    void Update()
    {
        
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            Debug.Log("Puck Hit a Player");
        }
    }
}
