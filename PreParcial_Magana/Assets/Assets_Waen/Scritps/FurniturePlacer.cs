using UnityEngine;

public class FurniturePlacer : MonoBehaviour
{
    [Header("Prefab del mueble")]
    [SerializeField] private GameObject mueblePrefab;

    public void ColocarMueble(Pose pose)
    {
        FurnitureData info = mueblePrefab.GetComponent<FurnitureData>();

        if (info != null && BudgetManager.Instance.PuedeComprar(info.precio))
        {
            GameObject nuevo = Instantiate(mueblePrefab, pose.position, pose.rotation);
            BudgetManager.Instance.RegistrarCompra(nuevo, info.precio);
        }
        else
        {
            Debug.LogWarning("¡No tienes plata o el prefab no tiene FurnitureData!");
        }
    }
}
