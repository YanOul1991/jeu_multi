using UnityEngine;
using System.Collections;
using Unity.Netcode;
using TMPro;
using Unity.Netcode.Components;

public class Puck : NetworkBehaviour
{
    public static Puck instance; // Singleton
    //[SerializeField] private CamShaker camShaker;
    [SerializeField] private float thrust = 20f;
    private Rigidbody rb;
    public GameObject Goal1;
    public GameObject Goal2;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            return;
        }
        Destroy(gameObject);
    }

    void Update()
    {
        //if (!IsServer) return;
        //if (!GameManager.instance.partieEnCours) return;
    }

    void OnTriggerEnter(Collider collision)
    {
        if (!IsServer) return; 

        if (collision.gameObject == Goal1)
        {
            ScoreManager.instance.AugmenteHoteScore();
            LancePuckMilieu();
        }
        else if (collision.gameObject == Goal2)
        {
            ScoreManager.instance.AugmenteScoreClient();
            LancePuckMilieu();
        }
    }

    public void LancePuckMilieu()
    {
        GetComponent<NetworkTransform>().Interpolate = false;
        transform.position = new Vector3(0f, 0f, 0f);
        GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 0);
        //if (GameManager.instance.partieTerminee) return; // Il faudra créer cette variable dans le GameManager
        StartCoroutine(NouvellePuck());

    }

    IEnumerator NouvellePuck()
    {
        yield return new WaitForSecondsRealtime(1f);
        GetComponent<NetworkTransform>().Interpolate = true;

        rb = GetComponent<Rigidbody>();
        rb.AddForce(0, 0, thrust, ForceMode.Impulse);
    }
    
    // [Rpc(SendTo.Everyone)]
    // private void Shake_Rpc()
    // {
    //    if (camShaker == null) return;
    //    camShaker.Shake(0.2f, 0.2f, 20f);
    // }
}
