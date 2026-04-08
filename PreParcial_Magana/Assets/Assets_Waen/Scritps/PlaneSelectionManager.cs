using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Events;

public class PlaneSelectionManager : MonoBehaviour
{
    [Header("Componentes AR")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;

    [Header("Reticle")]
    [SerializeField] private GameObject reticle;

    [Header("UI")]
    [SerializeField] private GameObject tapToPlaceUI; // texto "Toca para seleccionar"

    [Header("Evento al seleccionar plano")]
    public UnityEvent<Pose> OnPlaneSelected;

    private bool planeSeleccionado = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Pose posicionSeleccionada;

    void Update()
    {
        if (planeSeleccionado) return;

        ActualizarReticle();
        DetectarToque();
    }

    // Mueve el reticle al centro de la pantalla cada frame
    private void ActualizarReticle()
    {
        Vector2 centroPantalla = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(centroPantalla, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            reticle.SetActive(true);
            reticle.transform.position = hitPose.position;
            reticle.transform.rotation = hitPose.rotation;

            posicionSeleccionada = hitPose;

            if (tapToPlaceUI != null)
                tapToPlaceUI.SetActive(true);
        }
        else
        {
            reticle.SetActive(false);

            if (tapToPlaceUI != null)
                tapToPlaceUI.SetActive(false);
        }
    }

    // Detecta el toque del usuario para confirmar el plano
    private void DetectarToque()
    {
        if (Input.touchCount == 0) return;

        Touch toque = Input.GetTouch(0);
        if (toque.phase != TouchPhase.Began) return;

        // Solo confirma si el reticle está visible (hay plano válido)
        if (!reticle.activeSelf) return;

        ConfirmarPlano();
    }

    private void ConfirmarPlano()
    {
        planeSeleccionado = true;

        // Ocultar el reticle
        reticle.SetActive(false);

        // Ocultar UI
        if (tapToPlaceUI != null)
            tapToPlaceUI.SetActive(false);

        // Desactivar visualización de todos los planos detectados
        OcultarTodosLosPlanos();

        // Detener detección de nuevos planos
        planeManager.enabled = false;

        // Notificar al resto del sistema con la posición confirmada
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

    // Método público para reiniciar la selección si el usuario quiere cambiar el plano
    public void ReiniciarSeleccion()
    {
        planeSeleccionado = false;
        planeManager.enabled = true;

        if (tapToPlaceUI != null)
            tapToPlaceUI.SetActive(false);

        Debug.Log("Selección de plano reiniciada");
    }
}
