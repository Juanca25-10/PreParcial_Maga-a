using UnityEngine;

public class FurnitureSelector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private FurnitureInteraction interaction;
    [SerializeField] private Camera arCamera;

    [Header("Configuración de Selección")]
    [SerializeField] private LayerMask capaMuebles;
    [SerializeField] private float distanciaMaxima = 25f;

    private GameObject muebleMirandoActualmente = null;

    void Update()
    {
        Vector2 centroPantalla = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = arCamera.ScreenPointToRay(centroPantalla);

        if (Physics.Raycast(ray, out RaycastHit hit, distanciaMaxima, capaMuebles))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);

            // Obtenemos el objeto que tocamos (el comedor, la silla, etc.)
            GameObject muebleDetectado = hit.transform.gameObject;

            if (muebleMirandoActualmente != muebleDetectado)
            {
                Debug.Log($"<color=cyan>Selector:</color> Raycast tocó {muebleDetectado.name}. Avisando al Manager.");

                // LLAMADA DIRECTA AL MANAGER (AR_Managers)
                interaction.SeleccionarMueble(muebleDetectado);
                muebleMirandoActualmente = muebleDetectado;
            }
        }
        else
        {
            Debug.DrawRay(ray.origin, ray.direction * distanciaMaxima, Color.red);
            if (muebleMirandoActualmente != null)
            {
                interaction.Deseleccionar();
                muebleMirandoActualmente = null;
            }
        }
    }
}