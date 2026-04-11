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

    private List<ARRaycastHit> hitsMovimiento = new List<ARRaycastHit>();
    private Vector3 ultimaPosicionValida;

    [Header("Camara")]
    [SerializeField] private Camera arCamera;

    [Header("World Space UI")]
    [SerializeField] private WorldSpaceUIController worldUI;
    [Header("Efecto de Selección")]
    [SerializeField] private Color colorResalte = new Color(0.2f, 0.6f, 1f, 1f);

    private Dictionary<Material, Color> coloresOriginales = new Dictionary<Material, Color>();
    private GameObject muebleSeleccionado;

    // Velocidad aumentada para pruebas en PC
    private float velocidadMovimiento = 0.5f;
    private float velocidadRotacion = 100f;

    private bool moviendoAdelante, moviendoAtras, moviendoIzquierda, moviendoDerecha;
    private bool rotandoIzquierda, rotandoDerecha;

    void Start()
    {
        if (sliderR) sliderR.onValueChanged.AddListener((_) => ActualizarColor());
        if (sliderG) sliderG.onValueChanged.AddListener((_) => ActualizarColor());
        if (sliderB) sliderB.onValueChanged.AddListener((_) => ActualizarColor());
    }

    void Update()
    {
        if (muebleSeleccionado == null) return;

        Vector3 posicionAntes = muebleSeleccionado.transform.position;
        Vector3 movimiento = Vector3.zero;

        // --- CORRECCIÓN DE DIRECCIONES ---
        // Adelante (Arriba en UI) -> Aleja del usuario
        if (moviendoAdelante) movimiento += arCamera.transform.forward;
        // Atrás (Abajo en UI) -> Acerca al usuario
        if (moviendoAtras) movimiento -= arCamera.transform.forward;
        // Izquierda (Izquierda en UI) -> Mueve a la izquierda de la pantalla
        if (moviendoIzquierda) movimiento -= arCamera.transform.right;
        // Derecha (Derecha en UI) -> Mueve a la derecha de la pantalla
        if (moviendoDerecha) movimiento += arCamera.transform.right;

        // Bloqueamos el eje Y para que no flote ni se hunda
        movimiento.y = 0;

        if (movimiento != Vector3.zero)
        {
            // Aplicamos el movimiento normalizado para que no vaya más rápido en diagonal
            muebleSeleccionado.transform.position += movimiento.normalized * velocidadMovimiento * Time.deltaTime;

            // Verificación de posición (AR o Editor)
            if (!PosicionEsValida(muebleSeleccionado.transform.position))
            {
                muebleSeleccionado.transform.position = posicionAntes;
            }
        }

        // Rotación (Esta suele estar bien, pero asegúrate de que Space.Self sea lo que buscas)
        if (rotandoIzquierda)
            muebleSeleccionado.transform.Rotate(Vector3.up, -velocidadRotacion * Time.deltaTime);
        if (rotandoDerecha)
            muebleSeleccionado.transform.Rotate(Vector3.up, velocidadRotacion * Time.deltaTime);
    }

    public void SeleccionarMueble(GameObject mueble)
    {
        if (muebleSeleccionado == mueble) return;
        if (muebleSeleccionado != null) Deseleccionar();
        muebleSeleccionado = mueble;

        CerrarSubPaneles();
        worldUI.Mostrar(mueble.transform);
        AplicarResalteAzul();
    }

    public void Deseleccionar()
    {
        if (muebleSeleccionado != null) RestaurarColoresOriginales();
        muebleSeleccionado = null;
        worldUI.Ocultar();
        CerrarSubPaneles();
        DetenerTodo();
    }

    private void AplicarResalteAzul()
    {
        coloresOriginales.Clear();
        Renderer[] renderers = muebleSeleccionado.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_Color") || mat.HasProperty("_BaseColor"))
                {
                    coloresOriginales[mat] = mat.color;
                    mat.color = colorResalte;
                }
            }
        }
    }

    private void RestaurarColoresOriginales()
    {
        foreach (var entry in coloresOriginales)
        {
            if (entry.Key != null) entry.Key.color = entry.Value;
        }
    }

    private void ActualizarColor()
    {
        if (muebleSeleccionado == null) return;
        Color nuevoColor = new Color(sliderR.value, sliderG.value, sliderB.value);
        if (previewColor) previewColor.color = nuevoColor;

        Renderer[] renderers = muebleSeleccionado.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                if (mat.HasProperty("_Color") || mat.HasProperty("_BaseColor"))
                {
                    mat.color = nuevoColor;
                    // Actualizamos el diccionario para que el color persista al deseleccionar
                    if (coloresOriginales.ContainsKey(mat)) coloresOriginales[mat] = nuevoColor;
                }
            }
        }
    }

    private bool PosicionEsValida(Vector3 posicion)
    {
        if (Application.isEditor) return true; // Permitir movimiento libre en PC

        Vector2 posicionPantalla = arCamera.WorldToScreenPoint(posicion);
        return raycastManager.Raycast(posicionPantalla, hitsMovimiento, TrackableType.PlaneWithinPolygon);
    }

    // Navegación y Botones
    public void AbrirPanelMover() { CerrarSubPaneles(); subPanelMover.SetActive(true); }
    public void AbrirPanelRotar() { CerrarSubPaneles(); subPanelRotar.SetActive(true); }
    public void AbrirPanelColor() { CerrarSubPaneles(); subPanelColor.SetActive(true); }
    private void CerrarSubPaneles() { DetenerTodo(); subPanelMover.SetActive(false); subPanelRotar.SetActive(false); subPanelColor.SetActive(false); }
    public void PresionarAdelante(bool e) => moviendoAdelante = e;
    public void PresionarAtras(bool e) => moviendoAtras = e;
    public void PresionarIzquierda(bool e) => moviendoIzquierda = e;
    public void PresionarDerecha(bool e) => moviendoDerecha = e;
    public void PresionarRotarIzquierda(bool e) => rotandoIzquierda = e;
    public void PresionarRotarDerecha(bool e) => rotandoDerecha = e;
    private void DetenerTodo() => moviendoAdelante = moviendoAtras = moviendoIzquierda = moviendoDerecha = rotandoIzquierda = rotandoDerecha = false;
}