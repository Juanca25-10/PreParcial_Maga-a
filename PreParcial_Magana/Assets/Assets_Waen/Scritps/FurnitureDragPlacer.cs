using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FurnitureDragPlacer : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARRaycastManager raycastManager;

    [Header("Materiales de feedback")]
    [SerializeField] private Material materialValido;
    [SerializeField] private Material materialInvalido;

    [Header("UI")]
    [SerializeField] private GameObject instruccionUI; // Texto "Mueve el celular y toca para colocar"

    private GameObject previewInstance;
    private PlacementPreview previewController;
    private GameObject prefabActual;
    private bool modoColocacion = false;
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
        if (!modoColocacion || previewInstance == null) return;

        ActualizarPreviewCentro();
        DetectarToqueColocacion();
    }

    // El preview sigue el centro de la pantalla (donde apunta la camara)
    private void ActualizarPreviewCentro()
    {
        Vector2 centroPantalla = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(centroPantalla, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            previewInstance.transform.position = hitPose.position;
            previewInstance.transform.rotation = hitPose.rotation;
            previewController.SetValido(!HaySolapamiento());
        }
        else
        {
            previewController.SetValido(false);
        }
    }

    private void DetectarToqueColocacion()
    {
        if (Touch.activeTouches.Count == 0) return;

        Touch toque = Touch.activeTouches[0];
        if (toque.phase != UnityEngine.InputSystem.TouchPhase.Began) return;

        if (previewController.EsValido())
        {
            ConfirmarColocacion();
        }
    }

    // Llamado desde FurnitureMenuUI al tocar un item
    public void SeleccionarMueble(GameObject prefab)
    {
        if (previewInstance != null)
            Destroy(previewInstance);

        prefabActual = prefab;
        previewInstance = Instantiate(prefab);

        previewController = previewInstance.GetComponent<PlacementPreview>();
        if (previewController == null)
            previewController = previewInstance.AddComponent<PlacementPreview>();

        previewController.Inicializar(materialValido, materialInvalido);

        modoColocacion = true;

        if (instruccionUI != null)
            instruccionUI.SetActive(true);

        Debug.Log("Mueble seleccionado, apunta al suelo y toca para colocar");
    }

    private void ConfirmarColocacion()
    {
        Instantiate(prefabActual,
            previewInstance.transform.position,
            previewInstance.transform.rotation);

        Debug.Log("Mueble colocado");

        Destroy(previewInstance);
        previewInstance = null;
        modoColocacion = false;

        if (instruccionUI != null)
            instruccionUI.SetActive(false);
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
