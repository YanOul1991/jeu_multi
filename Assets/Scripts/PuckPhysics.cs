using Unity.Netcode;
using UnityEngine;

public class PuckPhysics : NetworkBehaviour
{
  public static PuckPhysics Singleton;
  private ulong m_lastPlayerHit;

  private void Awake()
  {
    if (Singleton == null)
      Singleton = this;
    else
      Destroy(gameObject);
  }
  
  void OnCollisionEnter(Collision collision)
  {
    if (!IsServer) return;

    if (collision.gameObject.CompareTag("Player"))
    {
      m_lastPlayerHit = collision.gameObject.GetComponent<NetworkObject>().NetworkObjectId;
    }

    if (collision.gameObject.CompareTag("Powerup"))
    {
      PowerupManager.Singleton.NetworkPowerupHit_Rpc(
        m_lastPlayerHit,
        collision.transform.parent.GetComponent<NetworkObject>().NetworkObjectId
      );
    }
  }
}