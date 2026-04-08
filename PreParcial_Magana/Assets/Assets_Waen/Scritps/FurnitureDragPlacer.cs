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

    private GameObject previewInstance;
    private PlacementPreview previewController;
    private GameObject prefabActual;
    private bool modoColocacion = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Evita que el toque del menu tambien coloque el mueble
    private int frameSeleccion = -1;
    private const int FRAMES_ESPERA = 10;

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

        // Espera unos frames antes de aceptar toque de colocacion
        if (Time.frameCount - frameSeleccion > FRAMES_ESPERA)
        {
            DetectarToqueColocacion();
        }
    }

    private void ActualizarPreviewCentro()
    {
        Vector2 centroPantalla = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(centroPantalla, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            previewInstance.transform.position = hitPose.position;
            previewInstance.transform.rotation = hitPose.rotation;
            previewController.SetValido(true);
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
        else
        {
            Debug.Log("Apunta al suelo para colocar");
        }
    }

    public void SeleccionarMueble(GameObject prefab)
    {
        Debug.Log("SeleccionarMueble llamado: " + prefab.name);

        if (previewInstance != null)
            Destroy(previewInstance);

        prefabActual = prefab;
        previewInstance = Instantiate(prefab);

        previewController = previewInstance.GetComponent<PlacementPreview>();
        if (previewController == null)
            previewController = previewInstance.AddComponent<PlacementPreview>();

        previewController.Inicializar(materialValido, materialInvalido);

        modoColocacion = true;
        frameSeleccion = Time.frameCount;

        Debug.Log("Preview creado, mueve el celular y toca para colocar");
    }

    private void ConfirmarColocacion()
    {
        Instantiate(prefabActual,
            previewInstance.transform.position,
            previewInstance.transform.rotation);

        Debug.Log("Mueble colocado en: " + previewInstance.transform.position);

        Destroy(previewInstance);
        previewInstance = null;
        modoColocacion = false;
    }
}
