using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class FurnitureDragPlacer : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARRaycastManager raycastManager;

    [Header("Materiales de feedback")]
    [SerializeField] private Material materialValido;
    [SerializeField] private Material materialInvalido;

    private GameObject previewInstance;
    private PlacementPreview previewController;
    private GameObject prefabActual;
    private bool arrastrando = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (!arrastrando || previewInstance == null) return;
        if (Input.touchCount == 0) return;

        Touch toque = Input.GetTouch(0);

        switch (toque.phase)
        {
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                ActualizarPreview(toque.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                FinalizarArrastre();
                break;
        }
    }

    // Llamado desde FurnitureMenuUI cuando el usuario toca un item del menu
    public void IniciarArrastre(GameObject prefab)
    {
        if (previewInstance != null)
            Destroy(previewInstance);

        prefabActual = prefab;
        previewInstance = Instantiate(prefab);

        previewController = previewInstance.GetComponent<PlacementPreview>();
        if (previewController == null)
            previewController = previewInstance.AddComponent<PlacementPreview>();

        previewController.Inicializar(materialValido, materialInvalido);
        arrastrando = true;
    }

    private void ActualizarPreview(Vector2 posicionPantalla)
    {
        // Si el dedo está sobre UI no raycasteamos
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
        {
            previewController.SetValido(false);
            return;
        }

        if (raycastManager.Raycast(posicionPantalla, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            previewInstance.transform.position = hitPose.position;
            previewInstance.transform.rotation = hitPose.rotation;

            bool sinSolapamiento = !HaySolapamiento();
            previewController.SetValido(sinSolapamiento);
        }
        else
        {
            previewController.SetValido(false);
        }
    }

    private bool HaySolapamiento()
    {
        Bounds bounds = CalcularBounds(previewInstance);

        Collider[] colliders = Physics.OverlapBox(
            bounds.center,
            bounds.extents * 0.85f,
            previewInstance.transform.rotation
        );

        foreach (var col in colliders)
        {
            if (col.gameObject != previewInstance &&
                !col.transform.IsChildOf(previewInstance.transform))
                return true;
        }

        return false;
    }

    private void FinalizarArrastre()
    {
        if (previewController != null && previewController.EsValido())
        {
            Instantiate(prefabActual,
                previewInstance.transform.position,
                previewInstance.transform.rotation);

            Debug.Log("Mueble colocado correctamente");
        }
        else
        {
            Debug.Log("Posicion invalida, objeto no colocado");
        }

        Destroy(previewInstance);
        previewInstance = null;
        arrastrando = false;
    }

    private Bounds CalcularBounds(GameObject obj)
    {
        Renderer[] rs = obj.GetComponentsInChildren<Renderer>();
        if (rs.Length == 0) return new Bounds(obj.transform.position, Vector3.one);

        Bounds bounds = rs[0].bounds;
        foreach (var r in rs)
            bounds.Encapsulate(r.bounds);

        return bounds;
    }
}
