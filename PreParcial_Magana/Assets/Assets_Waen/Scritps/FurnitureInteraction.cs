using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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
    [SerializeField] private Slider sliderR;
    [SerializeField] private Slider sliderG;
    [SerializeField] private Slider sliderB;
    [SerializeField] private Image previewColor;

    [Header("Camara")]
    [SerializeField] private Camera arCamera;

    [Header("World Space UI")]
    [SerializeField] private WorldSpaceUIController worldUI;

    [Header("Efecto de Seleccion")]
    [SerializeField] private Color colorResalte = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private float mezclaBaseResalte = 0.18f;
    [SerializeField] private float emisionResalte = 1.8f;

    private readonly List<ARRaycastHit> hitsMovimiento = new List<ARRaycastHit>();
    private readonly List<MaterialVisualState> estadosMateriales = new List<MaterialVisualState>();

    private GameObject muebleSeleccionado;

    private float velocidadMovimiento = 0.5f;
    private float velocidadRotacion = 100f;

    private bool moviendoAdelante;
    private bool moviendoAtras;
    private bool moviendoIzquierda;
    private bool moviendoDerecha;
    private bool rotandoIzquierda;
    private bool rotandoDerecha;

    private class MaterialVisualState
    {
        public Material material;
        public string colorProperty;
        public Color baseColor;
        public Color emissionColor;
        public bool soportaEmision;
    }

    private void Start()
    {
        if (sliderR) sliderR.onValueChanged.AddListener((_) => ActualizarColor());
        if (sliderG) sliderG.onValueChanged.AddListener((_) => ActualizarColor());
        if (sliderB) sliderB.onValueChanged.AddListener((_) => ActualizarColor());
    }

    private void Update()
    {
        if (muebleSeleccionado == null) return;

        Vector3 posicionAntes = muebleSeleccionado.transform.position;
        Vector3 movimiento = Vector3.zero;

        if (moviendoAdelante) movimiento += arCamera.transform.forward;
        if (moviendoAtras) movimiento -= arCamera.transform.forward;
        if (moviendoIzquierda) movimiento -= arCamera.transform.right;
        if (moviendoDerecha) movimiento += arCamera.transform.right;

        movimiento.y = 0f;

        if (movimiento != Vector3.zero)
        {
            muebleSeleccionado.transform.position += movimiento.normalized * velocidadMovimiento * Time.deltaTime;

            if (!PosicionEsValida(muebleSeleccionado.transform.position))
            {
                muebleSeleccionado.transform.position = posicionAntes;
            }
        }

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
        FurnitureRenderSetup.Configurar(muebleSeleccionado);
        CapturarEstadoVisualActual();
        SincronizarPreviewConMaterial();

        CerrarSubPaneles();
        worldUI.Mostrar(mueble.transform);
        AplicarVisualSeleccionado();
    }

    public void Deseleccionar()
    {
        if (muebleSeleccionado != null)
        {
            RestaurarEstadoVisualBase();
        }

        muebleSeleccionado = null;
        estadosMateriales.Clear();
        worldUI.Ocultar();
        CerrarSubPaneles();
        DetenerTodo();
    }

    private void CapturarEstadoVisualActual()
    {
        estadosMateriales.Clear();

        Renderer[] renderers = muebleSeleccionado.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material == null) continue;

                string colorProperty = ObtenerPropiedadColor(material);
                bool soportaEmision = material.HasProperty("_EmissionColor");
                if (colorProperty == null && !soportaEmision) continue;

                estadosMateriales.Add(new MaterialVisualState
                {
                    material = material,
                    colorProperty = colorProperty,
                    baseColor = ObtenerColorBase(material, colorProperty),
                    emissionColor = ObtenerColorEmision(material),
                    soportaEmision = soportaEmision
                });
            }
        }
    }

    private void AplicarVisualSeleccionado()
    {
        Color resalteIntenso = colorResalte;

        foreach (MaterialVisualState estado in estadosMateriales)
        {
            if (estado.material == null) continue;

            if (estado.colorProperty != null)
            {
                Color colorVisible = Color.Lerp(estado.baseColor, resalteIntenso, mezclaBaseResalte);
                estado.material.SetColor(estado.colorProperty, colorVisible);
            }

            if (estado.soportaEmision)
            {
                Color emisionVisible = estado.emissionColor + (resalteIntenso * emisionResalte);
                estado.material.EnableKeyword("_EMISSION");
                estado.material.SetColor("_EmissionColor", emisionVisible);
            }
        }
    }

    private void RestaurarEstadoVisualBase()
    {
        foreach (MaterialVisualState estado in estadosMateriales)
        {
            if (estado.material == null) continue;

            if (estado.colorProperty != null)
            {
                estado.material.SetColor(estado.colorProperty, estado.baseColor);
            }

            if (estado.soportaEmision)
            {
                estado.material.SetColor("_EmissionColor", estado.emissionColor);
            }
        }
    }

    private void ActualizarColor()
    {
        if (muebleSeleccionado == null) return;

        Color colorUI = new Color(sliderR.value, sliderG.value, sliderB.value, 1f);

        if (previewColor) previewColor.color = colorUI;

        foreach (MaterialVisualState estado in estadosMateriales)
        {
            if (estado.material == null) continue;

            if (estado.colorProperty != null)
            {
                estado.baseColor = colorUI;
            }
        }

        AplicarVisualSeleccionado();
    }

    private bool PosicionEsValida(Vector3 posicion)
    {
        if (Application.isEditor) return true;

        Vector2 posicionPantalla = arCamera.WorldToScreenPoint(posicion);
        return raycastManager.Raycast(posicionPantalla, hitsMovimiento, TrackableType.PlaneWithinPolygon);
    }

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
        subPanelMover.SetActive(false);
        subPanelRotar.SetActive(false);
        subPanelColor.SetActive(false);
    }

    public void PresionarAdelante(bool e) => moviendoAdelante = e;
    public void PresionarAtras(bool e) => moviendoAtras = e;
    public void PresionarIzquierda(bool e) => moviendoIzquierda = e;
    public void PresionarDerecha(bool e) => moviendoDerecha = e;
    public void PresionarRotarIzquierda(bool e) => rotandoIzquierda = e;
    public void PresionarRotarDerecha(bool e) => rotandoDerecha = e;

    private void DetenerTodo()
    {
        moviendoAdelante = false;
        moviendoAtras = false;
        moviendoIzquierda = false;
        moviendoDerecha = false;
        rotandoIzquierda = false;
        rotandoDerecha = false;
    }

    private void SincronizarPreviewConMaterial()
    {
        if (estadosMateriales.Count == 0) return;

        Color colorActual = estadosMateriales[0].baseColor;

        if (sliderR) sliderR.SetValueWithoutNotify(colorActual.r);
        if (sliderG) sliderG.SetValueWithoutNotify(colorActual.g);
        if (sliderB) sliderB.SetValueWithoutNotify(colorActual.b);
        if (previewColor) previewColor.color = colorActual;
    }

    private static string ObtenerPropiedadColor(Material material)
    {
        if (material.HasProperty("_BaseColor")) return "_BaseColor";
        if (material.HasProperty("_Color")) return "_Color";
        return null;
    }

    private static Color ObtenerColorBase(Material material, string colorProperty)
    {
        if (colorProperty == null) return Color.white;
        return material.GetColor(colorProperty);
    }

    private static Color ObtenerColorEmision(Material material)
    {
        if (!material.HasProperty("_EmissionColor")) return Color.black;
        return material.GetColor("_EmissionColor");
    }
}
