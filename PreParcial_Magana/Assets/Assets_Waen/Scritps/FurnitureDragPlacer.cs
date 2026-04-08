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
    private bool arrastrando = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private int touchId = -1;

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += OnFingerDown;
        Touch.onFingerMove += OnFingerMove;
        Touch.onFingerUp += OnFingerUp;
    }

    void OnDisable()
    {
        Touch.onFingerDown -= OnFingerDown;
        Touch.onFingerMove -= OnFingerMove;
        Touch.onFingerUp -= OnFingerUp;
        EnhancedTouchSupport.Disable();
    }

    private void OnFingerDown(Finger finger)
    {
        if (!arrastrando) return;
        if (touchId != -1) return;

        touchId = finger.index;
        ActualizarPreview(finger.currentTouch.screenPosition);
    }

    private void OnFingerMove(Finger finger)
    {
        if (!arrastrando) return;
        if (finger.index != touchId) return;

        ActualizarPreview(finger.currentTouch.screenPosition);
    }

    private void OnFingerUp(Finger finger)
    {
        if (!arrastrando) return;
        if (finger.index != touchId) return;

        FinalizarArrastre();
        touchId = -1;
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
        touchId = -1;

        Debug.Log("Arrastre iniciado, mueve el dedo sobre el plano");
    }

    private void ActualizarPreview(Vector2 posicionPantalla)
    {
        if (raycastManager.Raycast(posicionPantalla, hits, TrackableType.PlaneWithinPolygon))
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
