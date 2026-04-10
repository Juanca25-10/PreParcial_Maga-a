using UnityEngine;

public class FurnitureSelector : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private FurnitureInteraction interaction;
    [SerializeField] private Camera arCamera;

    [Header("Configuración de Selección")]
    [SerializeField] private LayerMask capaMuebles;
    [SerializeField] private float distanciaMaxima = 5f;

    private GameObject muebleMirandoActualmente = null;

    void Update()
    {
        Vector2 centroPantalla = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = arCamera.ScreenPointToRay(centroPantalla);

        if (Physics.Raycast(ray, out RaycastHit hit, distanciaMaxima, capaMuebles))
        {
            Transform root = hit.transform;
            while (root.parent != null && root.GetComponent<FurnitureInteraction>() == null)
            {
                root = root.parent;
            }

            GameObject muebleDetectado = root.gameObject;

            if (muebleMirandoActualmente != muebleDetectado)
            {
                interaction.SeleccionarMueble(muebleDetectado);
                muebleMirandoActualmente = muebleDetectado;
            }
        }
        else
        {
            if (muebleMirandoActualmente != null)
            {
                interaction.Deseleccionar();
                muebleMirandoActualmente = null;
            }
        }
    }
}