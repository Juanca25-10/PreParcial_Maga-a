using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class FurnitureInteraction : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARRaycastManager raycastManager;

    [Header("UI Panels")]
    [SerializeField] private GameObject panelInteraccion;
    [SerializeField] private GameObject subPanelMover;
    [SerializeField] private GameObject subPanelRotar;
    [SerializeField] private GameObject subPanelColor;

    [Header("Color")]
    [SerializeField] private UnityEngine.UI.Slider sliderR;
    [SerializeField] private UnityEngine.UI.Slider sliderG;
    [SerializeField] private UnityEngine.UI.Slider sliderB;
    [SerializeField] private UnityEngine.UI.Image previewColor;

    private GameObject muebleSeleccionado;

    // Velocidad de movimiento y rotacion
    private float velocidadMovimiento = 0.005f;
    private float velocidadRotacion = 60f;

    // Flags para mantener presionado
    private bool moviendoAdelante, moviendoAtras;
    private bool moviendoIzquierda, moviendoDerecha;
    private bool rotandoIzquierda, rotandoDerecha;

    void Start()
    {
        // Conectar sliders
        if (sliderR) sliderR.onValueChanged.AddListener((_) => ActualizarColor());
        if (sliderG) sliderG.onValueChanged.AddListener((_) => ActualizarColor());
        if (sliderB) sliderB.onValueChanged.AddListener((_) => ActualizarColor());
    }

    void Update()
    {
        if (muebleSeleccionado == null) return;

        // Movimiento continuo mientras se mantiene presionado
        if (moviendoAdelante)
            muebleSeleccionado.transform.Translate(Vector3.forward * velocidadMovimiento, Space.World);
        if (moviendoAtras)
            muebleSeleccionado.transform.Translate(Vector3.back * velocidadMovimiento, Space.World);
        if (moviendoIzquierda)
            muebleSeleccionado.transform.Translate(Vector3.left * velocidadMovimiento, Space.World);
        if (moviendoDerecha)
            muebleSeleccionado.transform.Translate(Vector3.right * velocidadMovimiento, Space.World);

        // Rotacion continua mientras se mantiene presionado
        if (rotandoIzquierda)
            muebleSeleccionado.transform.Rotate(Vector3.up, -velocidadRotacion * Time.deltaTime);
        if (rotandoDerecha)
            muebleSeleccionado.transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);
    }

    // ─── Seleccion ───────────────────────────────────────

    public void SeleccionarMueble(GameObject mueble)
    {
        muebleSeleccionado = mueble;
        panelInteraccion.SetActive(true);
        CerrarSubPaneles();
        Debug.Log("Mueble seleccionado: " + mueble.name);
    }

    public void Deseleccionar()
    {
        muebleSeleccionado = null;
        panelInteraccion.SetActive(false);
        CerrarSubPaneles();
        DetenerTodo();
    }

    // ─── Navegacion de subpaneles ─────────────────────────

    public void AbrirPanelMover()
    {
        CerrarSubPaneles();
        subPanelMover.SetActive(true);
    }

    public void AbrirPanelRotar()
    {
        CerrarSubPaneles();
        subPanelRotar.SetActive(true);
    }

    public void AbrirPanelColor()
    {
        CerrarSubPaneles();
        subPanelColor.SetActive(true);
    }

    private void CerrarSubPaneles()
    {
        DetenerTodo();
        if (subPanelMover) subPanelMover.SetActive(false);
        if (subPanelRotar) subPanelRotar.SetActive(false);
        if (subPanelColor) subPanelColor.SetActive(false);
    }

    // ─── Botones de movimiento (PointerDown / PointerUp) ──

    public void PresionarAdelante(bool estado) => moviendoAdelante = estado;
    public void PresionarAtras(bool estado) => moviendoAtras = estado;
    public void PresionarIzquierda(bool estado) => moviendoIzquierda = estado;
    public void PresionarDerecha(bool estado) => moviendoDerecha = estado;

    // ─── Botones de rotacion ──────────────────────────────

    public void PresionarRotarIzquierda(bool estado) => rotandoIzquierda = estado;
    public void PresionarRotarDerecha(bool estado) => rotandoDerecha = estado;

    private void DetenerTodo()
    {
        moviendoAdelante = moviendoAtras = false;
        moviendoIzquierda = moviendoDerecha = false;
        rotandoIzquierda = rotandoDerecha = false;
    }

    // ─── Color ────────────────────────────────────────────

    private void ActualizarColor()
    {
        if (muebleSeleccionado == null) return;

        Color color = new Color(sliderR.value, sliderG.value, sliderB.value);

        if (previewColor) previewColor.color = color;

        Renderer[] renderers = muebleSeleccionado.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;
            foreach (Material mat in mats)
                mat.color = color;
            r.materials = mats;
        }
    }
}