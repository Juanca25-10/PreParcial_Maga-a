using UnityEngine;

public class FurniturePlacer : MonoBehaviour
{
    [Header("Prefab del mueble")]
    [SerializeField] private GameObject mueblePrefab;

    // Este método se conecta al OnPlaneSelected del PlaneSelectionManager
    public void ColocarMueble(Pose pose)
    {
        Instantiate(mueblePrefab, pose.position, pose.rotation);
        Debug.Log("Mueble colocado en: " + pose.position);
    }
}
