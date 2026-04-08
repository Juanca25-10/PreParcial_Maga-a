using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

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

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        if (!arrastrando || previewInstance == null) return;
        if (Touch.activeTouches.Count == 0) return;

        Touch toque = Touch.activeTouches[0];

        switch (toque.phase)
        {
            case UnityEngine.InputSystem.TouchPhase.Moved:
            case UnityEngine.InputSystem.TouchPhase.Stationary:
                ActualizarPreview(toque.screenPosition);
                break;

            case UnityEngine.InputSystem.TouchPhase.Ended:
            case UnityEngine.InputSystem.TouchPhase.Canceled:
                FinalizarArrastre();
                break;
        }
    }

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
