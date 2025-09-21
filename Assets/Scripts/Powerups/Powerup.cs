using UnityEngine;

public class Powerup : MonoBehaviour
{
  private Mesh m_mesh;
  private Bounds m_bounds;
  [field: SerializeField] public float SizeX { get; private set; }
  [field: SerializeField] public float SizeZ { get; private set; }

  private void Awake()
  {
    m_mesh = GetComponent<MeshFilter>().mesh;
    m_bounds = m_mesh.bounds;
    SizeX = m_bounds.size.x;
    SizeZ = m_bounds.size.z;
  }
}
