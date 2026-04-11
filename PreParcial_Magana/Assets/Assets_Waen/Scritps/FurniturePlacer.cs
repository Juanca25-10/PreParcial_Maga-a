using UnityEngine;

public class FurniturePlacer : MonoBehaviour
{
    [Header("Prefab del mueble")]
    [SerializeField] private GameObject mueblePrefab;

    public void ColocarMueble(Pose pose)
    {
        // Buscamos el precio en el prefab que vamos a instanciar
        FurnitureData info = mueblePrefab.GetComponent<FurnitureData>();

        if (info != null)
        {
            // 1. Validamos con el Manager ANTES de instanciar
            if (BudgetManager.Instance.PuedeComprar(info.precio))
            {
                GameObject nuevo = Instantiate(mueblePrefab, pose.position, pose.rotation);

                // 2. ¡ESTA LÍNEA ES CLAVE! Registra el gasto
                BudgetManager.Instance.RegistrarCompra(nuevo, info.precio);

                Debug.Log($"Se restaron ${info.precio}. Quedan: ${BudgetManager.Instance.presupuestoRestante}");
            }
            else
            {
                Debug.LogWarning("No tienes suficiente presupuesto para este mueble.");
            }
        }
    }
}
