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

    public GameObject puckRespawnPlayer1;
    public GameObject puckRespawnPlayer2;


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
            PlacementPuckGoal(puckRespawnPlayer2.transform.position);
        }
        else if (collision.gameObject == Goal2)
        {
            ScoreManager.instance.AugmenteScoreClient();
            PlacementPuckGoal(puckRespawnPlayer1.transform.position);
        }
    }

    public void PlacementPuckGoal(Vector3 puckRespawnPosition)
    {
        PlacementPuck(puckRespawnPosition);
        if (GameManager.instance.partieTerminee) return; 
        StartCoroutine(NouvellePuck());
    }
    public void PlacementPuck(Vector3 puckRespawnPosition)
    {
        GetComponent<NetworkTransform>().Interpolate = false;
        transform.position = puckRespawnPosition;
        GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
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
