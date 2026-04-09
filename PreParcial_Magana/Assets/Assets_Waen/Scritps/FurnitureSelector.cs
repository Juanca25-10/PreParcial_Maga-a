using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FurnitureSelector : MonoBehaviour
{
    [SerializeField] private FurnitureInteraction interaction;
    [SerializeField] private Camera arCamera;

    private bool modoSeleccion = false;

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Update()
    {
        if (!modoSeleccion) return;
        if (Touch.activeTouches.Count == 0) return;

        Touch toque = Touch.activeTouches[0];
        if (toque.phase != UnityEngine.InputSystem.TouchPhase.Began) return;

        Ray ray = arCamera.ScreenPointToRay(toque.screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform root = hit.transform;
            while (root.parent != null)
                root = root.parent;

            interaction.SeleccionarMueble(root.gameObject);
            modoSeleccion = false;
        }
    }

    public void ActivarModoSeleccion() => modoSeleccion = true;
    public void Cancelar() => modoSeleccion = false;
}
