using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem; // Necesario para Unity 6
using UnityEngine.XR.Interaction.Toolkit.AR;

public class InputManager : MonoBehaviour
{
    [Header("Configuración de AR")]
    [SerializeField] private GameObject furniturePrefab;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager; // Recomendado para estabilidad
    [SerializeField] private GameObject crosshair;

    [Header("Input")]
    [SerializeField] private InputActionProperty tapAction; // Referencia al toque de pantalla

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void OnEnable() => tapAction.action.Enable();
    private void OnDisable() => tapAction.action.Disable();

    void Update()
    {
        UpdateCrosshair();

        // Detectar si el usuario tocó la pantalla
        if (tapAction.action.WasPerformedThisFrame())
        {
            OnTapPerformed();
        }
    }

    private void UpdateCrosshair()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            crosshair.SetActive(true);
            crosshair.transform.position = hitPose.position;
            crosshair.transform.rotation = hitPose.rotation;
        }
        else
        {
            crosshair.SetActive(false);
        }
    }

    private void OnTapPerformed()
    {
        Vector2 touchPosition = Pointer.current.position.ReadValue();

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // 1. Crear un GameObject vacío para el anclaje
            GameObject anchorGO = new GameObject("Anchor_Cucarama");
            anchorGO.transform.position = hitPose.position;
            anchorGO.transform.rotation = hitPose.rotation;

            // 2. Agregar el componente ARAnchor (esto reemplaza a AddAnchor)
            // Al agregarlo, AR Foundation lo registra automáticamente en el subsistema
            anchorGO.AddComponent<ARAnchor>();

            // 3. Instanciar tu objeto y emparentarlo al anclaje
            GameObject placedObject = Instantiate(furniturePrefab, hitPose.position, hitPose.rotation);
            placedObject.transform.SetParent(anchorGO.transform, false);
        }
    }
}
