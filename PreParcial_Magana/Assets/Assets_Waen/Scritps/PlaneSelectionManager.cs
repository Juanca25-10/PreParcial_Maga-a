using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class PlaneSelectionManager : MonoBehaviour
{
    [Header("Componentes AR")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Reticle")]
    [SerializeField] private GameObject reticle;

    [Header("UI")]
    [SerializeField] private GameObject tapToPlaceUI;

    [Header("Evento al seleccionar plano")]
    public UnityEvent<Pose> OnPlaneSelected;

    private bool planeSeleccionado = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Pose posicionSeleccionada;

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        if (planeSeleccionado) return;

        ActualizarReticle();
        DetectarToque();
    }

    private void ActualizarReticle()
    {
        Vector2 centroPantalla = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(centroPantalla, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            if (!PlaneValidator.EsPlanoHorizontal(hitPose))
            {
                OcultarReticle();
                return;
            }

            reticle.SetActive(true);
            reticle.transform.position = hitPose.position;
            reticle.transform.rotation = hitPose.rotation;

            posicionSeleccionada = hitPose;

            if (tapToPlaceUI != null)
                tapToPlaceUI.SetActive(true);
        }
        else
        {
            OcultarReticle();
        }
    }

    private void OcultarReticle()
    {
        reticle.SetActive(false);
        if (tapToPlaceUI != null)
            tapToPlaceUI.SetActive(false);
    }

    private void DetectarToque()
    {
        if (Touch.activeTouches.Count == 0) return;
        Touch toque = Touch.activeTouches[0];
        if (toque.phase != UnityEngine.InputSystem.TouchPhase.Began) return;
        if (!reticle.activeSelf) return;

        ConfirmarPlano();
    }

    private void ConfirmarPlano()
    {
        planeSeleccionado = true;
        OcultarReticle();
        OcultarTodosLosPlanos();
        planeManager.enabled = false;

        OnPlaneSelected?.Invoke(posicionSeleccionada);
        Debug.Log($"Plano seleccionado en: {posicionSeleccionada.position}");
    }

    private void OcultarTodosLosPlanos()
    {
        foreach (ARPlane plano in planeManager.trackables)
        {
            plano.gameObject.SetActive(false);
        }
    }

    public void ReiniciarSeleccion()
    {
        planeSeleccionado = false;
        planeManager.enabled = true;
        if (tapToPlaceUI != null) tapToPlaceUI.SetActive(false);
    }
}